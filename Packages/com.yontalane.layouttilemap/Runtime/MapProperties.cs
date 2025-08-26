using UnityEngine;

namespace Yontalane.LayoutTilemap
{
    /// <summary>
    /// Specifies the type of value that a map property can hold.
    /// </summary>
    [System.Serializable]
    public enum MapPropertyValueType
    {
        /// <summary>
        /// A string value.
        /// </summary>
        String,
        /// <summary>
        /// A floating-point numeric value.
        /// </summary>
        Float,
        /// <summary>
        /// An integer numeric value.
        /// </summary>
        Int,
        /// <summary>
        /// A boolean value (true or false).
        /// </summary>
        Bool,
        /// <summary>
        /// A Unity Object reference.
        /// </summary>
        Object
    }

    /// <summary>
    /// Represents a value for a map property, supporting multiple data types such as string, float, int, bool, or Unity Object.
    /// </summary>
    [System.Serializable]
    public struct MapPropertyValue
    {
        [Tooltip("The type of value stored in this property (String, Float, Int, Bool, or Object).")]
        public MapPropertyValueType type;

        [Tooltip("The string value of the property (used if type is String).")]
        public string stringValue;

        [Tooltip("The float value of the property (used if type is Float).")]
        public float floatValue;

        [Tooltip("The integer value of the property (used if type is Int).")]
        public int intValue;

        [Tooltip("The boolean value of the property (used if type is Bool).")]
        public bool boolValue;

        [Tooltip("The Unity Object reference value of the property (used if type is Object).")]
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
        [Tooltip("A collection of key-value property pairs for this map element.")]
        [SerializeField]
        private MapPropertyDictionary m_properties = new();

        /// <summary>
        /// Gets the collection of key-value property pairs for this map element.
        /// </summary>
        public MapPropertyDictionary Properties => m_properties;
    }
}