using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YontalaneEditor.Aseprite
{
    /// <summary>
    /// Provides utility methods for working with animation data, animation clips, and animated sprites.
    /// </summary>
    internal static class AnimationUtilities
    {
        internal static EditorCurveBinding SpriteBinding => new()
        {
            path = "Sprite",
            propertyName = "m_Sprite",
            type = typeof(SpriteRenderer),
        };

        /// <summary>
        /// Calculates the duration of a single frame in an animation clip.
        /// </summary>
        /// <param name="clip">The animation clip to calculate the duration of.</param>
        /// <returns>The duration of a single frame in the animation clip.</returns>
        internal static float GetDurationOfSingleFrame(this AnimationClip clip)
        {
            return clip.length / clip.frameRate;
        }

        /// <summary>
        /// Finds the animation data that contains the specified frame index, if any.
        /// </summary>
        /// <param name="fileData">The import file data containing animation information.</param>
        /// <param name="frameIndex">The frame index to search for.</param>
        /// <param name="animationData">The animation data found for the frame, or default if not found.</param>
        /// <returns>True if an animation containing the frame index is found; otherwise, false.</returns>
        internal static bool TryGetAnimationForFrame(this ImportFileData fileData, int frameIndex, out AnimationData animationData)
        {
            foreach (AnimationData data in fileData.animations)
            {
                if (frameIndex >= data.fromFrame && frameIndex <= data.toFrame)
                {
                    animationData = data;
                    return true;
                }
            }

            animationData = default;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve an AnimationClip with the specified name from the ImportFileData's objects list.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing the list of objects.</param>
        /// <param name="name">The name of the AnimationClip to search for.</param>
        /// <param name="animationClip">The found AnimationClip if successful; otherwise, null.</param>
        /// <returns>True if an AnimationClip with the given name is found; otherwise, false.</returns>
        internal static bool TryGetAnimationClip(this ImportFileData fileData, string name, out AnimationClip animationClip)
        {
            foreach (Object obj in fileData.objects)
            {
                if (obj is AnimationClip clip && clip.name == name)
                {
                    animationClip = clip;
                    return true;
                }
            }

            animationClip = null;
            return false;
        }

        /// <summary>
        /// Attempts to find a Sprite object in the ImportFileData's objects list that matches the given frame index.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing the list of objects.</param>
        /// <param name="frameIndex">The index of the frame to search for.</param>
        /// <param name="sprite">The resulting Sprite if found; otherwise, null.</param>
        /// <returns>True if a matching Sprite is found; otherwise, false.</returns>
        internal static bool TryGetSprite(this ImportFileData fileData, int frameIndex, out Sprite sprite)
        {
            foreach (Object obj in fileData.objects)
            {
                if (obj is Sprite frame && frame.name == $"Frame_{frameIndex}")
                {
                    sprite = frame;
                    return true;
                }
            }

            sprite = null;
            return false;
        }

        /// <summary>
        /// Sets the tangent mode of all keys in the provided animation curve to Constant.
        /// </summary>
        /// <param name="curve">The animation curve to modify.</param>
        internal static void ConstantifyCurve(ref AnimationCurve curve)
        {
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            }
        }

        /// <summary>
        /// Returns the index of the key in the given AnimationCurve that matches the specified time, or -1 if not found.
        /// </summary>
        /// <param name="curve">The AnimationCurve to search.</param>
        /// <param name="time">The time value to look for.</param>
        /// <returns>The index of the key if found; otherwise, -1.</returns>
        internal static int IndexOfKey(this AnimationCurve curve, float time)
        {
            if (curve == null)
            {
                return -1;
            }

            for (int i = 0; i < curve.length; i++)
            {
                if (Mathf.Approximately(curve.keys[i].time, time))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Adds a key to the given AnimationCurve if it doesn't exist, or replaces the value of an existing key with the specified time and value.
        /// </summary>
        /// <param name="curve">The AnimationCurve to modify.</param>
        /// <param name="time">The time value of the key to add or replace.</param>
        /// <param name="value">The value to set for the key.</param>
        internal static void AddOrReplaceKey(ref AnimationCurve curve, float time, float value)
        {
            int i = IndexOfKey(curve, time);

            if (i == -1)
            {
                _ = curve.AddKey(time, value);
                return;
            }

            curve.keys[i].value = value;
        }

        /// <summary>
        /// Finds the keyframe in the given list whose time is closest to the specified time.
        /// </summary>
        /// <param name="frames">The list of ObjectReferenceKeyframes to search.</param>
        /// <param name="time">The target time to find the nearest keyframe to.</param>
        /// <param name="nearestTime">Outputs the time of the nearest keyframe found.</param>
        /// <param name="nearestIndex">Outputs the index of the nearest keyframe found, or -1 if none found.</param>
        /// <returns>True if a nearest keyframe was found; otherwise, false.</returns>
        internal static bool TryGetNearestFrameTime(this IReadOnlyList<ObjectReferenceKeyframe> frames, float time, out float nearestTime, out int nearestIndex)
        {
            nearestTime = time;
            nearestIndex = -1;
            float nearestDiff = Mathf.Infinity;

            for (int i = 0; i < frames.Count; i++)
            {
                float diff = Mathf.Abs(frames[i].time - time);

                if (diff < nearestDiff)
                {
                    nearestIndex = i;
                    nearestDiff = diff;
                    nearestTime = frames[i].time;
                }
            }

            return nearestIndex >= 0;
        }

        /// <summary>
        /// Finds the time of the keyframe in the given list that is closest to the specified time.
        /// </summary>
        /// <param name="frames">The list of ObjectReferenceKeyframes to search.</param>
        /// <param name="time">The target time to find the nearest keyframe to.</param>
        /// <returns>The time of the nearest keyframe found.</returns>
        internal static float GetNearestFrameTime(this IReadOnlyList<ObjectReferenceKeyframe> frames, float time)
        {
            _ = TryGetNearestFrameTime(frames, time, out float nearestTime, out int _);
            return nearestTime;
        }

        /// <summary>
        /// Finds the keyframe in the given list whose time is closest to the specified time,
        /// then outputs the time and index of the next keyframe (if any).
        /// </summary>
        /// <param name="frames">The list of ObjectReferenceKeyframes to search.</param>
        /// <param name="time">The target time to find the nearest keyframe to.</param>
        /// <param name="nearestTime">Outputs the time of the next keyframe after the nearest one found.</param>
        /// <param name="nearestIndex">Outputs the index of the next keyframe after the nearest one found, or -1 if none found.</param>
        /// <returns>True if a next keyframe exists after the nearest one; otherwise, false.</returns>
        internal static bool TryGetNextNearestFrameTime(this IReadOnlyList<ObjectReferenceKeyframe> frames, float time, out float nearestTime, out int nearestIndex)
        {
            nearestTime = time;
            nearestIndex = -1;
            float nearestDiff = Mathf.Infinity;

            for (int i = 0; i < frames.Count; i++)
            {
                float diff = Mathf.Abs(frames[i].time - time);

                if (diff < nearestDiff)
                {
                    nearestIndex = i;
                    nearestDiff = diff;
                    nearestTime = frames[i].time;
                }
            }

            if (nearestIndex == -1)
            {
                return false;
            }

            if (nearestIndex == frames.Count - 1) // If the nearest keyframe is the last one, return false
            {
                return false;
            }

            nearestIndex++;
            nearestTime = frames[nearestIndex].time;

            return true;
        }

        /// <summary>
        /// Finds the time of the keyframe in the given list that is closest to the specified time,
        /// then outputs the time of the next keyframe (if any).
        /// </summary>
        /// <param name="frames">The list of ObjectReferenceKeyframes to search.</param>
        /// <param name="time">The target time to find the nearest keyframe to.</param>
        /// <returns>The time of the next keyframe after the nearest one found.</returns>
        internal static float GetNextNearestFrameTime(this IReadOnlyList<ObjectReferenceKeyframe> frames, float time)
        {
            _ = TryGetNextNearestFrameTime(frames, time, out float nearestTime, out int _);
            return nearestTime;
        }

        /// <summary>
        /// Sets the sprite for a specific frame in the list of ObjectReferenceKeyframes at the given time.
        /// If snapToNearest is true, updates the nearest frame's sprite instead of inserting a new keyframe.
        /// </summary>
        /// <param name="frames">The list of ObjectReferenceKeyframes to modify.</param>
        /// <param name="sprite">The sprite to set for the frame.</param>
        /// <param name="time">The time at which to set the sprite.</param>
        /// <param name="snapToNearest">If true, updates the nearest frame instead of inserting a new one.</param>
        internal static void SetSpriteFrame(this List<ObjectReferenceKeyframe> frames, Sprite sprite, float time, bool snapToNearest)
        {
            // If snapToNearest is enabled and there is at least one frame, update the nearest frame's sprite instead of inserting a new keyframe.
            if (snapToNearest && TryGetNearestFrameTime(frames, time, out float _, out int nearestIndex))
            {
                // If we're snapping to the second to last frame, and if the last frame is a duplicate of the second to last, then replace both.
                if (nearestIndex == frames.Count - 2 && frames[^1].value == frames[^2].value)
                {
                    ObjectReferenceKeyframe finalFrame = frames[^1];
                    finalFrame.value = sprite;
                    frames[^1] = finalFrame;
                }

                ObjectReferenceKeyframe frame = frames[nearestIndex];
                frame.value = sprite;
                frames[nearestIndex] = frame;

                return;
            }

            // If snapToNearest is disabled or there are no frames, insert a new keyframe at the specified time.
            bool isEarlier = true;

            // Iterate through the frames to find the correct insertion point.
            for (int i = 0; i < frames.Count; i++)
            {
                // If a frame is found at the specified time, update its sprite and return.
                if (Mathf.Approximately(frames[i].time, time))
                {
                    ObjectReferenceKeyframe frame = frames[i];
                    frame.value = sprite;
                    frames[i] = frame;
                    return;
                }
                // If the current frame is later than the specified time and isEarlier is true, insert a new keyframe at the specified time.
                else if (isEarlier && frames[i].time > time)
                {
                    frames.Insert(i, new()
                    {
                        time = time,
                        value = sprite,
                    });
                    return;
                }
            }

            // If no frame was found at the specified time, add a new keyframe at that time.
            frames.Add(new()
            {
                time = time,
                value = sprite,
            });
        }

        /// <summary>
        /// Returns the animation binding path for a child object with the given name, relative to the spriteObject in the ImportFileData.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing the root sprite object.</param>
        /// <param name="childObjectName">The name of the child object.</param>
        /// <returns>The binding path string used for animation bindings.</returns>
        internal static string GetBindingPath(this ImportFileData fileData, string childObjectName)
        {
            return $"{fileData.spriteObject.name}/{childObjectName}";
        }

        /// <summary>
        /// Returns the animation binding path for the given child object, relative to the spriteObject in the ImportFileData.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing the root sprite object.</param>
        /// <param name="childObject">The child object to get the binding path for.</param>
        /// <returns>The binding path string used for animation bindings.</returns>
        internal static string GetBindingPath(this ImportFileData fileData, GameObject childObject)
        {
            return fileData.GetBindingPath(childObject.name);
        }

        /// <summary>
        /// Returns the animation binding path for the given child object, relative to the spriteObject in the ImportFileData.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing the root sprite object.</param>
        /// <param name="childObject">The child object to get the binding path for.</param>
        /// <returns>The binding path string used for animation bindings.</returns>
        internal static string GetBindingPath(this ImportFileData fileData, Component childObject)
        {
            return fileData.GetBindingPath(childObject.gameObject);
        }

        /// <summary>
        /// Returns the index of the key in the given AnimationCurve that matches the specified time, or -1 if not found.
        /// </summary>
        /// <param name="clip">The AnimationClip to search.</param>
        /// <param name="binding">The EditorCurveBinding to search.</param>
        /// <param name="time">The time value to look for.</param>
        /// <returns>The index of the key if found; otherwise, -1.</returns>
        internal static int IndexOfKey(this AnimationClip clip, EditorCurveBinding binding, float time)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            return curve.IndexOfKey(time);
        }

        /// <summary>
        /// Sets a boolean key in the given AnimationClip at the specified time and path.
        /// </summary>
        /// <param name="clip">The AnimationClip to modify.</param>
        /// <param name="path">The path to the property to set.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="type">The type of the component to set the property on.</param>
        /// <param name="time">The time at which to set the property.</param>
        /// <param name="value">The value to set for the property.</param>
        private static void SetBoolKey<T>(this AnimationClip clip, string path, string propertyName, float time, bool value)
        {
            // Create an EditorCurveBinding for the specified property on the given path and type
            EditorCurveBinding binding = new()
            {
                path = path,
                propertyName = propertyName,
                type = typeof(T),
            };
            
            // Get the animation curve for the specified property on the given path and type
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

            // If the animation curve is null, create a new one
            curve ??= new();

            // Add or replace a keyframe at the specified time with the given value
            AddOrReplaceKey(ref curve, time, value ? 1f : 0f);

            // Set the tangent mode of the curve to constant
            ConstantifyCurve(ref curve);

            // Set the animation curve for the specified property on the given path and type
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        /// <summary>
        /// Sets the isActive property within the AnimationClip at the specified time.
        /// </summary>
        /// <param name="clip">The AnimationClip to modify.</param>
        /// <param name="path">The path to the property to set.</param>
        /// <param name="time">The time at which to set the property.</param>
        internal static void SetIsActiveKey(this AnimationClip clip, string path, float time, bool value)
        {
            clip.SetBoolKey<GameObject>(path, "m_IsActive", time, value);
        }

        /// <summary>
        /// Sets the enabled property within the AnimationClip at the specified time.
        /// </summary>
        /// <typeparam name="T">The type of the component to set the property on.</typeparam>
        /// <param name="clip">The AnimationClip to modify.</param>
        /// <param name="path">The path to the property to set.</param>
        /// <param name="time">The time at which to set the property.</param>
        /// <param name="value">The value to set for the property.</param>
        internal static void SetEnabledKey<T>(this AnimationClip clip, string path, float time, bool value) where T : Component
        {
            clip.SetBoolKey<T>(path, "m_Enabled", time, value);
        }

        /// <summary>
        /// Sets a Vector2 key in the given AnimationClip at the specified time and path.
        /// </summary>
        /// <param name="clip">The AnimationClip to modify.</param>
        /// <param name="path">The path to the property to set.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="type">The type of the component to set the property on.</param>
        /// <param name="time">The time at which to set the property.</param>
        /// <param name="value">The value to set for the property.</param>
        /// <param name="constantify">Whether to set the tangent mode of the curve to constant.</param>
        private static void SetVector2Key<T>(this AnimationClip clip, string path, string propertyName, float time, Vector2 value, bool constantify = false)
        {
            // Create an EditorCurveBinding for the X component of the specified property on the given path and type
            EditorCurveBinding bindingX = new()
            {
                path = path,
                propertyName = $"{propertyName}.x",
                type = typeof(T),
            };

            // Create an EditorCurveBinding for the Y component of the specified property on the given path and type
            EditorCurveBinding bindingY = new()
            {
                path = path,
                propertyName = $"{propertyName}.y",
                type = typeof(T),
            };

            // Get the animation curves for the X and Y components of the specified property on the given path and type
            AnimationCurve curveX = AnimationUtility.GetEditorCurve(clip, bindingX);
            AnimationCurve curveY = AnimationUtility.GetEditorCurve(clip, bindingY);

            // If the animation curves are null, create new ones
            curveX ??= new();
            curveY ??= new();

            // Add or replace keyframes at the specified time with the given values
            AddOrReplaceKey(ref curveX, time, value.x);
            AddOrReplaceKey(ref curveY, time, value.y);

            // If constantify is true, set the tangent mode of the curves to constant
            if (constantify)
            {
                ConstantifyCurve(ref curveX);
                ConstantifyCurve(ref curveY);
            }

            // Set the animation curves for the X and Y components of the specified property on the given path and type
            AnimationUtility.SetEditorCurve(clip, bindingX, curveX);
            AnimationUtility.SetEditorCurve(clip, bindingY, curveY);
        }

        /// <summary>
        /// Sets the Transform localPosition property within the AnimationClip at the specified time.
        /// </summary>
        /// <param name="clip">The AnimationClip to modify.</param>
        /// <param name="path">The path to the property to set.</param>
        /// <param name="time">The time at which to set the property.</param>
        /// <param name="value">The value to set for the property.</param>
        /// <param name="constantify">Whether to set the tangent mode of the curve to constant.</param>
        internal static void SetPositionKey(this AnimationClip clip, string path, float time, Vector2 value, bool constantify = false)
        {
            clip.SetVector2Key<Transform>(path, "m_LocalPosition", time, value, constantify);
        }

        /// <summary>
        /// Sets the BoxCollider2D size property within the AnimationClip at the specified time.
        /// </summary>
        /// <param name="clip">The AnimationClip to modify.</param>
        /// <param name="path">The path to the property to set.</param>
        /// <param name="time">The time at which to set the property.</param>
        /// <param name="value">The value to set for the property.</param>
        /// <param name="constantify">Whether to set the tangent mode of the curve to constant.</param>
        internal static void SetSizeKey(this AnimationClip clip, string path, float time, Vector2 value, bool constantify = false)
        {
            clip.SetVector2Key<BoxCollider2D>(path, "m_Size", time, value, constantify);
        }

        /// <summary>
        /// Sets the BoxCollider2D offset property within the AnimationClip at the specified time.
        /// </summary>
        /// <param name="clip">The AnimationClip to modify.</param>
        /// <param name="path">The path to the property to set.</param>
        /// <param name="time">The time at which to set the property.</param>
        /// <param name="value">The value to set for the property.</param>
        /// <param name="constantify">Whether to set the tangent mode of the curve to constant.</param>
        internal static void SetOffsetKey(this AnimationClip clip, string path, float time, Vector2 value, bool constantify = false)
        {
            clip.SetVector2Key<BoxCollider2D>(path, "m_Offset", time, value, constantify);
        }

        /// <summary>
        /// Ensures that all boolean properties in the AnimationClip begin with a keyframe value of 0 at time 0.
        /// </summary>
        /// <param name="clip">The AnimationClip to modify.</param>
        internal static void EnsureBoolPropertiesBeginOff(this AnimationClip clip)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

            // Iterate through the bindings and ensure that all boolean properties begin with a keyframe value of 0 at time 0
            for (int i = 0; i < bindings.Length; i++)
            {
                // Get the property name of the current binding
                string propertyName = bindings[i].propertyName.ToLower();

                // If the property name does not contain "enabled" or "isactive", skip it
                if (!propertyName.Contains("enabled") && !propertyName.Contains("isactive"))
                {
                    continue;
                }

                // Get the animation curve for the current binding
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);

                // If the animation curve is null, skip it
                if (curve == null)
                {
                    continue;
                }

                // If the animation curve already has a key at time 0, skip it
                if (curve.HasKeyAtTime(0f))
                {
                    continue;
                }

                // Add a keyframe at time 0 with a value of 0
                AddOrReplaceKey(ref curve, 0f, 0f);

                // Set the tangent mode of the curve to constant
                ConstantifyCurve(ref curve);

                AnimationUtility.SetEditorCurve(clip, bindings[i], curve);
            }
        }

        /// <summary>
        /// Checks if the AnimationCurve has a key at the specified time.
        /// </summary>
        /// <param name="curve">The AnimationCurve to check.</param>
        /// <param name="time">The time to check for a key.</param>
        /// <returns>True if the AnimationCurve has a key at the specified time; otherwise, false.</returns>
        internal static bool HasKeyAtTime(this AnimationCurve curve, float time)
        {
            foreach(Keyframe keyframe in curve.keys)
            {
                if (Mathf.Approximately(keyframe.time, time))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
