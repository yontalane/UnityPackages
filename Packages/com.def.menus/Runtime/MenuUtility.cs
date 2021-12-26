using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DEF.Menus
{
    public static class MenuUtility
    {
        private static EventSystem m_eventSystem = null;

        public static EventSystem EventSystem
        {
            get
            {
                if (m_eventSystem == null)
                {
                    m_eventSystem = Object.FindObjectOfType<EventSystem>();
                }
                return m_eventSystem;
            }
        }

        public static void Highlight(this Selectable selectable)
        {
            selectable.Select();
            selectable.OnSelect(null);

            if (EventSystem != null)
            {
                EventSystem.SetSelectedGameObject(selectable.gameObject);
            }
        }

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

        public static void RemoveNavigation(this Selectable s)
        {
            Navigation n = s.navigation;
            n.mode = Navigation.Mode.None;
            s.navigation = n;
        }
    }
}