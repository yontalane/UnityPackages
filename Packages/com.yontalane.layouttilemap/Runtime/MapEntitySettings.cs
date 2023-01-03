using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.LayoutTilemap
{
    class MapEntitySettings : ScriptableObject
    {
        public const string MAP_ENTITY_SETTINGS_PATH = "Packages/com.yontalane.layouttilemap/Editor/MapEntitySettings.asset";

#pragma warning disable 0414
        [SerializeField]
        private MapEntityConfig m_config;
#pragma warning restore 0414

        internal MapEntityConfig Config => m_config;

        internal static MapEntitySettings GetOrCreateSettings()
        {
            MapEntitySettings settings = AssetDatabase.LoadAssetAtPath<MapEntitySettings>(MAP_ENTITY_SETTINGS_PATH);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MapEntitySettings>();
                settings.m_config = default(MapEntityConfig);
                AssetDatabase.CreateAsset(settings, MAP_ENTITY_SETTINGS_PATH);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    static class MapEntitySettingsUIElementsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMapEntitySettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Project/Yontalane/Layout Tilemap", SettingsScope.Project)
            {
                label = "Map Entity",
                activateHandler = (searchContext, rootElement) =>
                {
                    SerializedObject settings = MapEntitySettings.GetSerializedSettings();

                    StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.yontalane.layouttilemap/Editor/MapEntitySettings.uss");
                    rootElement.styleSheets.Add(styleSheet);
                    Label title = new Label()
                    {
                        text = "Map Entity"
                    };
                    title.AddToClassList("field");
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    VisualElement properties = new VisualElement()
                    {
                        style =
                        {
                        flexDirection = FlexDirection.Column
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    Label header = new Label("Map Entity");
                    header.AddToClassList("field");
                    header.AddToClassList("header");
                    properties.Add(header);

                    UnityEditor.UIElements.ObjectField objectField = new UnityEditor.UIElements.ObjectField("Settings Asset");
                    objectField.allowSceneObjects = false;
                    objectField.objectType = typeof(MapEntityConfig);
                    objectField.SetValueWithoutNotify(settings.FindProperty("m_config").objectReferenceValue);
                    objectField.RegisterValueChangedCallback((ChangeEvent<Object> e) =>
                    {
                        settings.FindProperty("m_config").objectReferenceValue = (MapEntityConfig)e.newValue;
                        settings.ApplyModifiedProperties();
                    });
                    objectField.AddToClassList("field");
                    properties.Add(objectField);
                },

                keywords = new HashSet<string>(new[] { "Config", "Settings Asset", "Map Entity" })
            };

            return provider;
        }
    }
}