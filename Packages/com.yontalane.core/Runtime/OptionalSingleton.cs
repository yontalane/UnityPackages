using UnityEngine;

namespace Yontalane
{
    [DisallowMultipleComponent]
    public abstract class OptionalSingleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

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
