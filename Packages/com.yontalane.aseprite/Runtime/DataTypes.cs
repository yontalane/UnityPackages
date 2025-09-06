using UnityEngine;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// Represents a motion tree, which contains an identifier, an animation curve, and a list of animation names.
    /// </summary>
    [System.Serializable]
    public struct MotionTree
    {
        [Tooltip("The identifier for this motion tree.")]
        public string id;

        [Tooltip("The animation curve associated with this motion tree.")]
        public AnimationCurve curve;

        [Tooltip("The list of animations associated with this motion tree.")]
        public AnimationClip[] animations;
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
