using System.Collections.Generic;
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

    /// <summary>
    /// Contains information about a sprite object's animation, including the animation name, its length, and the frames/times when an attached object, such as a collider, is active.
    /// </summary>
    [System.Serializable]
    public class SpriteObjectAnimationInfo
    {
        /// <summary>
        /// The animation's name.
        /// </summary>
        public string animation;

        /// <summary>
        /// The total number of frames.
        /// </summary>
        public int length;

        /// <summary>
        /// The list of frame indices where the attached object, such as a collider, is active.
        /// </summary>
        public List<int> framesOn;

        /// <summary>
        /// The list of times (in seconds) corresponding to when the attached object, such as a collider, is active.
        /// </summary>
        public List<float> timesOn;
    }

    /// <summary>
    /// Enumerates the possible types of objects that can be attached to sprites.
    /// </summary>
    public enum SpriteObjectType
    {
        /// <summary>
        /// No specific type assigned.
        /// </summary>
        None = 0,

        /// <summary>
        /// A collider object.
        /// </summary>
        Collider = 10,

        /// <summary>
        /// A trigger object.
        /// </summary>
        Trigger = 20,

        /// <summary>
        /// A point object (an empty transform).
        /// </summary>
        Point = 30,
    }

    /// <summary>
    /// Holds information about an object attached to a sprite. This includes the object's name, type, and associated animation data.
    /// </summary>
    [System.Serializable]
    public class SpriteObjectInfo
    {
        /// <summary>
        /// The name of the object.
        /// </summary>
        public string name;

        /// <summary>
        /// The type of the attached object (e.g., Collider, Trigger, Point).
        /// </summary>
        public SpriteObjectType type;

        /// <summary>
        /// The list of animation information associated with this object.
        /// </summary>
        public List<SpriteObjectAnimationInfo> animationInfo;
    }
}