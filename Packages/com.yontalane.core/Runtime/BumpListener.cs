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
        [System.Serializable]
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

            public bool RespondToCollision(BumpListener bumpListener, Collision collision, Func<Vector3, bool> velocityCheck = null)
            {
                if ((m_layerMask.value & (1 << collision.gameObject.layer)) > 0
                    && collision.relativeVelocity.magnitude > m_requiredVelocity
                    && (velocityCheck == null || velocityCheck.Invoke(collision.relativeVelocity)))
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
        #endregion

        #region Private variables
        private AudioSource m_audioSource = null;
        #endregion

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

        private bool LandVelocityCheck(Vector3 v) => Vector3.Dot(v, Vector3.up) > 0.5f;

        private bool BumpVelocityCheck(Vector3 v) => Mathf.Abs(v.y) < new Vector2(v.x, v.z).magnitude * 0.25f;
    }
}
