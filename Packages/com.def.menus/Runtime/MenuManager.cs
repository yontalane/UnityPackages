using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DEF.Menus
{
    [DisallowMultipleComponent, RequireComponent(typeof(MenuInput))]
    public sealed class MenuManager : MenuComponent
    {
        public delegate void ActivateMenuHandler(Menu menu);
        public static ActivateMenuHandler OnActivateMenu = null;
        public delegate void MenuItemClickHandler(Menu menu, Selectable item);
        public static MenuItemClickHandler OnMenuItemClick = null;
        public delegate void MenuItemSelectHandler(Menu menu, Selectable item);
        public static MenuItemSelectHandler OnMenuItemSelect = null;
        public delegate void MenuButtonHandler(Menu menu, string buttonName);
        public static MenuButtonHandler OnMenuButton = null;

        [System.Serializable]
        public struct Connection
        {
            [Tooltip("Source button.")] public string sourceButton;
            [Tooltip("Source menu.")] public string sourceMenu;
            [Tooltip("Target menu.")] public string targetMenu;
        }

        private const string ANIMATOR_PARAMETER = "Active Menu";

        [SerializeField]
        [Tooltip("If unassigned, MenuManager will populate at runtime with all child Menu components. However, it will miss any children that are inactive in the hierarchy.")]
        private Menu[] m_menus = new Menu[0];

        [Space]

        [Min(0)]
        [SerializeField]
        [Tooltip("The initially active Menu.")]
        private int m_activeMenu = 0;
        private Animator m_animator = null;

        [Space]

        [SerializeField]
        [Tooltip("Manage buttons that navigate from one Menu to the next.")]
        private Connection[] m_connections = new Connection[0];

        private void OnEnable()
        {
            MenuInput.OnInputEvent += OnInputEvent;
            Menu.OnClick += OnClickMenuItem;
            SelectItemAction.OnSelectItem += OnSelectMenuItem;
        }

        private void OnDisable()
        {
            MenuInput.OnInputEvent -= OnInputEvent;
            Menu.OnClick -= OnClickMenuItem;
            SelectItemAction.OnSelectItem -= OnSelectMenuItem;
        }

        private void Awake()
        {
            m_animator = GetComponent<Animator>();

            if (m_menus.Length == 0)
            {
                m_menus = GetComponentsInChildren<Menu>();
            }
            for (int i = 0; i < m_menus.Length; i++)
            {
                m_menus[i].Initialize();
                if (i == m_activeMenu && m_menus[i].activeSelectable >= 0 && m_menus[i].activeSelectable < m_menus[i].selectables.Count && EventSystem.current != null)
                {
                    EventSystem.current.firstSelectedGameObject = m_menus[i].selectables[m_menus[i].activeSelectable].gameObject;
                }
            }

            ActivateMenu(m_activeMenu);
        }

        #region Menu

        /// <summary>
        /// Search for a Menu within this MenuManager by name. If it exists, set it to <c>menu</c> and return true. Otherwise, return false.
        /// </summary>
        public bool TryGetMenu(string name, out Menu menu)
        {
            if (m_menus.Length == 0)
            {
                m_menus = GetComponentsInChildren<Menu>();
            }

            foreach (Menu m in m_menus.Where(m => m.name == name))
            {
                menu = m;
                return true;
            }

            menu = null;
            return false;
        }

        /// <summary>
        /// Get the Menu called <c>name</c> within this MenuManager.
        /// </summary>
        public Menu GetMenu(string name) => TryGetMenu(name, out Menu menu) ? menu : null;

        private void ActivateMenu(int newActiveMenu)
        {
            m_activeMenu = newActiveMenu;

            for (int i = 0; i < m_menus.Length; i++)
            {
                m_menus[i].Activate(i == m_activeMenu);
            }

            if (m_animator != null)
            {
                m_animator.SetInteger(ANIMATOR_PARAMETER, m_activeMenu);
            }

            if (m_menus[m_activeMenu] != null)
            {
                OnActivateMenu?.Invoke(m_menus[m_activeMenu]);
            }
        }

        private void ActivateMenu(string newActiveMenu)
        {
            for (int i = 0; i < m_menus.Length; i++)
            {
                if (m_menus[i].name == newActiveMenu)
                {
                    ActivateMenu(i);
                    return;
                }
            }
        }

        private Menu ActiveMenu
        {
            get
            {
                foreach (Menu m in m_menus)
                {
                    if (m.IsActive)
                    {
                        return m;
                    }
                }
                return null;
            }
        }

        #endregion

        #region Input

        private void OnInputEvent(MenuInputEvent e)
        {
            if (e.move.y < 0)
            {
                m_menus[m_activeMenu].HighlightNext();
            }
            else if (e.move.y > 0)
            {
                m_menus[m_activeMenu].HighlightPrevious();
            }

            if (!string.IsNullOrEmpty(e.buttonName))
            {
                if (m_menus[m_activeMenu] != null && m_menus[m_activeMenu].TryGetItem(e.buttonName, out Selectable selectable))
                {
                    OnMenuItemClick?.Invoke(m_menus[m_activeMenu], selectable);
                }
                foreach (Connection c in m_connections)
                {
                    if (c.sourceMenu == ActiveMenu.name && c.sourceButton == e.buttonName)
                    {
                        ActivateMenu(c.targetMenu);
                        break;
                    }
                }
                OnMenuButton?.Invoke(m_menus[m_activeMenu], e.buttonName);
            }
        }

        private void OnClickMenuItem(MenuActionEvent e)
        {
            if (e.menu != null && e.item != null)
            {
                OnMenuItemClick?.Invoke(e.menu, e.item);
            }
            foreach (Connection c in m_connections)
            {
                if (c.sourceMenu == e.menuName && c.sourceButton == e.itemName)
                {
                    ActivateMenu(c.targetMenu);
                    break;
                }
            }
        }

        private void OnSelectMenuItem(Menu menu, Selectable item) => OnMenuItemSelect?.Invoke(menu, item);

        #endregion
    }
}