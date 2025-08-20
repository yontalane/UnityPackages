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
    /// A bridge between Aseprite animations and Unity Animator.
    /// Facilitates interaction between Aseprite animations and Unity's Animator by handling root motion events, animation lifecycle events, and providing utility methods for animation control and querying.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Yontalane/Aseprite/Aseprite Animation Bridge")]
    public class AsepriteAnimationBridge : MonoBehaviour
    {
        #region Event Types

        /// <summary>
        /// UnityEvent that is invoked with a Vector2 representing root motion data from an Aseprite animation.
        /// </summary>
        [System.Serializable]
        public class OnMotionHandler : UnityEvent<AnimationMotionEvent> { }

        /// <summary>
        /// UnityEvent that is invoked upon animation lifecycle events.
        /// </summary>
        [System.Serializable]
        public class OnLifecycleHandler : UnityEvent<AnimationLifecycleEvent> { }

        #endregion

        #region Private Fields

        private Animator m_animator = null;
        private SpriteRenderer m_spriteRenderer = null;

        #endregion

        #region Serialized Fields

        [Tooltip("Event invoked when root motion data is received from the Aseprite animation.")]
        [SerializeField]
        private OnMotionHandler m_onMotion = null;

        [Tooltip("Event invoked when the animation starts.")]
        [SerializeField]
        private OnLifecycleHandler m_onStart = null;

        [Tooltip("Event invoked when the animation completes.")]
        [SerializeField]
        private OnLifecycleHandler m_onComplete = null;

        #endregion

        #region Events

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

        #endregion

        #region Component Properties

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
        /// Gets the SpriteRenderer.
        /// </summary>
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                // Check if the SpriteRenderer reference is null and assign it if necessary
                if (m_spriteRenderer == null)
                {
                    m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }

                // Return the cached SpriteRenderer component
                return m_spriteRenderer;
            }
        }

        #endregion

        #region Sprite Properties

        /// <summary>
        /// The Sprite to render.
        /// </summary>
        public Sprite Sprite => SpriteRenderer.sprite;

        /// <summary>
        /// Rendering color for the Sprite graphic.
        /// </summary>
        public Color Color
        {
            get => SpriteRenderer.color;
            set => SpriteRenderer.color = value;
        }

        /// <summary>
        /// Renderer's order within a sorting layer.
        /// </summary>
        public int SortingOrder
        {
            get => SpriteRenderer.sortingOrder;
            set => SpriteRenderer.sortingOrder = value;
        }

        /// <summary>
        /// Flips the sprite on the X axis.
        /// </summary>
        public bool FlipX
        {
            get => SpriteRenderer.flipX;
            set => SpriteRenderer.flipX = value;
        }

        /// <summary>
        /// Flips the sprite on the Y axis.
        /// </summary>
        public bool FlipY
        {
            get => SpriteRenderer.flipY;
            set => SpriteRenderer.flipY = value;
        }

        /// <summary>
        /// Gets the name of the currently playing animation.
        /// </summary>
        public string CurrentAnimation { get; private set; } = string.Empty;

        #endregion


        #region Aseprite Animation Events

        /// <summary>
        /// Parses the root motion data from the Aseprite animation and invokes the OnMotion event with the parsed motion vector.
        /// </summary>
        /// <param name="position">The root motion data from the Aseprite animation.</param>
        public void OnAsepriteRootMotion(string position)
        {
            // Split the position string into an array of strings
            string[] pos = position.Split(',');
            int frameIndex = int.TryParse(pos[0].Trim(), out int outFrameIndex) ? outFrameIndex : -1;
            float x = float.TryParse(pos[1].Trim(), out float outX) ? outX : 0f;
            float y = float.TryParse(pos[2].Trim(), out float outY) ? outY : 0f;

            // Invoke the OnMotion event with the parsed motion vector
            OnMotion?.Invoke(new()
            {
                frameIndex = frameIndex,
                motion = new Vector2(x, y),
            });
        }

        /// <summary>
        /// Invoked when the animation starts.
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

        #endregion

        #region Animation Clip Utilities

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

        #endregion

        #region Animation Playback

        /// <summary>
        /// Tries to play an animation with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="startTime">The start time offset between zero and one.</param>
        /// <param name="restartLoop">Whether to restart the animation loop if the animation is looping and the current animation is the same as the specified animation name.</param>
        /// <returns>True if the animation was played, false otherwise.</returns>
        public bool TryPlay(string animationName, float startTime = 0f, bool restartLoop = false)
        {
            // Try to get the animation clip with the specified name
            if (!TryGetAnimationClip(animationName, out AnimationClip clip))
            {
                return false;
            }

            // If the animation is looping and the current animation is the same as the specified animation name, and restartLoop is false, return false
            if (!restartLoop && clip.isLooping && CurrentAnimation == animationName)
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
        /// <param name="restartLoop">Whether to restart the animation loop if the animation is looping and the current animation is the same as the specified animation name.</param>
        public void Play(string animationName, float startTime = 0f, bool restartLoop = false)
        {
            // Play the animation
            TryPlay(animationName, startTime, restartLoop);
        }

        #endregion
    }
}
