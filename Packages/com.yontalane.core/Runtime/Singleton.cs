using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A base class for creating singleton components.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// Gets the singleton instance of the component.
        /// </summary>
        public static T Instance { get; private set; }

        protected virtual void Awake() => Instance = this as T;
    }
}
