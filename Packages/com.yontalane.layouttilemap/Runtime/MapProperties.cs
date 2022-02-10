using UnityEngine;

namespace Yontalane.LayoutTilemap
{
    [System.Serializable]
    public enum MapPropertyValueType
    {
        String,
        Float,
        Int,
        Bool,
        Object
    }

    [System.Serializable]
    public struct MapPropertyValue
    {
        public MapPropertyValueType type;
        public string stringValue;
        public float floatValue;
        public int intValue;
        public bool boolValue;
        public Object objectReference;
    }

    [System.Serializable]
    public class MapPropertyDictionary : SerializableDictionary<string, MapPropertyValue> { }

    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Layout Tilemap/Properties")]
    public class MapProperties : MonoBehaviour
    {
        [SerializeField]
        private MapPropertyDictionary m_properties = new MapPropertyDictionary();

        public MapPropertyDictionary Properties => m_properties;
    }
}