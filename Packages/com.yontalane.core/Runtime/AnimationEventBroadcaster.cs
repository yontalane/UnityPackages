using UnityEngine;
using UnityEngine.Events;

namespace Yontalane
{
    /// <summary>
    /// A component that broadcasts animation events.
    /// </summary>
    [AddComponentMenu("Yontalane/Animation Event Broadcaster")]
    [RequireComponent(typeof(Animator))]
    public sealed class AnimationEventBroadcaster : MonoBehaviour
    {
        /// <summary>
        /// A delegate that represents a method that handles an animation event.
        /// </summary>
        [System.Serializable]
        public class AnimationUnityEventHandler : UnityEvent<AnimationEvent> { }
        
        /// <summary>
        /// A delegate that represents a method that handles an animation event.
        /// </summary>
        public delegate void AnimationEventHandler( AnimationEvent animationEvent );

        [Tooltip("An event that is triggered when an animation event occurs.")]
        [SerializeField]
        private AnimationUnityEventHandler m_onEvent;

        /// <summary>
        /// An event that is triggered when an animation event occurs.
        /// </summary>
        public AnimationEventHandler OnAnimationEventLocal = null;

        /// <summary>
        /// An event that is triggered when an animation event occurs.
        /// </summary>
        public static AnimationEventHandler OnAnimationEvent = null;

        /// <summary>
        /// Invokes the animation event.
        /// </summary>
        public void AnimationEvent(AnimationEvent animationEvent)
        {
            m_onEvent?.Invoke(animationEvent);
            OnAnimationEventLocal?.Invoke(animationEvent);
            OnAnimationEvent?.Invoke(animationEvent);
        }
    }
}
