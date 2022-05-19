using System;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane
{
    [AddComponentMenu("Yontalane/Collision Listener")]
    public class CollisionListener : MonoBehaviour
    {
        public enum Type
        {
            Enter = 0,
            Exit = 1
        }

        #region Classes
        [Serializable]
        public class CollisionHandler : UnityEvent<Type, Collision> { }
        [Serializable]
        public class Collision2DHandler : UnityEvent<Type, Collision2D> { }
        [Serializable]
        public class TriggerHandler : UnityEvent<Type, Collider> { }
        [Serializable]
        public class Trigger2DHandler : UnityEvent<Type, Collider2D> { }
        [Serializable]
        public class ControllerHitHandler : UnityEvent<ControllerColliderHit> { }
        #endregion

        #region Fields
        public LayerMask filter;
        [Header("Collision")]
        public CollisionHandler OnCollision = null;
        public Collision2DHandler OnCollision2D = null;
        [Header("Trigger")]
        public TriggerHandler OnTrigger = null;
        public Trigger2DHandler OnTrigger2D = null;
        [Header("Controller")]
        public ControllerHitHandler OnControllerHit = null;
        #endregion

        private void Reset() => filter = LayerMask.NameToLayer("Everything");

        #region Event Methods
        private void OnCollisionEnter(Collision collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnCollision?.Invoke(Type.Enter, collision);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnCollision2D?.Invoke(Type.Enter, collision);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnCollision?.Invoke(Type.Exit, collision);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnCollision2D?.Invoke(Type.Exit, collision);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((filter.value & (1 << other.gameObject.layer)) > 0)
            {
                OnTrigger?.Invoke(Type.Enter, other);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnTrigger2D?.Invoke(Type.Enter, collision);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ((filter.value & (1 << other.gameObject.layer)) > 0)
            {
                OnTrigger?.Invoke(Type.Exit, other);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if ((filter.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnTrigger2D?.Invoke(Type.Exit, collision);
            }
        }

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
