using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A component that allows a CharacterController to push other rigidbodies when colliding.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Yontalane/Character Bump Controller")]
    public class CharacterBumpController : MonoBehaviour
    {
        private const float FORCE_SCALAR = 2.5f;

        /// <summary>
        /// A delegate that represents a method that handles a hit event.
        /// </summary>
        public delegate void HitHandler(GameObject hitReceiver, CharacterBumpController hitSource, Vector3 force);

        /// <summary>
        /// An event that is triggered when a hit occurs.
        /// </summary>
        public static HitHandler OnHit = null;

        [Tooltip("The mass of the CharacterController.")]
        [SerializeField]
        [Min(0f)]
        private float m_mass = 1f;

        /// <summary>
        /// The CharacterController component attached to this GameObject.
        /// </summary>
        public CharacterController CharacterController { get; private set; } = null;

        private void Start() => CharacterController = GetComponent<CharacterController>();

        /// <summary>
        /// Called when the CharacterController hits a collider.
        /// </summary>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If the hit object is static, do nothing.
            if (hit.gameObject.isStatic)
            {
                return;
            }

            // If the hit object has no rigidbody, do nothing.
            if (hit.rigidbody == null)
            {
                return;
            }

            // If the hit object is kinematic, do nothing.
            if (hit.rigidbody.isKinematic)
            {
                return;
            }

            // Calculate the force to apply to the hit object.
            float normalDot = Mathf.Clamp01(Vector3.Dot(hit.moveDirection, -1f * hit.normal));
            Vector3 force = m_mass / hit.rigidbody.mass * normalDot * FORCE_SCALAR * hit.moveLength * hit.moveDirection;

            // Apply the force to the hit object.
            hit.rigidbody.AddForce(force, ForceMode.VelocityChange);
            OnHit?.Invoke(hit.gameObject, this, force);
        }
    }
}