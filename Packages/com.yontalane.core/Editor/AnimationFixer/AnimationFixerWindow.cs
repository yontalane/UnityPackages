using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YontalaneEditor
{
    /// <summary>
    /// Editor window for fixing broken property paths in AnimationClips, either automatically or via
    /// manual search/replace.
    /// </summary>
    internal sealed class AnimationFixerWindow : EditorWindow
    {
        /// <summary>
        /// The folder, relative to the project root, containing this window's UXML and USS assets.
        /// </summary>
        private const string k_AssetFolder = "Packages/com.yontalane.core/Editor/AnimationFixer/";

        private Button m_automaticFixButton;
        private TextField m_findField;
        private TextField m_newField;
        private Button m_replaceButton;
        private Button m_prependButton;

        private AnimationFixerUtility.FixTarget m_currentTarget;

        /// <summary>
        /// Opens the Animation Fixer window.
        /// </summary>
        [MenuItem("Window/Yontalane/Animation Fixer")]
        private static void ShowWindow()
        {
            AnimationFixerWindow window = GetWindow<AnimationFixerWindow>();
            window.titleContent = new GUIContent("Animation Fixer");
        }

        private void CreateGUI()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{k_AssetFolder}AnimationFixerWindow.uxml");
            visualTree.CloneTree(rootVisualElement);

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{k_AssetFolder}AnimationFixerWindow.uss");
            rootVisualElement.styleSheets.Add(styleSheet);

            m_automaticFixButton = rootVisualElement.Q<Button>("automatic-fix-button");
            m_findField = rootVisualElement.Q<TextField>("find-field");
            m_newField = rootVisualElement.Q<TextField>("new-field");
            m_replaceButton = rootVisualElement.Q<Button>("replace-button");
            m_prependButton = rootVisualElement.Q<Button>("prepend-button");

            m_automaticFixButton.clicked += OnAutomaticFixClicked;
            m_replaceButton.clicked += OnReplaceClicked;
            m_prependButton.clicked += OnPrependClicked;

            RefreshTarget();
        }

        private void OnEnable()
        {
            EditorApplication.update += RefreshTarget;
        }

        private void OnDisable()
        {
            EditorApplication.update -= RefreshTarget;
        }

        /// <summary>
        /// Re-resolves the current target (Animation window clip, or Project pane selection) and
        /// enables/disables controls accordingly.
        /// </summary>
        private void RefreshTarget()
        {
            m_currentTarget = AnimationFixerUtility.ResolveTarget();

            bool hasTarget = m_currentTarget.HasTarget;

            m_automaticFixButton?.SetEnabled(hasTarget && m_currentTarget.HasReference);
            m_findField?.SetEnabled(hasTarget);
            m_newField?.SetEnabled(hasTarget);
            m_replaceButton?.SetEnabled(hasTarget);
            m_prependButton?.SetEnabled(hasTarget);
        }

        private void OnAutomaticFixClicked()
        {
            if (!m_currentTarget.HasTarget || !m_currentTarget.HasReference)
            {
                return;
            }

            AnimationFixerUtility.AutoFixResult result = AnimationFixerUtility.AutoFix(m_currentTarget.Clips[0], m_currentTarget.ReferenceRoot);

            string message = result.UnresolvedCount == 0
                ? $"Fixed {result.FixedCount} propert{(result.FixedCount == 1 ? "y" : "ies")}."
                : $"Fixed {result.FixedCount} propert{(result.FixedCount == 1 ? "y" : "ies")}. {result.UnresolvedCount} could not be confidently resolved.";

            EditorUtility.DisplayDialog("Animation Fixer", message, "OK");
        }

        private void OnReplaceClicked()
        {
            if (!m_currentTarget.HasTarget)
            {
                return;
            }

            AnimationFixerUtility.Replace(m_currentTarget.Clips, m_findField.value, m_newField.value);
        }

        private void OnPrependClicked()
        {
            if (!m_currentTarget.HasTarget)
            {
                return;
            }

            AnimationFixerUtility.Prepend(m_currentTarget.Clips, m_newField.value);
        }
    }
}
