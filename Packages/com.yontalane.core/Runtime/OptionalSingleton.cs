using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A base class for creating optional singleton components.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class OptionalSingleton<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// Gets the singleton instance of the component.
        /// </summary>
        public static T Instance { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a singleton.
        /// </summary>
        protected abstract bool IsSingleton { get; }

        protected virtual void Awake()
        {
            if (IsSingleton)
            {
                Instance = this as T;
            }
        }
    }
}
