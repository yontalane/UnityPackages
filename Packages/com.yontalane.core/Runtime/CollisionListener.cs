using UnityEngine;

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

        public delegate void CollisionHandler(Type type, Collision collision);
        public CollisionHandler OnCollision = null;
        public delegate void Collision2DHandler(Type type, Collision2D collision);
        public Collision2DHandler OnCollision2D = null;
        public delegate void TriggerHandler(Type type, Collider other);
        public TriggerHandler OnTrigger = null;
        public delegate void Trigger2DHandler(Type type, Collider2D other);
        public Trigger2DHandler OnTrigger2D = null;

        private void OnCollisionEnter(Collision collision) => OnCollision?.Invoke(Type.Enter, collision);

        private void OnCollisionEnter2D(Collision2D collision) => OnCollision2D?.Invoke(Type.Enter, collision);

        private void OnCollisionExit(Collision collision) => OnCollision?.Invoke(Type.Exit, collision);

        private void OnCollisionExit2D(Collision2D collision) => OnCollision2D?.Invoke(Type.Exit, collision);

        private void OnTriggerEnter(Collider other) => OnTrigger?.Invoke(Type.Enter, other);

        private void OnTriggerEnter2D(Collider2D collision) => OnTrigger2D?.Invoke(Type.Enter, collision);

        private void OnTriggerExit(Collider other) => OnTrigger?.Invoke(Type.Exit, other);

        private void OnTriggerExit2D(Collider2D collision) => OnTrigger2D?.Invoke(Type.Exit, collision);
    }
}
