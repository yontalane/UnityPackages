using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// A struct containing information about an animation lifecycle event.
    /// </summary>
    [System.Serializable]
    public struct AnimationLifecycleEvent
    {
        /// <summary>
        /// The name of the animation.
        /// </summary>
        public string animationName;

        /// <summary>
        /// Whether the animation is looping.
        /// </summary>
        public bool isLooping;
    }

    /// <summary>
    /// A struct containing information about an animation motion event.
    /// </summary>
    [System.Serializable]
    public struct AnimationMotionEvent
    {
        /// <summary>
        /// The animation frame at which the motion takes place.
        /// </summary>
        public int frameIndex;

        /// <summary>
        /// The root motion value.
        /// </summary>
        public Vector2 motion;
    }

    /// <summary>
    /// Represents a motion tree, which contains an identifier, an animation curve, and a list of animation names.
    /// </summary>
    [System.Serializable]
    public struct MotionTree
    {
        [Tooltip("The identifier for this motion tree.")]
        public string id;

        [Tooltip("The list of animations associated with this motion tree.")]
        public AnimationClip[] animations;
    }

    /// <summary>
    /// Represents a key-value pair where the key is a string and the value is a float.
    /// </summary>
    public class KeyFloatPair
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

    /// <summary>
    /// UnityEvent that is invoked with a Vector2 representing root motion data from an Aseprite animation.
    /// </summary>
    [System.Serializable]
    public class OnMotionHandler : UnityEvent<AnimationMotionEvent>
    { }

    /// <summary>
    /// UnityEvent that is invoked upon animation lifecycle events.
    /// </summary>
    [System.Serializable]
    public class OnLifecycleHandler : UnityEvent<AnimationLifecycleEvent>
    { }

    /// <summary>
    /// UnityEvent handler for KeyFloatPair events. Expects invocations to populate the provided <see cref="KeyFloatPair"/> with a value between 0 and 1.
    /// </summary>
    [System.Serializable]
    public class KeyFloatHandler : UnityEvent<KeyFloatPair>
    { }
}