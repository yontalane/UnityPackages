using UnityEngine;
using UnityEngine.UI;

namespace DEF.Menus
{
    public static class MenuUtility
    {
        /// <summary>
        /// Is the target transform a descendent of <c>possibleParent</c> within the hierarchy?
        /// </summary>
        public static bool IsDescendantOf(this Transform possibleChild, Transform possibleParent)
        {
            Transform transform = possibleChild;
            while (transform != null)
            {
                if (transform == possibleParent)
                {
                    return true;
                }
                transform = transform.parent;
            }
            return false;
        }

        /// <summary>
        /// Remove the target Selectable from the UI navigation map.
        /// </summary>
        public static void RemoveNavigation(this Selectable s)
        {
            Navigation n = s.navigation;
            n.mode = Navigation.Mode.None;
            s.navigation = n;
        }

        /// <summary>
        /// Get a component of type <c>T</c> in the target object or any of its ancestors within the hierarchy.
        /// </summary>
        public static T GetComponentInSelfOrParent<T>(this Transform transform) where T : Component
        {
            T component = null;
            Transform obj = transform;
            while (component == null && obj != null)
            {
                component = obj.GetComponent<T>();
                obj = obj.parent;
            }
            return component;
        }
    }
}