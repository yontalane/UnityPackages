using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A component that transforms the GameObject based on animation events.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Yontalane/Animation Event Transform")]
    public sealed class AnimationEventTransform : MonoBehaviour
    {
        /// <summary>
        /// Moves the GameObject based on the animation event.
        /// </summary>
        public void Move(AnimationEvent animationEvent)
        {
            if (!TryGetVector3(animationEvent, out Vector3 v))
            {
                return;
            }
            transform.Translate(v);
        }

        /// <summary>
        /// Sets the position of the GameObject based on the animation event.
        /// </summary>
        public void SetPosition(AnimationEvent animationEvent)
        {
            if (!TryGetVector3(animationEvent, out Vector3 v))
            {
                return;
            }
            transform.position = v;
        }

        /// <summary>
        /// Sets the euler angles of the GameObject based on the animation event.
        /// </summary>
        public void SetEulerAngles(AnimationEvent animationEvent)
        {
            if (!TryGetVector3(animationEvent, out Vector3 v))
            {
                return;
            }
            transform.eulerAngles = v;
        }

        /// <summary>
        /// Rotates the GameObject based on the animation event.
        /// </summary>
        public void Rotate(AnimationEvent animationEvent)
        {
            if (TryGetVector3(animationEvent, out Vector3 v))
            {
                transform.eulerAngles += v;
            }
            else if (!Mathf.Approximately(animationEvent.floatParameter, 0f))
            {
                transform.Rotate(Vector3.up, animationEvent.floatParameter);
            }
        }

        /// <summary>
        /// Tries to get the vector3 from the animation event.
        /// </summary>
        private bool TryGetVector3(AnimationEvent animationEvent, out Vector3 vector3)
        {
            vector3 = default;

            string[] s = animationEvent.stringParameter.Split(',');

            if (s.Length != 3)
            {
                return false;
            }

            float[] n = new float[s.Length];

            if (!float.TryParse(s[0].Trim(), out float x))
            {
                return false;
            }
            n[0] = x;

            if (!float.TryParse(s[1].Trim(), out float y))
            {
                return false;
            }
            n[1] = y;

            if (!float.TryParse(s[2].Trim(), out float z))
            {
                return false;
            }
            n[2] = z;

            vector3 = new Vector3(n[0], n[1], n[2]);
            return true;
        }

        /// <summary>
        /// Sets the scale of the GameObject based on the animation event.
        /// </summary>
        public void SetScale(AnimationEvent animationEvent)
        {
            if (!TryGetVector3(animationEvent, out Vector3 v))
            {
                return;
            }
            transform.localScale = v;
        }
    }
}