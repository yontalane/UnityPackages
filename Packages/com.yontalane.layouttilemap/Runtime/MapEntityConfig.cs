using UnityEngine;

namespace Yontalane.LayoutTilemap
{
    [CreateAssetMenu(fileName = "Map Entity Config", menuName = "Yontalane/Map Entity Config", order = 1)]
    public sealed class MapEntityConfig : ScriptableObject
    {
        [System.Serializable]
        public struct MapEntityData
        {
            public float scale;
            public Color outerColor;
            public Color innerColor;
            public float pointerLength;
            public float pointerScale;
        }

        [System.Serializable]
        public struct NamedMapEntityData
        {
            public string name;
            public MapEntityData data;
        }

        public MapEntityData defaultData;
        public NamedMapEntityData[] specialCaseData;

        private void Reset()
        {
            defaultData = new MapEntityData()
            {
                scale = 0.75f,
                outerColor = Color.gray,
                innerColor = Color.white,
                pointerLength = 1.5f,
                pointerScale = 0.5f
            };

            specialCaseData = new NamedMapEntityData[0];
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