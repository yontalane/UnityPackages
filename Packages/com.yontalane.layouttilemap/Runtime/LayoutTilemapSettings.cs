using UnityEditor;
using UnityEngine;

namespace Yontalane.LayoutTilemap
{
    [FilePath("ProjectSettings/LayoutTilemapSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class LayoutTilemapSettings : ScriptableSingleton<LayoutTilemapSettings>
    {
        [System.Serializable]
        public struct MapEntityData
        {
            [Min(0.01f)]
            public float scale;

            public Color outerColor;

            public Color innerColor;

            [Min(0.01f)]
            public float pointerLength;

            [Min(0.01f)]
            public float pointerScale;

            [Range(1, 5)]
            public int thickness;
        }

        [System.Serializable]
        public struct NamedMapEntityData
        {
            public string name;
            public MapEntityData data;
        }

        public MapEntityData defaultData;
        public NamedMapEntityData[] specialCaseData;

        public LayoutTilemapSettings()
        {
            defaultData = new MapEntityData()
            {
                scale = 0.75f,
                outerColor = Color.gray,
                innerColor = Color.white,
                pointerLength = 1.5f,
                pointerScale = 0.5f,
                thickness = 2
            };

            specialCaseData = new NamedMapEntityData[0];
        }

        public void Save()
        {
            Save(true);
        }

        public MapEntityData GetData(string name)
        {
            foreach (NamedMapEntityData namedData in specialCaseData)
            {
                if (namedData.name.Contains(name))
                {
                    return namedData.data;
                }
            }
            return defaultData;
        }
    }
}
