using UnityEngine;

namespace Yontalane
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Yontalane/Character Bump Controller")]
    public class CharacterBumpController : MonoBehaviour
    {
        private const float FORCE_SCALAR = 2.5f;

        public delegate void HitHandler(GameObject hitReceiver, CharacterBumpController hitSource, Vector3 force);
        public static HitHandler OnHit = null;

        [SerializeField]
        [Min(0f)]
        private float m_mass = 1f;

        public CharacterController CharacterController { get; private set; } = null;

        private void Start() => CharacterController = GetComponent<CharacterController>();

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.isStatic)
            {
                return;
            }
            if (hit.rigidbody == null)
            {
                return;
            }
            if (hit.rigidbody.isKinematic)
            {
                return;
            }

            float normalDot = Mathf.Clamp01(Vector3.Dot(hit.moveDirection, -1f * hit.normal));
            Vector3 force = m_mass / hit.rigidbody.mass * normalDot * FORCE_SCALAR * hit.moveLength * hit.moveDirection;
            hit.rigidbody.AddForce(force, ForceMode.VelocityChange);
            OnHit?.Invoke(hit.gameObject, this, force);
        }
    }
}