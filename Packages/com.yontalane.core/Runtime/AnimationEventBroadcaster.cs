using UnityEngine;

namespace Yontalane
{
    [AddComponentMenu("Yontalane/Animation Event Broadcaster")]
    [RequireComponent(typeof(Animator))]
    public sealed class AnimationEventBroadcaster : MonoBehaviour
    {
        public delegate void AnimationEventHandler( AnimationEvent animationEvent );
        public static AnimationEventHandler OnAnimationEvent = null;

        public void AnimationEvent( AnimationEvent animationEvent )
        {
            OnAnimationEvent?.Invoke(animationEvent);
        }
    }
}
