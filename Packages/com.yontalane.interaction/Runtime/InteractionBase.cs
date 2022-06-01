using UnityEngine;

namespace Yontalane.Interaction
{
    public abstract class InteractionBase : MonoBehaviour
    {
        public Interactable Interactable { get; set; } = null;

        #region Serialized fields
        [Header("Interaction")]
        [SerializeField]
        [Tooltip("This interaction's type. Interactors can only interact with one type at a time.")]
        protected string m_interactionType = string.Empty;
        public string InteractionType => m_interactionType;

        [SerializeField]
        [Tooltip("The root object of this interaction. If left blank, this will default to this object's gameobject.")]
        protected GameObject m_root = null;
        #endregion

        private void Awake()
        {
            if (m_root == null)
            {
                m_root = gameObject;
            }
        }

        #region Interacting
        /// <summary>
        /// Is the interaction currently happening?
        /// </summary>
        public virtual bool IsInteracting { get; protected set; } = false;

        /// <summary>
        /// Initiate the interaction.
        /// </summary>
        public abstract void Interact(InteractionInfo info);

        /// <summary>
        /// Some interactions may require feedback from the interactor while they're running. For example, if you're pulling a lever, the moment your pull animation gets to the end, you may want to signal an event. At that point, the interaction will then slide open a door.
        /// </summary>
        public virtual void DoEvent() { }
        #endregion

        /// <summary>
        /// Does the interactable object exist? (All interactions need to be connected to an interactable.) And is the provided info properly set up?
        /// </summary>
        protected bool IsInteractableAndInfoValid(InteractionInfo info, bool includeData = false)
        {
            if (Interactable == null)
            {
                return false;
            }
            if (info.interactor == null)
            {
                return false;
            }
            if (info.rootTransform == null)
            {
                return false;
            }
            if (info.animator == null)
            {
                return false;
            }
            if (includeData && info.data == null)
            {
                return false;
            }
            return true;
        }

        #region Physics helpers
        /// <summary>
        /// Presumably this will be called on <c>m_root</c>. If a rigidbody is attached to the gameobject, then zero out its velocity.
        /// </summary>
        protected bool TryKillRigidbodyMovement(GameObject gameObject)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                return false;
            }
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            return true;
        }

        /// <summary>
        /// Presumably this will be called on <c>m_root</c>. If a rigidbody is attached to the gameobject, then toggle its kinematic state.
        /// </summary>
        protected bool TrySetRigidbodyKinematic(GameObject gameObject, bool isKinematic)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                return false;
            }
            rigidbody.isKinematic = isKinematic;
            return true;
        }
        #endregion
    }
}