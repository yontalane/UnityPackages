using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.LayoutTilemap
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Layout Tilemap/Map Entity Manager")]
    public sealed class MapEntityManager : MonoBehaviour
    {
        [System.Serializable]
        public struct MapEntityData
        {
            public float scale;
            public Color outerColor;
            public Color innerColor;
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
                scale = 1f,
                outerColor = Color.gray,
                innerColor = Color.white
            };

            specialCaseData = new NamedMapEntityData[0];
        }

        public MapEntityData GetData(string name)
        {
            foreach(NamedMapEntityData namedData in specialCaseData)
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