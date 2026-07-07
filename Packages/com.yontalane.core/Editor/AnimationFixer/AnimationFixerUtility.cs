using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace YontalaneEditor
{
    /// <summary>
    /// Provides the target-resolution and property-path-rewriting logic behind the Animation Fixer window.
    /// </summary>
    internal static class AnimationFixerUtility
    {
        /// <summary>
        /// Binding flags used to reflect into Unity's internal Animation window state.
        /// </summary>
        private const BindingFlags k_ReflectionFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Cached reflection handle for AnimationWindow's private m_AnimEditor field.
        /// </summary>
        private static FieldInfo s_animEditorField;

        /// <summary>
        /// Cached reflection handle for AnimEditor's internal state property.
        /// </summary>
        private static PropertyInfo s_stateProperty;

        /// <summary>
        /// Cached reflection handle for AnimationWindowState's activeAnimationClip property.
        /// </summary>
        private static PropertyInfo s_activeAnimationClipProperty;

        /// <summary>
        /// Cached reflection handle for AnimationWindowState's activeRootGameObject property.
        /// </summary>
        private static PropertyInfo s_activeRootGameObjectProperty;

        /// <summary>
        /// The clips and (optionally) the reference hierarchy that Animation Fixer should operate on.
        /// </summary>
        internal readonly struct FixTarget
        {
            /// <summary>
            /// The clips to operate on. Empty if there is no valid target.
            /// </summary>
            internal AnimationClip[] Clips { get; }

            /// <summary>
            /// The GameObject whose hierarchy can be used to validate/resolve property paths, if any.
            /// Only available when the Animation window is targeting a clip.
            /// </summary>
            internal GameObject ReferenceRoot { get; }

            internal bool HasTarget => Clips != null && Clips.Length > 0;

            internal bool HasReference => ReferenceRoot != null;

            internal FixTarget(AnimationClip[] clips, GameObject referenceRoot)
            {
                Clips = clips;
                ReferenceRoot = referenceRoot;
            }
        }

        /// <summary>
        /// The result of an Automatic Fix pass.
        /// </summary>
        internal readonly struct AutoFixResult
        {
            /// <summary>
            /// The number of curve property paths that were confidently resolved and rewritten.
            /// </summary>
            internal int FixedCount { get; }

            /// <summary>
            /// The number of broken property paths that could not be confidently resolved.
            /// </summary>
            internal int UnresolvedCount { get; }

            internal AutoFixResult(int fixedCount, int unresolvedCount)
            {
                FixedCount = fixedCount;
                UnresolvedCount = unresolvedCount;
            }
        }

        /// <summary>
        /// Resolves what Animation Fixer should operate on: the clip targeted by an open Animation window,
        /// or, failing that, the AnimationClips currently selected in the Project pane.
        /// </summary>
        /// <returns>The resolved target.</returns>
        internal static FixTarget ResolveTarget()
        {
            if (TryGetAnimationWindowTarget(out AnimationClip windowClip, out GameObject referenceRoot))
            {
                return new FixTarget(new[] { windowClip }, referenceRoot);
            }

            AnimationClip[] selectedClips = Selection.GetFiltered<AnimationClip>(SelectionMode.Assets);

            return selectedClips.Length > 0 ? new FixTarget(selectedClips, null) : new FixTarget(null, null);
        }

        /// <summary>
        /// Replaces every occurrence of <paramref name="find"/> with <paramref name="replacement"/> in the
        /// property paths of every curve in the given clips.
        /// </summary>
        /// <param name="clips">The clips to modify.</param>
        /// <param name="find">The text to search for. If null or empty, no changes are made.</param>
        /// <param name="replacement">The text to substitute in its place.</param>
        /// <returns>The number of curve bindings that were rewritten.</returns>
        internal static int Replace(IReadOnlyList<AnimationClip> clips, string find, string replacement)
        {
            if (string.IsNullOrEmpty(find))
            {
                return 0;
            }

            replacement ??= string.Empty;

            return RewritePaths(clips, path => path.Replace(find, replacement), "Animation Fixer: Replace");
        }

        /// <summary>
        /// Prepends <paramref name="prefix"/> to the property paths of every curve in the given clips.
        /// </summary>
        /// <param name="clips">The clips to modify.</param>
        /// <param name="prefix">The text to insert at the start of each path.</param>
        /// <returns>The number of curve bindings that were rewritten.</returns>
        internal static int Prepend(IReadOnlyList<AnimationClip> clips, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return 0;
            }

            return RewritePaths(clips, path => prefix + path, "Animation Fixer: Prepend");
        }

        /// <summary>
        /// Attempts to automatically resolve broken property paths in the clip by matching their leaf object
        /// names against the actual hierarchy under <paramref name="referenceRoot"/>. Only unambiguous matches
        /// are rewritten; anything else is left untouched and reported as unresolved.
        /// </summary>
        /// <param name="clip">The clip to fix.</param>
        /// <param name="referenceRoot">The GameObject whose hierarchy represents the correct paths.</param>
        /// <returns>How many paths were fixed and how many were left unresolved.</returns>
        internal static AutoFixResult AutoFix(AnimationClip clip, GameObject referenceRoot)
        {
            Dictionary<string, Transform> validPaths = BuildValidPathMap(referenceRoot);
            Dictionary<string, List<string>> pathsByLeaf = new();
            Dictionary<string, List<string>> pathsByLeafIgnoreCase = new();

            foreach (string validPath in validPaths.Keys)
            {
                string leaf = GetLeaf(validPath);
                AddCandidate(pathsByLeaf, leaf, validPath);
                AddCandidate(pathsByLeafIgnoreCase, leaf.ToLowerInvariant(), validPath);
            }

            Dictionary<string, string> resolution = new();
            int unresolvedCount = 0;

            void Consider(string path)
            {
                if (validPaths.ContainsKey(path) || resolution.ContainsKey(path))
                {
                    return;
                }

                string leaf = GetLeaf(path);

                if (!TryGetSingleCandidate(pathsByLeaf, leaf, out string resolvedPath)
                    && !TryGetSingleCandidate(pathsByLeafIgnoreCase, leaf.ToLowerInvariant(), out resolvedPath))
                {
                    unresolvedCount++;
                    return;
                }

                resolution[path] = resolvedPath;
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
            {
                Consider(binding.path);
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                Consider(binding.path);
            }

            int fixedCount = RewritePaths(
                new[] { clip },
                path => resolution.TryGetValue(path, out string newPath) ? newPath : path,
                "Animation Fixer: Automatic Fix");

            return new AutoFixResult(fixedCount, unresolvedCount);
        }

        /// <summary>
        /// Adds a candidate path under the given key, creating the list if necessary.
        /// </summary>
        private static void AddCandidate(Dictionary<string, List<string>> map, string key, string path)
        {
            if (!map.TryGetValue(key, out List<string> candidates))
            {
                candidates = new List<string>();
                map[key] = candidates;
            }

            candidates.Add(path);
        }

        /// <summary>
        /// Returns the single candidate path under the given key, if and only if there is exactly one.
        /// </summary>
        private static bool TryGetSingleCandidate(Dictionary<string, List<string>> map, string key, out string path)
        {
            if (map.TryGetValue(key, out List<string> candidates) && candidates.Count == 1)
            {
                path = candidates[0];
                return true;
            }

            path = null;
            return false;
        }

        /// <summary>
        /// Returns the last path segment (the object name) of an animation property path.
        /// </summary>
        private static string GetLeaf(string path)
        {
            int lastSlash = path.LastIndexOf('/');
            return lastSlash < 0 ? path : path[(lastSlash + 1)..];
        }

        /// <summary>
        /// Builds a map of every valid animation property path under the reference root, including the root
        /// itself (which maps to the empty-string path).
        /// </summary>
        private static Dictionary<string, Transform> BuildValidPathMap(GameObject referenceRoot)
        {
            Dictionary<string, Transform> map = new();
            Transform root = referenceRoot.transform;

            void Walk(Transform current)
            {
                map[AnimationUtility.CalculateTransformPath(current, root)] = current;

                for (int i = 0; i < current.childCount; i++)
                {
                    Walk(current.GetChild(i));
                }
            }

            Walk(root);

            return map;
        }

        /// <summary>
        /// Applies a path transform to every float and object-reference curve binding across the given clips,
        /// recording undo and marking clips dirty only where a path actually changes.
        /// </summary>
        /// <param name="clips">The clips to modify.</param>
        /// <param name="transformPath">Computes the new path for a given existing path.</param>
        /// <param name="undoLabel">The label used for the undo entry.</param>
        /// <returns>The total number of curve bindings that were rewritten.</returns>
        private static int RewritePaths(IReadOnlyList<AnimationClip> clips, Func<string, string> transformPath, string undoLabel)
        {
            if (clips == null || clips.Count == 0)
            {
                return 0;
            }

            int totalChanged = 0;

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (AnimationClip clip in clips)
                {
                    if (clip != null)
                    {
                        totalChanged += RewritePathsInClip(clip, transformPath, undoLabel);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            if (totalChanged > 0)
            {
                AssetDatabase.SaveAssets();
            }

            return totalChanged;
        }

        /// <summary>
        /// Applies a path transform to every float and object-reference curve binding in a single clip.
        /// </summary>
        private static int RewritePathsInClip(AnimationClip clip, Func<string, string> transformPath, string undoLabel)
        {
            List<(EditorCurveBinding oldBinding, EditorCurveBinding newBinding, AnimationCurve curve)> floatChanges = new();
            List<(EditorCurveBinding oldBinding, EditorCurveBinding newBinding, ObjectReferenceKeyframe[] keyframes)> objectChanges = new();

            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
            {
                string newPath = transformPath(binding.path);

                if (newPath == binding.path)
                {
                    continue;
                }

                EditorCurveBinding newBinding = binding;
                newBinding.path = newPath;
                floatChanges.Add((binding, newBinding, AnimationUtility.GetEditorCurve(clip, binding)));
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                string newPath = transformPath(binding.path);

                if (newPath == binding.path)
                {
                    continue;
                }

                EditorCurveBinding newBinding = binding;
                newBinding.path = newPath;
                objectChanges.Add((binding, newBinding, AnimationUtility.GetObjectReferenceCurve(clip, binding)));
            }

            if (floatChanges.Count == 0 && objectChanges.Count == 0)
            {
                return 0;
            }

            Undo.RegisterCompleteObjectUndo(clip, undoLabel);

            foreach (var change in floatChanges)
            {
                AnimationUtility.SetEditorCurve(clip, change.oldBinding, null);
                AnimationUtility.SetEditorCurve(clip, change.newBinding, change.curve);
            }

            foreach (var change in objectChanges)
            {
                AnimationUtility.SetObjectReferenceCurve(clip, change.oldBinding, null);
                AnimationUtility.SetObjectReferenceCurve(clip, change.newBinding, change.keyframes);
            }

            EditorUtility.SetDirty(clip);

            return floatChanges.Count + objectChanges.Count;
        }

        /// <summary>
        /// Attempts to find an open Animation window and read the clip and reference GameObject it is
        /// currently targeting, via reflection into Unity's internal AnimEditor/AnimationWindowState.
        /// </summary>
        private static bool TryGetAnimationWindowTarget(out AnimationClip clip, out GameObject referenceRoot)
        {
            clip = null;
            referenceRoot = null;

            AnimationWindow[] windows = Resources.FindObjectsOfTypeAll<AnimationWindow>();

            foreach (AnimationWindow window in windows)
            {
                object state = GetAnimationWindowState(window);

                if (state == null)
                {
                    continue;
                }

                s_activeAnimationClipProperty ??= state.GetType().GetProperty("activeAnimationClip", k_ReflectionFlags);

                if (s_activeAnimationClipProperty?.GetValue(state) is not AnimationClip activeClip)
                {
                    continue;
                }

                s_activeRootGameObjectProperty ??= state.GetType().GetProperty("activeRootGameObject", k_ReflectionFlags);

                clip = activeClip;
                referenceRoot = s_activeRootGameObjectProperty?.GetValue(state) as GameObject;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reflects into an AnimationWindow instance to retrieve its internal AnimationWindowState object.
        /// </summary>
        private static object GetAnimationWindowState(AnimationWindow window)
        {
            s_animEditorField ??= typeof(AnimationWindow).GetField("m_AnimEditor", k_ReflectionFlags);

            object animEditor = s_animEditorField?.GetValue(window);

            if (animEditor == null)
            {
                return null;
            }

            s_stateProperty ??= animEditor.GetType().GetProperty("state", k_ReflectionFlags);

            return s_stateProperty?.GetValue(animEditor);
        }
    }
}
