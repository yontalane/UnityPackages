using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// Receives root motion data from Aseprite animations and invokes a UnityEvent with the parsed motion vector.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Yontalane/Aseprite/Aseprite Motion Receiver")]
    public class AsepriteMotionReceiver : MonoBehaviour
    {
        /// <summary>
        /// UnityEvent that is invoked with a Vector2 representing root motion data from an Aseprite animation.
        /// </summary>
        [System.Serializable]
        public class OnMotionHandler : UnityEvent<Vector2> { }

        [SerializeField]
        [Tooltip("Event invoked when root motion data is received from the Aseprite animation.")]
        private OnMotionHandler m_onMotion = null;

        /// <summary>
        /// Event invoked when root motion data is received from the Aseprite animation.
        /// </summary>
        public OnMotionHandler OnMotion => m_onMotion;

        /// <summary>
        /// Parses the root motion data from the Aseprite animation and invokes the OnMotion event with the parsed motion vector.
        /// </summary>
        /// <param name="position">The root motion data from the Aseprite animation.</param>
        public void OnAsepriteRootMotion(string position)
        {
            string[] pos = position.Split(',');
            float x = float.TryParse(pos[0].Trim(), out float outX) ? outX : 0f;
            float y = float.TryParse(pos[1].Trim(), out float outY) ? outY : 0f;

            OnMotion?.Invoke(new Vector2(x, y));
        }
    }
}
