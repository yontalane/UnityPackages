using UnityEngine;

namespace Yontalane.Interaction
{
    public class InteractionSimple : InteractionBase
    {
        #region Serialized fields
        [Header("Simple")]
        [SerializeField]
        [Tooltip("The interactor's animator trigger for starting this interaction.")]
        private string m_startInteractAnimatorTrigger = string.Empty;

        [SerializeField]
        [Tooltip("The interactor's animator trigger for ending this interaction.")]
        private string m_stopInteractAnimatorTrigger = string.Empty;

        [SerializeField]
        [Tooltip("When interacting with this object, teleport the interactor to this position (interaction local space).")]
        private Vector3 m_interactPosition = Vector3.zero;

        [SerializeField]
        [Tooltip("When interacting with this object, teleport the interactor to this orientation (interaction local space).")]
        private Vector3 m_interactDirection = Vector3.back;

        [SerializeField]
        [Tooltip("Play this audio clip when DoEvent() is called.")]
        private AudioClip m_eventAudioClip = null;
        #endregion

        public Vector3 InteractPosition => transform.TransformPoint(m_interactPosition);
        public Vector3 InteractDirection => transform.TransformDirection(m_interactDirection).normalized;

        private void Reset() => m_interactionType = "Action";

        #region Interacting
        /// <summary>
        /// If we're not already interacting, then teleport the interactor to the designated position and trigger its animation. If we are interacting, then trigger a different animation.
        /// </summary>
        public override void Interact(InteractionInfo info)
        {
            if (!IsInteractableAndInfoValid(info, false))
            {
                return;
            }

            IsInteracting = !IsInteracting;

            Interactable.IgnoreCollision(info.rootTransform.gameObject, IsInteracting);

            TryKillRigidbodyMovement(m_root);

            if (IsInteracting)
            {
                info.interactor.IsLocked = true;
                info.rootTransform.position = InteractPosition;
                info.rootTransform.LookAt(InteractPosition + 100f * InteractDirection, Vector3.up);
            }
            else
            {
                info.interactor.IsLocked = false;
            }

            if (IsInteracting && !string.IsNullOrEmpty(m_startInteractAnimatorTrigger))
            {
                info.animator.SetTrigger(m_startInteractAnimatorTrigger);
            }
            else if (!IsInteracting && !string.IsNullOrEmpty(m_stopInteractAnimatorTrigger))
            {
                info.animator.SetTrigger(m_stopInteractAnimatorTrigger);
            }
        }

        /// <summary>
        /// If an audioclip exists, and if an audiosource is attached to this gameobject, then play the clip.
        /// </summary>
        public override void DoEvent()
        {
            if (m_eventAudioClip == null)
            {
                return;
            }
            if (!TryGetComponent(out AudioSource audioSource))
            {
                return;
            }
            audioSource.PlayOneShot(m_eventAudioClip);
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(InteractPosition, 0.15f);
            Gizmos.DrawLine(InteractPosition, InteractPosition + 0.25f * InteractDirection);
        }
        #endregion
    }
}