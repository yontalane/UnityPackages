using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

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
    /// Represents an entry in the motion tree.
    /// You only need to fill out one of the two fields: <see cref="name"/> or <see cref="clip"/> (not both).
    /// </summary>
    [System.Serializable]
    internal struct MotionTreeEntry
    {
        [Tooltip("The name of the animation. You only need to fill out this or 'clip', not both.")]
        public string name;

        [Tooltip("The reference to the animation clip. You only need to fill out this or 'name', not both.")]
        public AnimationClip clip;
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
        [SerializeField]
        private MotionTreeEntry[] m_animations;

        /// <summary>
        /// The number of animations in this <see cref="MotionTree"/>.
        /// </summary>
        public readonly int Count => m_animations != null ? m_animations.Length : 0;

        /// <summary>
        /// Gets the <see cref="AnimationClip"/> at the specified index, using a provided <see cref="RuntimeAnimatorController"/>.
        /// Allows lookup by name if the entry uses a name and not an <see cref="AnimationClip"/>.
        /// </summary>
        /// <param name="index">Index of the animation.</param>
        /// <param name="animatorController">The animator controller to use for lookup.</param>
        /// <returns>The animation clip if found; otherwise, null.</returns>
        public readonly AnimationClip Get(int index, RuntimeAnimatorController animatorController)
        {
            _ = TryGet(index, animatorController, out AnimationClip clip);
            return clip;
        }

        /// <summary>
        /// Gets the <see cref="AnimationClip"/> at the specified index, using a provided <see cref="Animator"/>.
        /// Allows lookup by name if the entry uses a name and not an <see cref="AnimationClip"/>.
        /// </summary>
        /// <param name="index">Index of the animation.</param>
        /// <param name="animator">Animator to obtain the controller from.</param>
        /// <returns>The animation clip if found; otherwise, null.</returns>
        public readonly AnimationClip Get(int index, Animator animator)
        {
            _ = TryGet(index, animator != null ? animator.runtimeAnimatorController : null, out AnimationClip clip);
            return clip;
        }

        /// <summary>
        /// Gets the <see cref="AnimationClip"/> at the specified index, without a controller.
        /// Requires entries to use <see cref="AnimationClip"/>s and not names.
        /// </summary>
        /// <param name="index">Index of the animation.</param>
        /// <returns>The animation clip if found; otherwise, null.</returns>
        public readonly AnimationClip Get(int index)
        {
            RuntimeAnimatorController animatorController = null;
            _ = TryGet(index, animatorController, out AnimationClip clip);
            return clip;
        }

        /// <summary>
        /// Attempts to get the <see cref="AnimationClip"/> at the specified index, searching by direct reference or controller lookup.
        /// Allows lookup by name if the entry uses a name and not an <see cref="AnimationClip"/>.
        /// </summary>
        /// <param name="index">Index of the animation.</param>
        /// <param name="animatorController">Animator controller to search in.</param>
        /// <param name="clip">The found animation clip, if any.</param>
        /// <returns>True if an animation clip was found; otherwise, false.</returns>
        public readonly bool TryGet(int index, RuntimeAnimatorController animatorController, out AnimationClip clip)
        {
            // Check for invalid state: empty animations array or index out of range.
            if (m_animations == null || index < 0 || index >= Count)
            {
                clip = null;
                return false;
            }

            // Retrieve the entry at the specified index.
            MotionTreeEntry entry = m_animations[index];

            // Prefer direct reference if available.
            if (entry.clip != null)
            {
                clip = entry.clip;
                return true;
            }

            // Either controller is missing, or entry has no name;
            // unable to look up by name.
            if (animatorController == null || string.IsNullOrEmpty(entry.name))
            {
                clip = null;
                return false;
            }

            // Iterate through animation clips in the controller, searching for a name match.
            foreach (AnimationClip c in animatorController.animationClips.Where(c => c.name == entry.name))
            {
                clip = c;
                return true;
            }

            // No matching AnimationClip found.
            clip = null;
            return false;
        }

        /// <summary>
        /// Attempts to get the <see cref="AnimationClip"/> at the specified index, using a provided <see cref="Animator"/> for lookup.
        /// Allows lookup by name if the entry uses a name and not an <see cref="AnimationClip"/>.
        /// </summary>
        /// <param name="index">Index of the animation.</param>
        /// <param name="animator">Animator to obtain the controller from.</param>
        /// <param name="clip">The found animation clip, if any.</param>
        /// <returns>True if an animation clip was found; otherwise, false.</returns>
        public readonly bool TryGet(int index, Animator animator, out AnimationClip clip)
        {
            return TryGet(index, animator != null ? animator.runtimeAnimatorController : null, out clip);
        }

        /// <summary>
        /// Attempts to get the <see cref="AnimationClip"/> at the specified index, without a controller.
        /// Requires entries to use <see cref="AnimationClip"/>s and not names.
        /// </summary>
        /// <param name="index">Index of the animation.</param>
        /// <param name="clip">The found animation clip, if any.</param>
        /// <returns>True if an animation clip was found; otherwise, false.</returns>
        public readonly bool TryGet(int index, out AnimationClip clip)
        {
            RuntimeAnimatorController animatorController = null;
            return TryGet(index, animatorController, out clip);
        }
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
    /// A start time and duration, counted both in seconds and frames.
    /// </summary>
    [System.Serializable]
    public struct ActivationPeriod
    {
        /// <summary>
        /// Start time in seconds.
        /// </summary>
        public float startTime;
        
        /// <summary>
        /// Duration in seconds.
        /// </summary>
        public float length;
        
        /// <summary>
        /// Start time in frames.
        /// </summary>
        public int startFrame;
        
        /// <summary>
        /// Duration in frames.
        /// </summary>
        public int frameCount;
        
        /// <summary>
        /// End time in seconds.
        /// </summary>
        public readonly float EndTime => startTime + length;
        
        /// <summary>
        /// End time in frames.
        /// </summary>
        public readonly int EndFrame => startFrame + frameCount;
    }

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
        /// The total frame count of the animation.
        /// </summary>
        public int length;

        /// <summary>
        /// The length in seconds of the animation.
        /// </summary>
        public float duration;

        /// <summary>
        /// The list of frame indices where the attached object, such as a collider, is active.
        /// </summary>
        public List<int> framesOn;

        /// <summary>
        /// The list of times (in seconds) corresponding to when the attached object, such as a collider, is active.
        /// </summary>
        public List<float> timesOn;
        
        /// <summary>
        /// The length in seconds of a single frame.
        /// </summary>
        public float FrameDuration => duration / length;

        /// <summary>
        /// Returns a list of time periods during which this sprite object is activated.
        /// </summary>
        /// <param name="periods">A list to populate with the time periods.</param>
        /// <param name="clearList">Whether to clear <paramref name="periods"/> before adding to it.</param>
        /// <returns>The number of time periods added to the list.</returns>
        public int GetKeyframePeriods(ref List<ActivationPeriod> periods, bool clearList = true)
        {
            // Exit early if the output list or the keyframe lists are null.
            if (periods == null || framesOn == null || timesOn == null)
            {
                return 0;
            }
            
            // Exit early if the keyframe lists are improperly set up (this shouldn't happen).
            // Also exit early if the keyframe lists are empty.
            if (framesOn.Count == 0 || framesOn.Count != timesOn.Count)
            {
                return 0;
            }

            // Clear the output list.
            if (clearList)
            {
                periods.Clear();
            }

            // Get the frame duration period once to save calculation time.
            float frameDuration = FrameDuration;

            // Track the number of periods added to the list.
            int count = 0;

            // For each frame in which the component gets activated...
            for (int i = 0; i < framesOn.Count; i++)
            {
                // If we're looking at the very next frame after the previous activation,
                // then increase that period's duration by one frame. 
                if (periods.Count > 0 && framesOn[i] == periods[^1].startFrame + 1)
                {
                    ActivationPeriod  period = periods[^1];
                    period.frameCount++;
                    period.length += frameDuration;
                    periods[^1] = period;
                }
                // If we haven't recorded any activation yet, or we're more than one
                // frame past the previous, start a new activation period.
                else
                {
                    periods.Add(new ActivationPeriod()
                    {
                        startFrame = framesOn[i],
                        startTime = timesOn[i],
                        frameCount = 1,
                        length = frameDuration,
                    });
                    count++;
                }
            }
            
            // Return the number of periods.
            return count;
        }
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