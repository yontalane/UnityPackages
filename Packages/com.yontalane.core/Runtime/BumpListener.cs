using System;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Yontalane/Bump Listener")]
    public class BumpListener : MonoBehaviour
    {
        #region Event info
        [Serializable]
        private class BumpInfo
        {
            #region Serialized fields
            [SerializeField]
            private LayerMask m_layerMask = ~0;

            [SerializeField]
            [Min(0f)]
            private float m_requiredVelocity = 0f;

            [SerializeField]
            private UnityEvent m_onCollision = null;

            [SerializeField]
            private AudioClip m_audioClip = null;
            #endregion

            public bool RespondToCollision(BumpListener bumpListener, GameObject other, Vector3 velocity, Func<Vector3, bool> velocityCheck = null)
            {
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

            public bool RespondToCollision(BumpListener bumpListener, Collision collision, Func<Vector3, bool> velocityCheck = null) => RespondToCollision(bumpListener, collision.gameObject, collision.relativeVelocity, velocityCheck);
        }
        #endregion

        public AudioSource AudioSource
        {
            get
            {
                if (m_audioSource == null)
                {
                    if (!TryGetComponent(out m_audioSource))
                    {
                        m_audioSource = gameObject.AddComponent<AudioSource>();
                    }
                }
                return m_audioSource;
            }
        }

        #region Serialized fields
        [SerializeField]
        private BumpInfo m_onLand = new BumpInfo();

        [SerializeField]
        private BumpInfo m_onBump = new BumpInfo();

        [Space]
        [SerializeField]
        private bool m_listenForCharacterControllerBump = false;

        [SerializeField]
        private BumpInfo m_onCharacterControllerBump = new BumpInfo();
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

        private void CharacterBumpController_OnHit(GameObject hitReceiver, CharacterBumpController hitSource, Vector3 force)
        {
            if (hitReceiver != gameObject)
            {
                return;
            }
            _ = m_onCharacterControllerBump.RespondToCollision(this, hitSource.gameObject, force);
        }

        private bool LandVelocityCheck(Vector3 v) => Vector3.Dot(v, Vector3.up) > 0.5f;

        private bool BumpVelocityCheck(Vector3 v) => Mathf.Abs(v.y) < new Vector2(v.x, v.z).magnitude * 0.25f;
    }
}
