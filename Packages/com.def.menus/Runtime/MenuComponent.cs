using UnityEngine;

namespace DEF.Menus
{
    [DisallowMultipleComponent]
    public abstract class MenuComponent : MonoBehaviour
    {
        private MenuInput m_menuInput = null;

        /// <summary>
        /// The MenuInput that affects this MenuComponent.
        /// </summary>
        public MenuInput MenuInput
        {
            get
            {
                if (m_menuInput == null)
                {
                    m_menuInput = transform.GetComponentInSelfOrParent<MenuInput>();
                }
                return m_menuInput;
            }
        }

        private MenuManager m_menuManager = null;

        /// <summary>
        /// The MenuManager that affects this MenuComponent.
        /// </summary>
        public MenuManager MenuManager
        {
            get
            {
                if (m_menuManager == null)
                {
                    m_menuManager = transform.GetComponentInSelfOrParent<MenuManager>();
                }
                return m_menuManager;
            }
        }
    }
}