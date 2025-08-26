using UnityEngine;

namespace Yontalane.LayoutTilemap
{
    /// <summary>
    /// Specifies the type of value that a map property can hold.
    /// </summary>
    [System.Serializable]
    public enum MapPropertyValueType
    {
        String,
        Float,
        Int,
        Bool,
        Object
    }

    /// <summary>
    /// Represents a value for a map property, supporting multiple data types such as string, float, int, bool, or Unity Object.
    /// </summary>
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

    /// <summary>
    /// A serializable dictionary that maps string keys to MapPropertyValue objects, allowing storage of various property types for map elements.
    /// </summary>
    [System.Serializable]
    public class MapPropertyDictionary : SerializableDictionary<string, MapPropertyValue> { }

    /// <summary>
    /// MapProperties is a MonoBehaviour component that stores a collection of key-value property pairs (MapPropertyDictionary) for a map element,
    /// allowing flexible assignment and retrieval of custom data (string, float, int, bool, or Object) at runtime or in the editor.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Layout Tilemap/Properties")]
    public class MapProperties : MonoBehaviour
    {
        [SerializeField]
        private MapPropertyDictionary m_properties = new MapPropertyDictionary();

        public MapPropertyDictionary Properties => m_properties;
    }
}