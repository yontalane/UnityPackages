using UnityEngine;

namespace DEF.Menus
{
    [DisallowMultipleComponent]
    public abstract class MenuComponent : MonoBehaviour
    {
        private MenuInput m_menuInput = null;
        public MenuInput MenuInput
        {
            get
            {
                if (m_menuInput == null)
                {
                    m_menuInput = GetComponentInSelfOrParent<MenuInput>();
                }
                return m_menuInput;
            }
        }

        private MenuManager m_menuManager = null;
        public MenuManager MenuManager
        {
            get
            {
                if (m_menuManager == null)
                {
                    m_menuManager = GetComponentInSelfOrParent<MenuManager>();
                }
                return m_menuManager;
            }
        }

        private T GetComponentInSelfOrParent<T>() where T : Component
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