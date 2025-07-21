using System;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane
{
    /// <summary>
    /// A component that listens for collision events.
    /// </summary>
    [AddComponentMenu("Yontalane/Collision Listener")]
    public class CollisionListener : MonoBehaviour
    {
        /// <summary>
        /// The type of collision event.
        /// </summary>
        public enum Type
        {
            Enter = 0,
            Exit = 1
        }

        #region Classes
        /// <summary>
        /// A UnityEvent that is invoked when a collision occurs.
        /// </summary>
        [Serializable]
        public class CollisionHandler : UnityEvent<Type, Collision> { }

        /// <summary>
        /// A UnityEvent that is invoked when a collision occurs.
        /// </summary>
        public class Collision2DHandler : UnityEvent<Type, Collision2D> { }

        /// <summary>
        /// A UnityEvent that is invoked when a trigger occurs.
        /// </summary>
        [Serializable]
        public class TriggerHandler : UnityEvent<Type, Collider> { }

        /// <summary>
        /// A UnityEvent that is invoked when a trigger occurs.
        /// </summary>
        public class Trigger2DHandler : UnityEvent<Type, Collider2D> { }

        /// <summary>
        /// A UnityEvent that is invoked when a controller collider hit occurs.
        /// </summary>
        [Serializable]
        public class ControllerHitHandler : UnityEvent<ControllerColliderHit> { }
        #endregion

        #region Fields
        [Tooltip("Only invoke events for GameObjects on these layers.")]
        public LayerMask filter;

        [Header("Collision")]

        [Tooltip("Invoked when a 3D collision event occurs (Enter/Exit).")]
        public CollisionHandler OnCollision = null;

        [Tooltip("Invoked when a 2D collision event occurs (Enter/Exit).")]
        public Collision2DHandler OnCollision2D = null;

        [Header("Trigger")]

        [Tooltip("Invoked when a 3D trigger event occurs (Enter/Exit).")]
        public TriggerHandler OnTrigger = null;

        [Tooltip("Invoked when a 2D trigger event occurs (Enter/Exit).")]
        public Trigger2DHandler OnTrigger2D = null;

        [Header("Controller")]

        [Tooltip("Invoked when a CharacterController hits a collider.")]
        public ControllerHitHandler OnControllerHit = null;
        #endregion

        private void Reset() => filter = LayerMask.NameToLayer("Everything");

        #region Event Methods
        /// <summary>
        /// Called when a collision occurs.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnCollision?.Invoke(Type.Enter, collision);
            }
        }

        /// <summary>
        /// Called when a 2D collision occurs.
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnCollision2D?.Invoke(Type.Enter, collision);
            }
        }

        /// <summary>
        /// Called when a collision exits.
        /// </summary>
        private void OnCollisionExit(Collision collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnCollision?.Invoke(Type.Exit, collision);
            }
        }

        /// <summary>
        /// Called when a 2D collision exits.
        /// </summary>
        private void OnCollisionExit2D(Collision2D collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnCollision2D?.Invoke(Type.Exit, collision);
            }
        }

        /// <summary>
        /// Called when a trigger enters.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if ((filter.value & (1 << other.gameObject.layer)) > 0)
            {
                OnTrigger?.Invoke(Type.Enter, other);
            }
        }

        /// <summary>
        /// Called when a 2D trigger enters.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnTrigger2D?.Invoke(Type.Enter, collision);
            }
        }

        /// <summary>
        /// Called when a trigger exits.
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if ((filter.value & (1 << other.gameObject.layer)) > 0)
            {
                OnTrigger?.Invoke(Type.Exit, other);
            }
        }

        /// <summary>
        /// Called when a 2D trigger exits.
        /// </summary>
        private void OnTriggerExit2D(Collider2D collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnTrigger2D?.Invoke(Type.Exit, collision);
            }
        }

        /// <summary>
        /// Called when a CharacterController hits a collider.
        /// </summary>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if ((filter.value & (1 << hit.gameObject.layer)) > 0)
            {
                OnControllerHit?.Invoke(hit);
            }
        }
        #endregion
    }
}
