using UnityEngine;

namespace Yontalane.Interaction.Samples
{
    /// <summary>
    /// Move the interactor to a specified point next to the interactable. Play an animation on each.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Interaction/Samples/Simple Interaction")]
    public class InteractionSimple : InteractionBase
    {
        #region Serialized fields
        [Header("Simple")]
        [SerializeField]
        [Tooltip("The interactable's animator trigger for starting this interaction.")]
        private string m_interactableStartTrigger = string.Empty;

        [SerializeField]
        [Tooltip("The interactable's animator trigger for ending this interaction.")]
        private string m_interactableStopTrigger = string.Empty;

        [SerializeField]
        [Tooltip("The interactor's animator trigger for starting this interaction.")]
        private string m_interactorStartTrigger = string.Empty;

        [SerializeField]
        [Tooltip("The interactor's animator trigger for ending this interaction.")]
        private string m_interactorStopTrigger = string.Empty;

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

            Interactable.IsHighlightVisible = !IsInteracting;

            Interactable.IgnoreCollision(info.rootTransform.gameObject, IsInteracting);

            TryKillRigidbodyMovement(m_root);

            if (IsInteracting)
            {
                info.interactor.IsLocked = true;
                Teleport(info.rootTransform, InteractPosition);
                info.rootTransform.LookAt(InteractPosition + 100f * InteractDirection, Vector3.up);
            }
            else
            {
                info.interactor.IsLocked = false;
            }

            if (IsInteracting)
            {
                if (!string.IsNullOrEmpty(m_interactableStartTrigger) && Interactable.Animator != null)
                {
                    Interactable.Animator.SetTrigger(m_interactableStartTrigger);
                }
                if (!string.IsNullOrEmpty(m_interactorStartTrigger) && info.animator != null)
                {
                    info.animator.SetTrigger(m_interactorStartTrigger);
                }
            }
            else if (!IsInteracting)
            {
                if (!string.IsNullOrEmpty(m_interactableStopTrigger) && Interactable.Animator != null)
                {
                    Interactable.Animator.SetTrigger(m_interactableStopTrigger);
                }
                if (!string.IsNullOrEmpty(m_interactorStopTrigger) && info.animator != null)
                {
                    info.animator.SetTrigger(m_interactorStopTrigger);
                }
            }
        }

        private void Teleport(Transform transform, Vector3 targetPosition)
        {
            bool hasRB = transform.TryGetComponent(out Rigidbody rb);
            bool hasCC = transform.TryGetComponent(out CharacterController cc);
            bool prevRB_DC = true;
            bool prevCC_DC = true;
            bool prevCC_En = true;

            if (hasRB)
            {
                prevRB_DC = rb.detectCollisions;
                rb.detectCollisions = false;
            }
            if (hasCC)
            {
                prevCC_DC = cc.detectCollisions;
                cc.detectCollisions = false;
                prevCC_En = cc.enabled;
                cc.enabled = false;
            }

            transform.position = targetPosition;

            if (hasRB)
            {
                rb.detectCollisions = prevRB_DC;
            }
            if (hasCC)
            {
                cc.detectCollisions = prevCC_DC;
                cc.enabled = prevCC_En;
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