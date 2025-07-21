using System;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane
{
    /// <summary>
    /// A component that listens for bumps and invokes events when a bump occurs.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Yontalane/Bump Listener")]
    public class BumpListener : MonoBehaviour
    {
        #region Event info
        /// <summary>
        /// A class that contains information about a bump.
        /// </summary>
        [Serializable]
        private class BumpInfo
        {
            #region Serialized fields
            [Tooltip("The layer mask of the colliders that will trigger the bump event.")]
            [SerializeField]
            private LayerMask m_layerMask = ~0;

            [Tooltip("The minimum velocity required for the bump event to be triggered.")]
            [SerializeField]
            [Min(0f)]
            private float m_requiredVelocity = 0f;

            [Tooltip("The event that is invoked when a bump occurs.")]
            [SerializeField]
            private UnityEvent m_onCollision = null;

            [Tooltip("The audio clip that is played when a bump occurs.")]
            [SerializeField]
            private AudioClip m_audioClip = null;
            #endregion

            /// <summary>
            /// Responds to a collision event.
            /// </summary>
            public bool RespondToCollision(BumpListener bumpListener, GameObject other, Vector3 velocity, Func<Vector3, bool> velocityCheck = null)
            {
                // If the other object is on the correct layer and has sufficient velocity, invoke the event and play the audio clip.
                if ((m_layerMask.value & (1 << other.layer)) > 0
                    && velocity.magnitude > m_requiredVelocity
                    && (velocityCheck == null || velocityCheck.Invoke(velocity)))
                {
                    
                    m_onCollision?.Invoke();

                    if (m_audioClip != null)
                    {
                        bumpListener.AudioSource.PlayOneShot(m_audioClip);
                    }

                    return true;
                }
                return false;
            }

            // Responds to a collision event using a Collision object.
            public bool RespondToCollision(BumpListener bumpListener, Collision collision, Func<Vector3, bool> velocityCheck = null) => RespondToCollision(bumpListener, collision.gameObject, collision.relativeVelocity, velocityCheck);
        }
        #endregion

        /// <summary>
        /// Gets the AudioSource component attached to this GameObject.
        /// </summary>
        public AudioSource AudioSource
        {
            get
            {
                // If the AudioSource has not been cached, try to get it from the GameObject; if not found, add a new one.
                if (m_audioSource == null && !TryGetComponent(out m_audioSource))
                {
                    m_audioSource = gameObject.AddComponent<AudioSource>();
                }

                return m_audioSource;
            }
        }

        #region Serialized fields
        [Tooltip("The event that is invoked when a bump occurs.")]
        [SerializeField]
        private BumpInfo m_onLand = new();

        [Tooltip("The event that is invoked when a bump occurs.")]
        [SerializeField]
        private BumpInfo m_onBump = new();

        [Space]

        [Tooltip("Whether to listen for bumps from a CharacterController.")]
        [SerializeField]
        private bool m_listenForCharacterControllerBump = false;

        [Tooltip("The event that is invoked when a bump occurs.")]
        [SerializeField]
        private BumpInfo m_onCharacterControllerBump = new();
        #endregion

        #region Private variables
        private AudioSource m_audioSource = null;
        #endregion

        private void OnEnable()
        {
            if (m_listenForCharacterControllerBump)
            {
                CharacterBumpController.OnHit += CharacterBumpController_OnHit;
            }
        }

        private void OnDisable()
        {
            if (m_listenForCharacterControllerBump)
            {
                CharacterBumpController.OnHit -= CharacterBumpController_OnHit;
            }
        }

        /// <summary>
        /// Called when a collision occurs.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (m_onLand.RespondToCollision(this, collision, LandVelocityCheck))
            {
                return;
            }
            if (m_onBump.RespondToCollision(this, collision, BumpVelocityCheck))
            {
                return;
            }
        }

        /// <summary>
        /// Called when a CharacterController hits a collider.
        /// </summary>
        private void CharacterBumpController_OnHit(GameObject hitReceiver, CharacterBumpController hitSource, Vector3 force)
        {
            if (hitReceiver != gameObject)
            {
                return;
            }
            _ = m_onCharacterControllerBump.RespondToCollision(this, hitSource.gameObject, force);
        }

        /// <summary>
        /// Checks if the velocity is a land velocity.
        /// </summary>
        private bool LandVelocityCheck(Vector3 v) => Vector3.Dot(v, Vector3.up) > 0.5f;

        /// <summary>
        /// Checks if the velocity is a bump velocity.
        /// </summary>
        private bool BumpVelocityCheck(Vector3 v) => Mathf.Abs(v.y) < new Vector2(v.x, v.z).magnitude * 0.25f;
    }
}
