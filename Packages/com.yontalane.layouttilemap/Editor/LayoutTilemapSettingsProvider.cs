using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Yontalane.LayoutTilemap;

namespace YontalaneEditor.LayoutTilemap
{
    public class LayoutTilemapSettingsProvider : SettingsProvider
    {
        private const string MAP_ENTITY_SETTINGS_PATH = "Yontalane/Layout Tilemap";

        private Editor _editor;

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new LayoutTilemapSettingsProvider(MAP_ENTITY_SETTINGS_PATH, SettingsScope.Project, null);
        }

        public LayoutTilemapSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords)
        {

        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Map Entity", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            _editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                LayoutTilemapSettings.instance.Save();
            }
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            LayoutTilemapSettings preferences = LayoutTilemapSettings.instance;
            preferences.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            Editor.CreateCachedEditor(preferences, null, ref _editor);
        }
    }
}