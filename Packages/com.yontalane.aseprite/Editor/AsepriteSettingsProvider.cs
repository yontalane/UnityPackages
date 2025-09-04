using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Yontalane.Aseprite;

namespace YontalaneEditor.Aseprite
{
    public class AsepriteSettingsProvider : SettingsProvider
    {
        private const string SETTINGS_PATH = "Yontalane/Aseprite";

        private Editor m_editor;

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new AsepriteSettingsProvider(SETTINGS_PATH, SettingsScope.Project, null);
        }

        public AsepriteSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords)
        {

        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Gizmos", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            m_editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                AsepriteSettings.instance.Save();
            }
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            AsepriteSettings preferences = AsepriteSettings.instance;
            preferences.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            Editor.CreateCachedEditor(preferences, null, ref m_editor);
        }
    }
}