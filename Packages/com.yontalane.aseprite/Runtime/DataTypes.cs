using UnityEngine;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// Represents a motion tree, which contains an identifier, an animation curve, and a list of animation names.
    /// </summary>
    [System.Serializable]
    public struct MotionTree
    {
        /// <summary>
        /// The identifier for this motion tree.
        /// </summary>
        [Tooltip("The identifier for this motion tree.")]
        public string id;

        /// <summary>
        /// The animation curve associated with this motion tree.
        /// </summary>
        [Tooltip("The animation curve associated with this motion tree.")]
        public AnimationCurve curve;

        /// <summary>
        /// The list of animation names associated with this motion tree.
        /// </summary>
        [Tooltip("The list of animation names associated with this motion tree.")]
        public string[] animations;
    }

    /// <summary>
    /// Represents a key-value pair where the key is a string and the value is a float.
    /// </summary>
    public struct KeyFloatPair
    {
        /// <summary>
        /// The key of the pair.
        /// </summary>
        public string key;

        /// <summary>
        /// The float value of the pair.
        /// </summary>
        public float value;
    }
}
