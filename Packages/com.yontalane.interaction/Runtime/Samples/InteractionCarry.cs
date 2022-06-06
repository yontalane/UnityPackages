using UnityEngine;

namespace Yontalane.Interaction.Samples
{
    /// <summary>
    /// The interactor picks up and carries the interactable.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Interaction/Samples/Carry Interaction")]
    public class InteractionCarry : InteractionBase
    {
        private enum Phase
        {
            PickingUp = 0,
            Carrying = 1,
            PuttingDown = 2
        }

        #region Serialized fields
        [Header("Carry Interaction")]
        [SerializeField]
        [Tooltip("The collider associated with this interaction. If left blank, will look for a collider on this object.")]
        private Collider m_collider = null;

        [SerializeField]
        [Tooltip("The interactor's animator parameter for carrying.")]
        private string m_carryAnimatorBool = "Carrying";
        #endregion

        #region Private variables
        private Interactor m_interactor = null;
        private Transform m_carryRoot = null;
        private Phase m_phase = Phase.PickingUp;
        private GameObject m_interactorRoot = null;
        private Vector3 m_carryPositionOffset = new Vector3();
        private Vector3 m_carryRotationOffset = new Vector3();
        #endregion

        private void Reset() => m_interactionType = "Carry";

        private void Start()
        {
            if (m_collider == null)
            {
                m_collider = GetComponent<Collider>();
            }
        }

        #region Position management
        /// <summary>
        /// If we're interacting and we're in the carry phase, then match our position and rotation to that of the carry root object.
        /// </summary>
        private void LateUpdate()
        {
            if (!IsInteracting || m_phase != Phase.Carrying)
            {
                return;
            }
            m_root.transform.position = m_carryRoot.transform.TransformPoint(m_carryPositionOffset);
            m_root.transform.eulerAngles = m_carryRoot.eulerAngles - m_carryRotationOffset;
        }
        #endregion

        #region Interact
        /// <summary>
        /// If we're not already interacting, then initiate the pick-up phase. Otherwise, initiate the drop phase.
        /// </summary>
        /// <param name="info"></param>
        public override void Interact(InteractionInfo info)
        {
            if (!IsInteractableAndInfoValid(info, true))
            {
                return;
            }

            if (!IsInteracting)
            {
                m_carryRoot = (Transform)info.data;
                if (m_carryRoot == null)
                {
                    return;
                }

                IsInteracting = true;
                m_phase = Phase.PickingUp;

                m_interactor = info.interactor;
                m_interactor.IsLocked = true;

                m_interactorRoot = info.rootTransform.gameObject;
                TryKillRigidbodyMovement(m_root);
                Interactable.IgnoreCollision(m_interactorRoot, true);

                info.animator.SetBool(m_carryAnimatorBool, true);
            }
            else if (m_phase == Phase.Carrying)
            {
                m_phase = Phase.PuttingDown;
                info.animator.SetBool(m_carryAnimatorBool, false);
            }
        }

        /// <summary>
        /// An event at the beginning of a carry interaction will switch us from the pick-up phase to the carry phase. An event at the end will end the interaction.
        /// </summary>
        public override void DoEvent()
        {
            if (m_phase == Phase.PickingUp)
            {
                Interactable.IsHighlightVisible = false;
                m_phase = Phase.Carrying;
                Vector3 p1 = m_collider.ClosestPoint(m_carryRoot.position);
                Vector3 p2 = m_carryRoot.position - p1 + m_root.transform.position;
                Vector3 p3 = Vector3.MoveTowards(p2, m_carryRoot.position, Mathf.Max(Vector3.Distance(p2, m_carryRoot.position) - 0.5f, 0f));
                m_carryPositionOffset = m_carryRoot.InverseTransformPoint(p3);
                m_carryRotationOffset = m_carryRoot.eulerAngles - m_root.transform.eulerAngles;
                TrySetRigidbodyKinematic(m_root, true);
            }
            else if (m_phase == Phase.PuttingDown)
            {
                Interactable.IsHighlightVisible = true;
                m_interactor.IsLocked = false;
                Interactable.IgnoreCollision(m_interactorRoot, false);
                TrySetRigidbodyKinematic(m_root, false);
                IsInteracting = false;
                TryKillRigidbodyMovement(m_root);
            }
        }
        #endregion
    }
}