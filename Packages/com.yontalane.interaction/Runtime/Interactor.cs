using System;
using UnityEngine;

namespace Yontalane.Interaction
{
    public struct InteractionInfo
    {
        /// <summary>
        /// The interactor involved in the interaction.
        /// </summary>
        public Interactor interactor;

        /// <summary>
        /// The interactor's root transform. Required if the interaction needs to move the interactor.
        /// </summary>
        public Transform rootTransform;

        /// <summary>
        /// An animator attached to the interactor. Required if the interaction needs to play an animation.
        /// </summary>
        public Animator animator;

        /// <summary>
        /// A generic data object to be used for interaction special cases.
        /// </summary>
        public UnityEngine.Object data;
    }

    /// <summary>
    /// Handles detection and interaction with nearby interactable objects, including raycasting for interactables,
    /// managing interaction events, and optionally highlighting interactables.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Interaction/Interactor")]
    public class Interactor : MonoBehaviour
    {
        public delegate void InteractorEventHandler(Interactable interactable);

        /// <summary>
        /// When the interactor detects an interactable.
        /// </summary>
        public InteractorEventHandler OnInteractableEnter = null;

        /// <summary>
        /// When the interactor stops detecting an interactable.
        /// </summary>
        public InteractorEventHandler OnInteractableExit = null;

        private const float RAY_COUNT = 4;

        #region Serialized fields
        [Header("Raycasting")]
        [SerializeField]
        [Tooltip("Filter raycast hits using this layer mask.")]
        private LayerMask m_targetLayer = ~0;

        [SerializeField]
        [Tooltip("Rays will emit from these points above the interactor's position.")]
        private float[] m_origins = { 0.1f, 0.75f };

        [SerializeField]
        [Min(0f)]
        [Tooltip("Rays will project a maximum of this distance.")]
        private float m_distance = 1f;

        [SerializeField]
        [Range(0f, 90f)]
        [Tooltip("The rays will have a spread of twice this value in front of the interactor.")]
        private float m_angle = 30f;

        [Header("Extras")]
        [SerializeField]
        [Tooltip("Should nearby interactable objects be highlighted? You might turn this on for player characters and off for non-player characters.")]
        private bool m_highlightInteractables = true;
        #endregion

        #region Private variables
        private Interactable m_currentInteractable = null;
        private Interactable m_lockedInteractable = null;
        #endregion

        #region Detecting nearby interactable objects
        private void LateUpdate() => CheckNow();

        /// <summary>
        /// Check if there are any interactables in proximity.
        /// </summary>
        public void CheckNow()
        {
            if (IsLocked && m_currentInteractable != null)
            {
                return;
            }
            foreach(float y in m_origins)
            {
                if (TryCheckFromOrigin(transform.position + y * Vector3.up, out Interactable interactable))
                {
                    if (m_currentInteractable == interactable)
                    {
                        return;
                    }
                    else if (m_currentInteractable == null || (m_currentInteractable != interactable && Vector3.Distance(transform.position, interactable.transform.position) < Vector3.Distance(transform.position, m_currentInteractable.transform.position)))
                    {
                        m_currentInteractable = interactable;
                        if (m_highlightInteractables)
                        {
                            m_currentInteractable.SetHighlightOn();
                        }
                        OnInteractableEnter?.Invoke(m_currentInteractable);
                        return;
                    }
                }
            }
            if (m_currentInteractable != null)
            {
                m_currentInteractable.SetHighlightOff();
                OnInteractableExit?.Invoke(m_currentInteractable);
                m_currentInteractable = null;
            }
        }

        /// <summary>
        /// Project a spread of raycasts from a single point. If they hit an interactable, return it.
        /// </summary>
        private bool TryCheckFromOrigin(Vector3 origin, out Interactable interactable)
        {
            Vector3 angle;
            for (int i = 0; i < RAY_COUNT; i++)
            {
                angle = Quaternion.Euler(0f, -(i * (m_angle / (RAY_COUNT - 1))), 0f) * transform.forward;
                if (TryCheckRay(new Ray(origin, angle.normalized), out interactable))
                {
                    return true;
                }
            }
            for (int i = 1; i < RAY_COUNT; i++)
            {
                angle = Quaternion.Euler(0f, i * (m_angle / (RAY_COUNT - 1)), 0f) * transform.forward;
                if (TryCheckRay(new Ray(origin, angle.normalized), out interactable))
                {
                    return true;
                }
            }
            interactable = null;
            return false;
        }

        /// <summary>
        /// Project a single raycast. If it hits an interactable, return it.
        /// </summary>
        private bool TryCheckRay(Ray ray, out Interactable interactable)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, m_distance, m_targetLayer.value);
            float closestDistance = Mathf.Infinity;
            interactable = null;
            foreach(RaycastHit hit in hits)
            {
                if (hit.collider.TryGetComponent(out Interactable testInteractable))
                {
                    float distance = Vector3.Distance(ray.origin, hit.collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        interactable = testInteractable;
                    }
                }
            }
            return interactable != null;
        }
        #endregion

        #region Do interactions
        /// <summary>
        /// If an interactable is in proximity, check if it responds to the provided <c>interactionType</c> string. If it does, then initiate an interaction.
        /// </summary>
        /// <param name="interactionType">The type of interaction we're intending to perform.</param>
        /// <param name="info">A block of data required for interacting.</param>
        /// <returns>Whether or not the desired interaction could be performed.</returns>
        public bool TryDoInteraction(string interactionType, InteractionInfo info)
        {
            if (!TryGetInteractable(out Interactable interactable))
            {
                return false;
            }
            return interactable.TryInteraction(interactionType, info);
        }

        /// <summary>
        /// Some interactions may require feedback from the interactor while they're running. For example, if you're pulling a lever, the moment your pull animation gets to the end, you may want to signal an event. At that point, the interaction will then slide open a door.
        /// </summary>
        public void DoEvent(AnimationEvent _)
        {
            if (!TryGetInteractable(out Interactable interactable))
            {
                return;
            }
            interactable.DoEvent();
        }
        #endregion

        /// <summary>
        /// Get the current interactable, if one is in proximity.
        /// </summary>
        public bool TryGetInteractable(out Interactable interactable)
        {
            interactable = m_currentInteractable != null ? m_currentInteractable : m_lockedInteractable;
            return interactable != null;
        }

        /// <summary>
        /// A locked interactor will not gain or lose a current interactable.
        /// </summary>
        public bool IsLocked
        {
            get => m_lockedInteractable != null;
            set => m_lockedInteractable = value ? m_currentInteractable : null;
        }

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            foreach(float y in m_origins)
            {
                Vector3 origin = transform.position + y * Vector3.up;
                Vector3 angle;
                for (int i = 0; i < RAY_COUNT; i++)
                {
                    angle = Quaternion.Euler(0f, -(i * (m_angle / (RAY_COUNT - 1))), 0f) * transform.forward;
                    Gizmos.DrawLine(origin, origin + m_distance * angle.normalized);
                }
                for (int i = 1; i < RAY_COUNT; i++)
                {
                    angle = Quaternion.Euler(0f, i * (m_angle / (RAY_COUNT - 1)), 0f) * transform.forward;
                    Gizmos.DrawLine(origin, origin + m_distance * angle.normalized);
                }
            }
        }
        #endregion
    }
}