using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yontalane.Interaction
{
    /// <summary>
    /// Represents an object in the scene that can be interacted with by an Interactor.
    /// Manages references to its root, renderer, animator, audio source, and interaction behaviors.
    /// Handles highlighting and provides access to its interaction components.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Interaction/Interactable")]
    public class Interactable : MonoBehaviour
    {
        public delegate void HighlightHandler(bool isHighlighted, Interactable interactable);
        public static HighlightHandler OnHighlight = null;

        #region Serialized fields
        [SerializeField]
        [Tooltip("This interactable's root gameobject.")]
        private GameObject m_root = null;

        [SerializeField]
        [Tooltip("The renderer attached to this interactable.")]
        private Renderer m_renderer = null;

        [SerializeField]
        [Tooltip("The animator attached to this interactable.")]
        private Animator m_animator = null;

        [SerializeField]
        [Tooltip("The audiosource attached to this interactable.")]
        private AudioSource m_audioSource = null;
        #endregion

        public GameObject Root => m_root;
        public AudioSource AudioSource => m_audioSource;
        public Animator Animator => m_animator;
        public Renderer Renderer => m_renderer;
        public bool IsHighlightVisible
        {
            get => m_isHighlightVisible;
            set
            {
                if (value == m_isHighlightVisible)
                {
                    return;
                }
                m_isHighlightVisible = value;
                if (!IsHighlighted)
                {
                    return;
                }
                OnHighlight?.Invoke(m_isHighlightVisible, this);
            }
        }

        #region Private variables
        private InteractionBase[] m_interactions = null;
        private List<Collider> m_colliders = new List<Collider>();
        private List<Collider> m_targetColliders = new List<Collider>();
        private bool m_isHighlightVisible = true;
        #endregion

        private void Start()
        {
            m_interactions = m_root.GetComponentsInChildren<InteractionBase>();
            for (int i = 0; i < m_interactions.Length; i++)
            {
                m_interactions[i].Interactable = this;
            }

            m_colliders = m_root.GetComponentsInChildren<Collider>().ToList();
            for (int i = m_colliders.Count - 1; i >= 0; i--)
            {
                if (m_colliders[i].gameObject == gameObject)
                {
                    m_colliders.RemoveAt(i);
                }
            }
        }

        #region Interact
        /// <summary>
        /// Try performing an interaction. WIll only work if, one, <c>interactionType</c> matches, and two, there isn't another interaction already in progress.
        /// </summary>
        public bool TryInteraction(string interactionType, InteractionInfo info)
        {
            bool activeExists = TryGetActiveInteraction(out InteractionBase testInteraction);
            foreach(InteractionBase interaction in m_interactions)
            {
                if (interaction.InteractionType == interactionType && (!activeExists || interaction == testInteraction))
                {
                    interaction.Interact(info);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Some interactions may require feedback from the interactor while they're running. For example, if you're pulling a lever, the moment your pull animation gets to the end, you may want to signal an event. At that point, the interaction will then slide open a door.
        /// </summary>
        public void DoEvent()
        {
            if (TryGetActiveInteraction(out InteractionBase interaction))
            {
                interaction.DoEvent();
            }
        }
        #endregion

        #region Highlighting
        /// <summary>
        /// Is this interactable highlighted?
        /// </summary>
        public bool IsHighlighted { get; private set; } = false;

        /// <summary>
        /// Highlight this interactable.
        /// </summary>
        public void SetHighlightOn()
        {
            if (m_isHighlightVisible)
            {
                Highlight(true);
            }
        }

        /// <summary>
        /// Unhighlight this interactable.
        /// </summary>
        public void SetHighlightOff() => Highlight(false);

        /// <summary>
        /// Update the isHighlighted boolean and invoke the onHighlight delegate.
        /// </summary>
        private void Highlight(bool newIsHighlight = true)
        {
            if (newIsHighlight == IsHighlighted)
            {
                return;
            }
            IsHighlighted = newIsHighlight;
            OnHighlight?.Invoke(IsHighlighted, this);
        }
        #endregion

        /// <summary>
        /// Ignore (or stop ignoring) collision on all colliders associated with this interactable and all colliders beneath <c>targetRoot</c>.
        /// </summary>
        public void IgnoreCollision(GameObject targetRoot = null, bool ignore = true)
        {
            if (ignore)
            {
                m_targetColliders = targetRoot.GetComponentsInChildren<Collider>().ToList();
                for (int i = m_targetColliders.Count - 1; i >= 0; i--)
                {
                    if (m_targetColliders[i].isTrigger)
                    {
                        m_targetColliders.RemoveAt(i);
                    }
                }
                for (int i = 0; i < m_colliders.Count; i++)
                {
                    for (int j = 0; j < m_targetColliders.Count; j++)
                    {
                        Physics.IgnoreCollision(m_colliders[i], m_targetColliders[j], true);
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_colliders.Count; i++)
                {
                    for (int j = 0; j < m_targetColliders.Count; j++)
                    {
                        Physics.IgnoreCollision(m_colliders[i], m_targetColliders[j], false);
                    }
                }
                m_targetColliders.Clear();
            }
        }

        #region Interaction helpers
        /// <summary>
        /// See if this interactable contains an interaction of the provided type.
        /// </summary>
        public bool TryGetInteraction<T>(out T interaction) where T : InteractionBase
        {
            for (int i = 0; i < m_interactions.Length; i++)
            {
                if (m_interactions[i] is T)
                {
                    interaction = m_interactions[i] as T;
                    return true;
                }
            }
            interaction = null;
            return false;
        }

        /// <summary>
        /// See if this interactable has a currently active interaction.
        /// </summary>
        public bool TryGetActiveInteraction(out InteractionBase interaction)
        {
            foreach (InteractionBase testInteraction in m_interactions.Where(testInteraction => testInteraction.IsInteracting))
            {
                interaction = testInteraction;
                return true;
            }

            interaction = null;
            return false;
        }

        /// <summary>
        /// Is any interaction on this interactable currently active?
        /// </summary>
        public bool IsInteracting => TryGetActiveInteraction(out _);
        #endregion
    }
}