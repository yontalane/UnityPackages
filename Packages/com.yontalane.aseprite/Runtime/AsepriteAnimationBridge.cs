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
    /// A bridge between Aseprite animations and Unity Animator.
    /// Facilitates interaction between Aseprite animations and Unity's Animator by handling root motion events, animation lifecycle events, and providing utility methods for animation control and querying.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Yontalane/Aseprite/Aseprite Animation Bridge")]
    public class AsepriteAnimationBridge : MonoBehaviour
    {
        private Animator m_animator = null;

        /// <summary>
        /// UnityEvent that is invoked with a Vector2 representing root motion data from an Aseprite animation.
        /// </summary>
        [System.Serializable]
        public class OnMotionHandler : UnityEvent<Vector2> { }

        /// <summary>
        /// UnityEvent that is invoked upon animation lifecycle events.
        /// </summary>
        [System.Serializable]
        public class OnLifecycleHandler : UnityEvent<AnimationLifecycleEvent> { }

        [Tooltip("Event invoked when root motion data is received from the Aseprite animation.")]
        [SerializeField]
        private OnMotionHandler m_onMotion = null;

        [Tooltip("Event invoked when the animation starts.")]
        [SerializeField]
        private OnLifecycleHandler m_onStart = null;

        [Tooltip("Event invoked when the animation completes.")]
        [SerializeField]
        private OnLifecycleHandler m_onComplete = null;

        /// <summary>
        /// Event invoked when root motion data is received from the Aseprite animation.
        /// </summary>
        public OnMotionHandler OnMotion => m_onMotion;

        /// <summary>
        /// Event invoked when the animation starts.
        /// </summary>
        public OnLifecycleHandler OnStart => m_onStart;

        /// <summary>
        /// Event invoked when the animation completes.
        /// </summary>
        public OnLifecycleHandler OnComplete => m_onComplete;

        /// <summary>
        /// The Animator component attached to the GameObject.
        /// </summary>
        public Animator Animator
        {
            get
            {
                // If the Animator component is not assigned, get it from the GameObject
                if (m_animator == null)
                {
                    m_animator = GetComponent<Animator>();
                }

                // Return the Animator component
                return m_animator;
            }
        }

        /// <summary>
        /// Gets the name of the currently playing animation.
        /// </summary>
        public string CurrentAnimation { get; private set; } = string.Empty;

        /// <summary>
        /// Parses the root motion data from the Aseprite animation and invokes the OnMotion event with the parsed motion vector.
        /// </summary>
        /// <param name="position">The root motion data from the Aseprite animation.</param>
        public void OnAsepriteRootMotion(string position)
        {
            // Split the position string into an array of strings
            string[] pos = position.Split(',');
            float x = float.TryParse(pos[0].Trim(), out float outX) ? outX : 0f;
            float y = float.TryParse(pos[1].Trim(), out float outY) ? outY : 0f;

            // Invoke the OnMotion event with the parsed motion vector
            OnMotion?.Invoke(new Vector2(x, y));
        }

        /// <summary>
        /// Invoked when the animation completes.
        /// </summary>
        /// <param name="animationEvent">The AnimationEvent that triggered the start.</param>
        public void OnAsepriteAnimationStart(AnimationEvent animationEvent)
        {
            CurrentAnimation = animationEvent.stringParameter;
            OnStart?.Invoke(new()
            {
                animationName = animationEvent.stringParameter,
                isLooping = animationEvent.intParameter > 0,
            });
        }

        /// <summary>
        /// Invoked when the animation completes.
        /// </summary>
        /// <param name="animationEvent">The AnimationEvent that triggered the completion.</param>
        public void OnAsepriteAnimationComplete(AnimationEvent animationEvent)
        {
            OnComplete?.Invoke(new()
            {
                animationName = animationEvent.stringParameter,
                isLooping = animationEvent.intParameter > 0,
            });
        }

        /// <summary>
        /// Tries to get an animation clip with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to get.</param>
        /// <param name="animationClip">The animation clip if found, otherwise null.</param>
        /// <returns>True if the animation clip was found, false otherwise.</returns>
        public bool TryGetAnimationClip(string animationName, out AnimationClip animationClip)
        {
            // Get the RuntimeAnimatorController from the Animator
            RuntimeAnimatorController controller = Animator.runtimeAnimatorController;

            // If the RuntimeAnimatorController is not assigned, return false
            if (controller == null)
            {
                animationClip = null;
                return false;
            }

            // Iterate through the animation clips in the RuntimeAnimatorController
            for (int i = 0; i < controller.animationClips.Length; i++)
            {
                // If the animation clip's name is not the same as the specified animation name, continue
                if (controller.animationClips[i].name != animationName)
                {
                    continue;
                }

                // Set the animation clip and return true
                animationClip = controller.animationClips[i];
                return true;
            }

            // If no animation clip's name is the same as the specified animation name, return false
            animationClip = null;
            return false;
        }

        /// <summary>
        /// Checks if the Animator has an animation with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to check for.</param>
        /// <returns>True if the Animator has an animation with the specified name, false otherwise.</returns>
        public bool HasAnimation(string animationName)
        {
            // Try to get the animation clip with the specified name
            return TryGetAnimationClip(animationName, out _);
        }

        /// <summary>
        /// Tries to play an animation with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="startTime">The start time offset between zero and one.</param>
        /// <returns>True if the animation was played, false otherwise.</returns>
        public bool TryPlay(string animationName, float startTime = 0f)
        {
            // Try to get the animation clip with the specified name
            if (!TryGetAnimationClip(animationName, out AnimationClip clip))
            {
                return false;
            }

            // Play the animation
            Animator.Play(clip.name, -1, startTime);
            return true;
        }

        /// <summary>
        /// Plays an animation with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="startTime">The start time offset between zero and one.</param>
        public void Play(string animationName, float startTime = 0f)
        {
            // Play the animation
            TryPlay(animationName, startTime);
        }
    }
}
