using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    /// <summary>
    /// Abstract base class for managing UI menus in a Unity application.
    /// Handles menu navigation, input actions, and menu state management.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class MenuManager : MonoBehaviour
    {
        #region Structs
        [System.Serializable]
        /// <summary>
        /// Represents the input configuration for menu controls, including input actions and button mappings.
        /// </summary>
        public struct ControlInput
        {
#if UNITY_EDITOR
#pragma warning disable IDE0079
            /// <summary>
            /// Used in the Unity Editor to track whether the struct is expanded in the inspector.
            /// </summary>
            public bool editorExpanded;
#pragma warning restore IDE0079
#endif
            /// <summary>
            /// The InputActionAsset containing all input actions for menu navigation.
            /// </summary>
            public InputActionAsset actions;

            /// <summary>
            /// The action name or path for navigating to the previous tab.
            /// </summary>
            public string tabLeft;

            /// <summary>
            /// The action name or path for navigating to the next tab.
            /// </summary>
            public string tabRight;

            /// <summary>
            /// An array of action names or paths for menu button actions (e.g., submit, cancel).
            /// </summary>
            public string[] buttons;
        }
        #endregion

        #region Constants
        protected const string GLOBAL_MENU = "GLOBAL";
        protected const string CANCEL_EVENT = "CANCEL";
        #endregion

        #region Private Variables
        private UIDocument m_document;
        private MenuManager m_dominant = null;
        private bool m_sourceIsGlobal = false;
        private bool m_sourceIsSubordinate = false;
        #endregion

        #region Serialized Fields
        [Tooltip("The global menu instance that manages global menu items and actions.")]
        [SerializeField]
        private GlobalMenu m_globalMenu;

        [Tooltip("The collection of menus managed by this MenuManager, including subordinates and menu definitions.")]
        [SerializeField]
        private MenuCollection m_menus;

        [Tooltip("The input configuration for menu navigation and button actions.")]
        [SerializeField]
        private ControlInput m_input;
        #endregion

        #region Accessors
        /// <summary>
        /// Gets the UIDocument associated with this MenuManager.
        /// </summary>
        public UIDocument Document => m_document;

        /// <summary>
        /// Gets the root VisualElement of the associated UIDocument.
        /// </summary>
        public VisualElement Root => Document.rootVisualElement;

        /// <summary>
        /// Gets the InputActionAsset used for menu navigation and actions.
        /// </summary>
        public InputActionAsset InputActions => m_input.actions;
        #endregion

        private void Reset()
        {
            m_globalMenu = default;
            m_menus = default;
            m_input = new()
            {
                actions = null,
                tabLeft = "UI/TabLeft",
                tabRight = "UI/TabRight",
                buttons = new string[0]
            };
        }

        protected virtual void Awake()
        {
            // Find and assign the UIDocument component in the scene. Log an error if not found.
            m_document = FindAnyObjectByType<UIDocument>();
            if (m_document == null)
            {
                Logger.LogError($"{GetType().Name} could not find the UIDocument.");
            }

            // Validate subordinate menu managers and their relationships.
            for (int i = 0; i < m_menus.subordinates.Length; i++)
            {
                // Check for null or self-reference in subordinates.
                if (m_menus.subordinates[i] == null || m_menus.subordinates[i] == this)
                {
                    Logger.LogError($"{GetType().Name} is not properly constructed.");
                    return;
                }

                // Ensure no circular subordinate relationships.
                foreach (MenuManager innerSubordinate in m_menus.subordinates[i].m_menus.subordinates)
                {
                    if (innerSubordinate == this)
                    {
                        Logger.LogError($"{GetType().Name}s cannot be subordinates of each other.");
                        return;
                    }
                }

                // Validate menu items and assign dominant reference.
                foreach (Menu menu in m_menus.menus)
                {
                    foreach (MenuItem menuItem in menu.items)
                    {
                        // Check for valid subordinate menu item targets.
                        if (menuItem.type == MenuItemType.Subordinate && (menuItem.targetSubordinate < 0 || menuItem.targetSubordinate >= m_menus.subordinates.Length))
                        {
                            Logger.LogError($"{GetType().Name} is not properly constructed.");
                            return;
                        }
                    }
                    // Assign this MenuManager as the dominant for the subordinate.
                    m_menus.subordinates[i].m_dominant = this;
                }
            }

            // Register click handlers for the global menu and all managed menus.
            RegisterClick(m_globalMenu.menu);
            foreach (Menu menu in m_menus.menus)
            {
                RegisterClick(menu);
            }
        }

        protected virtual void Start()
        {
            for(int i = 0; i < m_menus.subordinates.Length; i++)
            {
                m_menus.subordinates[i].HideAllMenus();
            }
            SetMenu(m_menus.firstMenu);
        }

        protected virtual void OnEnable()
        {
            if (m_input.actions == null)
            {
                return;
            }

            m_input.actions.Enable();

            if (!string.IsNullOrEmpty(m_input.tabLeft))
            {
                m_input.actions[m_input.tabLeft].Enable();
                m_input.actions[m_input.tabLeft].performed += OnTabLeft;
            }

            if (!string.IsNullOrEmpty(m_input.tabRight))
            {
                m_input.actions[m_input.tabRight].Enable();
                m_input.actions[m_input.tabRight].performed += OnTabRight;
            }

            foreach (string button in m_input.buttons)
            {
                if (!string.IsNullOrEmpty(button))
                {
                    m_input.actions[button].Enable();
                    m_input.actions[button].performed += OnButton;
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (m_input.actions == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(m_input.tabLeft))
            {
                m_input.actions[m_input.tabLeft].performed -= OnTabLeft;
            }

            if (!string.IsNullOrEmpty(m_input.tabRight))
            {
                m_input.actions[m_input.tabRight].performed -= OnTabRight;
            }

            foreach (string button in m_input.buttons)
            {
                if (!string.IsNullOrEmpty(button))
                {
                    m_input.actions[button].performed -= OnButton;
                }
            }
        }

        #region Clicks
        private void RegisterClick(Menu menu)
        {
            if (string.IsNullOrEmpty(menu.name))
            {
                return;
            }

            VisualElement root = Root.Q<VisualElement>(menu.name);
            if (root == null)
            {
                Logger.LogWarning($"Could not find menu \"{menu.name}.\"");
                return;
            }

            List<VisualElement> elements = root.Query<VisualElement>().ToList();
            foreach (VisualElement element in elements)
            {
                if (element is Button || element is Toggle)
                {
                    RegisterClick(menu, element);
                }
            }

            List<BindableElement> bindables = root.Query<BindableElement>().ToList();
            foreach (BindableElement item in bindables)
            {
                if (item is Button)
                {
                    continue;
                }
                item.RegisterCallback((NavigationCancelEvent e) =>
                {
                    OnCancelInternal(menu, item.name, out bool blockEvent);
                    if (blockEvent)
                    {
                        e.StopPropagation();
                        item.focusController.IgnoreEvent(e);
                    }
                });
                item.RegisterCallback((NavigationMoveEvent e) =>
                {
                    if (e.direction == NavigationMoveEvent.Direction.Left || e.direction == NavigationMoveEvent.Direction.Right)
                    {
                        OnSideNavigationInternal(menu, item.name, e.direction == NavigationMoveEvent.Direction.Right, out bool blockEvent);
                        if (blockEvent)
                        {
                            e.StopPropagation();
                            item.focusController.IgnoreEvent(e);
                        }
                    }
                });
            }
        }

        private void RegisterClick(Menu menu, VisualElement buttonOrToggle)
        {
            if (buttonOrToggle is Button button)
            {
                button.clicked += () => OnClickInternal(menu, buttonOrToggle.name);
            }
            else if (buttonOrToggle is Toggle toggle)
            {
                toggle.RegisterValueChangedCallback((e) =>
                {
                    if (e.newValue)
                    {
                        OnClickInternal(menu, buttonOrToggle.name);
                    }
                });
            }
            buttonOrToggle.RegisterCallback((NavigationCancelEvent e) =>
            {
                OnCancelInternal(menu, buttonOrToggle.name, out bool blockEvent);
                if (blockEvent)
                {
                    e.StopPropagation();
                    buttonOrToggle.focusController.IgnoreEvent(e);
                }
            });
            buttonOrToggle.RegisterCallback((NavigationMoveEvent e) =>
            {
                if (e.direction == NavigationMoveEvent.Direction.Left || e.direction == NavigationMoveEvent.Direction.Right)
                {
                    OnSideNavigationInternal(menu, buttonOrToggle.name, e.direction == NavigationMoveEvent.Direction.Right, out bool blockEvent);
                    if (blockEvent)
                    {
                        e.StopPropagation();
                        buttonOrToggle.focusController.IgnoreEvent(e);
                    }
                }
            });

            if (menu.name == m_globalMenu.menu.name)
            {
                buttonOrToggle.focusable = false;
            }
        }

        private void OnClickInternal(Menu menu, string item)
        {
            foreach (MenuItem keyValue in menu.items)
            {
                if (keyValue.name == item)
                {
                    switch(keyValue.type)
                    {
                        case MenuItemType.Normal:
                            SetMenu(keyValue.targetMenu);
                            break;
                        case MenuItemType.Subordinate:
                            HideAllMenus();
                            m_menus.subordinates[keyValue.targetSubordinate].SetMenu(keyValue.targetMenu);
                            break;
                        case MenuItemType.Dominant:
                            HideAllMenus();
                            m_dominant.m_sourceIsSubordinate = true;
                            m_dominant.SetMenu(keyValue.targetMenu);
                            break;
                    }
                    return;
                }
            }

            OnClick(menu.name, item);
        }

        /// <summary>
        /// Called when a menu item is clicked.
        /// Implement this method in a derived class to define custom behavior when a user selects a menu item.
        /// </summary>
        /// <param name="menu">The name of the menu containing the clicked item.</param>
        /// <param name="item">The name of the item that was clicked.</param>
        protected abstract void OnClick(string menu, string item);

        private void OnCancelInternal(Menu menu, string item, out bool blockEvent)
        {
            if (menu.hasCancelTarget)
            {
                switch (menu.cancelTarget.type)
                {
                    case MenuItemType.Normal:
                        SetMenu(menu.cancelTarget.targetMenu);
                        break;
                    case MenuItemType.Subordinate:
                        HideAllMenus();
                        m_menus.subordinates[menu.cancelTarget.targetSubordinate].SetMenu(menu.cancelTarget.targetMenu);
                        break;
                    case MenuItemType.Dominant:
                        HideAllMenus();
                        m_dominant.m_sourceIsSubordinate = true;
                        m_dominant.SetMenu(menu.cancelTarget.targetMenu);
                        break;
                }
                blockEvent = true;
            }
            else
            {
                OnCancel(menu.name, item, out blockEvent);
            }
        }

        /// <summary>
        /// Called when a cancel event occurs within a menu.
        /// Override this method to implement custom behavior when the user cancels or backs out of a menu or menu item.
        /// </summary>
        /// <param name="menu">The name of the menu where the cancel event occurred.</param>
        /// <param name="item">The name of the item within the menu that triggered the cancel event.</param>
        /// <param name="blockEvent">Set to true to block further processing of the cancel event; otherwise, false.</param>
        protected virtual void OnCancel(string menu, string item, out bool blockEvent)
        {
            blockEvent = false;
        }

        private void OnSideNavigationInternal(Menu menu, string item, bool isRight, out bool blockEvent)
        {
            OnSideNavigation(menu.name, item, isRight, out blockEvent);
            blockEvent = blockEvent || menu.blockSideNavigation;
        }

        /// <summary>
        /// Called when a side navigation (e.g., left/right tab or arrow) event occurs within a menu.
        /// Override this method to handle custom side navigation behavior for specific menus or items.
        /// </summary>
        /// <param name="menu">The name of the menu where the navigation event occurred.</param>
        /// <param name="item">The name of the item within the menu that is currently selected or focused.</param>
        /// <param name="isRight">True if the navigation is to the right; false if to the left.</param>
        /// <param name="blockEvent">Set to true to block further processing of the navigation event; otherwise, false.</param>
        protected virtual void OnSideNavigation(string menu, string item, bool isRight, out bool blockEvent)
        {
            blockEvent = false;
        }
        #endregion

        #region Display Menus
        /// <summary>
        /// Retrieves the currently active menu, its root VisualElement, and the currently focused VisualElement within that menu.
        /// Also records the name of the focused element in the menu's focusItem property.
        /// </summary>
        /// <param name="menu">Outputs the active Menu object, or default if none is active.</param>
        /// <param name="root">Outputs the root VisualElement of the active menu, or null if none is active.</param>
        /// <param name="element">Outputs the currently focused VisualElement within the active menu, or null if none is focused.</param>
        private void GetActiveMenuAndRecordCurrentFocus(out Menu menu, out VisualElement root, out VisualElement element)
        {
            int activeMenuIndex = IndexOfActiveMenu();

            if (activeMenuIndex < 0)
            {
                menu = default;
                root = null;
                element = null;
                return;
            }

            menu = m_menus.menus[activeMenuIndex];
            root = Root.Q<VisualElement>(m_menus.menus[activeMenuIndex].name);

            m_menus.menus[activeMenuIndex].focusItem = string.Empty;

            Focusable focusedElement = root.focusController.focusedElement;

            if (focusedElement == null || focusedElement is not VisualElement focusedVisualElement)
            {
                element = null;
                return;
            }

            element = focusedVisualElement;

            m_menus.menus[activeMenuIndex].focusItem = element.name;
        }

        /// <summary>
        /// Sets the currently active menu by its index in the menus array.
        /// If the index is valid, activates the corresponding menu; otherwise, hides all menus.
        /// </summary>
        /// <param name="menu">The index of the menu to activate.</param>
        private void SetMenu(int menu)
        {
            if (menu >= 0 && menu < m_menus.menus.Length)
            {
                SetMenu(m_menus.menus[menu].name);
            }
            else
            {
                HideAllMenus();
            }
        }

        /// <summary>
        /// Sets the currently active menu by name, manages menu visibility, focus, and global menu state.
        /// </summary>
        /// <param name="menu">The name of the menu to activate.</param>
        protected void SetMenu(string menu)
        {
            bool sourceIsGlobal = m_sourceIsGlobal;
            m_sourceIsGlobal = false;

            bool sourceIsSubordinate = m_sourceIsSubordinate;
            m_sourceIsSubordinate = false;

            if (string.IsNullOrEmpty(menu))
            {
                HideAllMenus(true);
                return;
            }

            GetActiveMenuAndRecordCurrentFocus(out Menu sourceMenu, out _, out _);
            bool globalMenuIsVisible = true;

            if (menu == m_globalMenu.menu.name || menu == GLOBAL_MENU)
            {
                foreach (Menu menuObject in m_menus.menus)
                {
                    Root.Q<VisualElement>(menuObject.name).style.display = DisplayStyle.None;
                }
                return;
            }

            for (int i = 0; i < m_menus.menus.Length; i++)
            {
                if (m_menus.menus[i].name == menu)
                {
                    if (!sourceIsSubordinate && sourceIsGlobal && m_globalMenu.resetFocus)
                    {
                        m_menus.menus[i].focusItem = string.Empty;
                    }
                    else if (!sourceIsSubordinate && !sourceIsGlobal && (!sourceMenu.hasCancelTarget || menu != sourceMenu.cancelTarget.targetMenu))
                    {
                        m_menus.menus[i].focusItem = string.Empty;
                    }
                    VisualElement root = Root.Q<VisualElement>(m_menus.menus[i].name);
                    root.style.display = DisplayStyle.Flex;
                    globalMenuIsVisible = m_menus.menus[i].hasGlobalMenu;
                    OnDisplayMenu(m_menus.menus[i]);
                    StartCoroutine(DelayedFocusElement(m_menus.menus[i], root));
                }
                else
                {
                    Root.Q<VisualElement>(m_menus.menus[i].name).style.display = DisplayStyle.None;
                }
            }

            if (string.IsNullOrEmpty(m_globalMenu.menu.name))
            {
                return;
            }

            if (globalMenuIsVisible)
            {
                UpdateGlobalMenuHighlight(menu, MenuItemType.Normal, -1);
            }
            else
            {
                Root.Q<VisualElement>(m_globalMenu.menu.name).style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Called when a menu is displayed. Override this method in a derived class to perform custom actions
        /// (such as updating UI elements, playing sounds, or logging) whenever a menu becomes visible.
        /// </summary>
        /// <param name="menu">The menu that is being displayed.</param>
        protected virtual void OnDisplayMenu(Menu menu)
        {
        }

        private void UpdateGlobalMenuHighlight(string menuName, MenuItemType type, int _)
        {
            Root.Q<VisualElement>(m_globalMenu.menu.name).style.display = DisplayStyle.Flex;

            if (!TryGetMenu(m_globalMenu.menu.name, out VisualElement globalMenu))
            {
                return;
            }

            ToggleButton toggle;

            for (int i = 0; i < m_globalMenu.menu.items.Length; i++)
            {
                toggle = globalMenu.Q<ToggleButton>(m_globalMenu.menu.items[i].name);
                if (toggle == null)
                {
                    continue;
                }
                toggle.Value = m_globalMenu.menu.items[i].targetMenu == menuName && m_globalMenu.menu.items[i].type == type;
            }
        }

        private IEnumerator DelayedFocusElement(Menu menu, VisualElement root)
        {
            if (root == null)
            {
                yield break;
            }

            yield return new WaitForEndOfFrame();

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(m_document.gameObject);
            }

            if (!string.IsNullOrEmpty(menu.focusItem))
            {
                VisualElement focusItem = root.Q<VisualElement>(menu.focusItem);
                if (focusItem != null)
                {
                    focusItem.Focus();
                    yield break;
                }
            }

            List<VisualElement> children = root.Query<VisualElement>().ToList();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].focusable && children[i].canGrabFocus)
                {
                    children[i].Focus();
                    yield break;
                }
            }
        }

        /// <summary>
        /// Hides all menus managed by this MenuManager by setting their display style to None.
        /// Optionally hides the global menu as well.
        /// </summary>
        /// <param name="includingGlobalMenu">If true, also hides the global menu; otherwise, only hides regular menus.</param>
        protected void HideAllMenus(bool includingGlobalMenu = true)
        {
            GetActiveMenuAndRecordCurrentFocus(out _, out _, out _);

            if (includingGlobalMenu && !string.IsNullOrEmpty(m_globalMenu.menu.name))
            {
                Root.Q<VisualElement>(m_globalMenu.menu.name).style.display = DisplayStyle.None;
            }

            foreach (Menu menuObject in m_menus.menus)
            {
                Root.Q<VisualElement>(menuObject.name).style.display = DisplayStyle.None;
            }
        }
        #endregion

        #region Get Menu
        /// <summary>
        /// Returns the array of menus managed by this MenuManager.
        /// </summary>
        public Menu[] GetMenus()
        {
            return m_menus.menus;
        }

        /// <summary>
        /// Attempts to retrieve a <see cref="Menu"/> by its name, along with its root <see cref="VisualElement"/> and container <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="menuName">The name of the menu to retrieve. If null or empty, the first menu in the collection is used.</param>
        /// <param name="menu">When this method returns, contains the <see cref="Menu"/> if found; otherwise, the default value.</param>
        /// <param name="root">When this method returns, contains the root <see cref="VisualElement"/> of the menu if found; otherwise, null.</param>
        /// <param name="container">When this method returns, contains the container <see cref="VisualElement"/> for addable items if specified; otherwise, the root element or null.</param>
        /// <returns>True if the menu is found; otherwise, false.</returns>
        protected bool TryGetMenu(string menuName, out Menu menu, out VisualElement root, out VisualElement container)
        {
            menu = default;
            root = null;
            container = null;

            if (string.IsNullOrEmpty(menuName) && m_menus.menus.Length > 0)
            {
                menuName = m_menus.menus[0].name;
            }

            if (menuName == m_globalMenu.menu.name || menuName == GLOBAL_MENU)
            {
                menu = m_globalMenu.menu;
                root = Root.Q<VisualElement>(m_globalMenu.menu.name);
                container = null;

                return true;
            }

            for (int i = 0; i < m_menus.menus.Length; i++)
            {
                if (m_menus.menus[i].name == menuName)
                {
                    menu = m_menus.menus[i];
                    root = Root.Q<VisualElement>(m_menus.menus[i].name);

                    container = !string.IsNullOrEmpty(menu.addableContainer) ? root.Q<VisualElement>(menu.addableContainer) : root;
                    container ??= root;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve a <see cref="Menu"/> by name.
        /// </summary>
        /// <param name="name">The name of the menu to retrieve.</param>
        /// <param name="menu">When this method returns, contains the <see cref="Menu"/> if found; otherwise, the default value.</param>
        /// <returns>True if the menu is found; otherwise, false.</returns>
        public bool TryGetMenu(string name, out Menu menu)
        {
            return TryGetMenu(name, out menu, out _, out _);
        }

        /// <summary>
        /// Attempts to retrieve the root <see cref="VisualElement"/> of a menu by name.
        /// </summary>
        /// <param name="name">The name of the menu to retrieve.</param>
        /// <param name="menu">When this method returns, contains the root <see cref="VisualElement"/> of the menu if found; otherwise, null.</param>
        /// <returns>True if the menu is found; otherwise, false.</returns>
        public bool TryGetMenu(string name, out VisualElement menu)
        {
            return TryGetMenu(name, out _, out menu, out _);
        }

        /// <summary>
        /// Attempts to retrieve the container <see cref="VisualElement"/> for addable elements in a menu by name.
        /// </summary>
        /// <param name="name">The name of the menu to retrieve the container from.</param>
        /// <param name="container">When this method returns, contains the container <see cref="VisualElement"/> if found; otherwise, null.</param>
        /// <returns>True if the container is found; otherwise, false.</returns>
        public bool TryGetContainer(string name, out VisualElement container)
        {
            return TryGetMenu(name, out _, out _, out container);
        }

        /// <summary>
        /// Attempts to retrieve the currently active menu and its root VisualElement.
        /// </summary>
        /// <param name="menu">When this method returns, contains the active <see cref="Menu"/>, if found; otherwise, the default value.</param>
        /// <param name="root">When this method returns, contains the root <see cref="VisualElement"/> of the active menu, if found; otherwise, null.</param>
        /// <returns>True if an active menu is found; otherwise, false.</returns>
        public bool TryGetActiveMenu(out Menu menu, out VisualElement root)
        {
            int index = IndexOfActiveMenu();
            if (index >= 0)
            {
                menu = m_menus.menus[index];
                root = Root.Q<VisualElement>(menu.name);
                return true;
            }
            else
            {
                menu = default;
                root = null;
                return false;
            }
        }

        /// <summary>
        /// Returns the index of the currently active menu in the menus array.
        /// </summary>
        public int IndexOfActiveMenu()
        {
            VisualElement root;
            for (int i = 0; i < m_menus.menus.Length; i++)
            {
                root = Root.Q<VisualElement>(m_menus.menus[i].name);
                if (root.style.display == DisplayStyle.None)
                {
                    continue;
                }
                return i;
            }

            return -1;
        }
        #endregion

        #region Adjust Elements
        /// <summary>
        /// Sets the enabled state of a UI element with the specified name in the specified menu.
        /// </summary>
        /// <param name="menu">The name of the menu containing the UI element.</param>
        /// <param name="item">The name of the UI element to enable or disable.</param>
        /// <param name="value">True to enable the element, false to disable it.</param>
        public void SetEnabled(string menu, string item, bool value)
        {
            if (!TryGetMenu(menu, out VisualElement root))
            {
                return;
            }

            VisualElement element = root.Q<VisualElement>(item);

            if (element == null)
            {
                return;
            }

            element.SetEnabled(value);
        }

        /// <summary>
        /// Sets the enabled state of a UI element with the specified name in the currently active menu.
        /// </summary>
        /// <param name="item">The name of the UI element to enable or disable.</param>
        /// <param name="value">True to enable the element, false to disable it.</param>
        public void SetEnabled(string item, bool value)
        {
            VisualElement element = Root.Q<VisualElement>(item);

            if (element == null)
            {
                return;
            }

            element.SetEnabled(value);
        }

        /// <summary>
        /// Adds a CSS class to a UI element with the specified name within a given menu.
        /// </summary>
        /// <param name="menu">The name of the menu containing the UI element.</param>
        /// <param name="item">The name of the UI element to update.</param>
        /// <param name="className">The CSS class name to add to the element.</param>
        public void AddClass(string menu, string item, string className)
        {
            if (!TryGetMenu(menu, out VisualElement root))
            {
                return;
            }

            VisualElement element = root.Q<VisualElement>(item);

            if (element == null)
            {
                return;
            }

            element.AddToClassList(className);
        }

        /// <summary>
        /// Removes a CSS class from a UI element with the specified name within a given menu.
        /// </summary>
        /// <param name="menu">The name of the menu containing the UI element.</param>
        /// <param name="item">The name of the UI element to update.</param>
        /// <param name="className">The CSS class name to remove from the element.</param>
        public void RemoveClass(string menu, string item, string className)
        {
            if (!TryGetMenu(menu, out VisualElement root))
            {
                return;
            }

            VisualElement element = root.Q<VisualElement>(item);

            if (element == null)
            {
                return;
            }

            element.RemoveFromClassList(className);
        }

        /// <summary>
        /// Sets the text of a UI element with the specified name within a given menu.
        /// </summary>
        /// <param name="menu">The name of the menu containing the UI element.</param>
        /// <param name="item">The name of the UI element to update.</param>
        /// <param name="value">The new text value to set.</param>
        public void SetText(string menu, string item, string value)
        {
            if (!TryGetMenu(menu, out VisualElement root))
            {
                return;
            }

            TextElement element = root.Q<TextElement>(item);

            if (element == null)
            {
                return;
            }

            element.text = value;
        }

        /// <summary>
        /// Sets the text of a UI element with the specified name in the root visual element.
        /// </summary>
        /// <param name="item">The name of the UI element to update.</param>
        /// <param name="value">The new text value to set.</param>
        public void SetText(string item, string value)
        {
            TextElement element = Root.Q<TextElement>(item);

            if (element == null)
            {
                return;
            }

            element.text = value;
        }

        /// <summary>
        /// Sets keyboard focus to the specified UI element within the currently active menu, if possible.
        /// </summary>
        /// <param name="item">The name of the UI element to focus.</param>
        public void SetFocus(string item)
        {
            if (!TryGetActiveMenu(out _, out VisualElement root))
            {
                return;
            }

            VisualElement element = root.Q<VisualElement>(item);

            if (element == null || !element.focusable || !element.canGrabFocus)
            {
                return;
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(m_document.gameObject);
            }

            element.Focus();
        }
        #endregion

        #region Manage Addables
        /// <summary>
        /// Removes all child elements from the specified menu's container, effectively clearing its contents.
        /// </summary>
        /// <param name="menu">The name of the menu to clear.</param>
        public void Clear(string menu)
        {
            if (!TryGetContainer(menu, out VisualElement container))
            {
                return;
            }

            if (container is ScrollViewAuto containerAsScrollViewAuto)
            {
                containerAsScrollViewAuto.Clear();
            }
            else
            {
                container.Clear();
            }
        }

        /// <summary>
        /// Adds an addable UI item to the specified menu using the provided data.
        /// This method appends the item to the end of the menu's container.
        /// </summary>
        /// <param name="menu">The name of the menu to add the item to.</param>
        /// <param name="data">The data describing the addable item, including template, name, label, and text.</param>
        public void Add(string menu, AddableItemData data)
        {
            Insert(menu, -1, data);
        }

        /// <summary>
        /// Inserts an addable UI item into the specified menu at the given index using the provided data.
        /// Handles template instantiation, element naming, and text assignment for the new UI element.
        /// </summary>
        /// <param name="menu">The name of the menu to insert the item into.</param>
        /// <param name="index">The index at which to insert the item. Use -1 to append to the end.</param>
        /// <param name="data">The data describing the addable item, including template, name, label, and text.</param>
        public void Insert(string menu, int index, AddableItemData data)
        {
            if (!TryGetMenu(menu, out Menu menuObject, out _, out VisualElement container))
            {
                return;
            }
            VisualTreeAsset template = data.template != null ? data.template : menuObject.defaultAddableItemTemplate;
            if (template == null)
            {
                Logger.LogError("Could not find template.");
            }


            if (container is ScrollViewAuto containerAsScrollViewAuto)
            {
                containerAsScrollViewAuto.CloneTreeAsset(template, out int firstElementIndex, out int elementAddedCount);

                List<VisualElement> list = (List<VisualElement>)containerAsScrollViewAuto.Children();

                if (elementAddedCount != 1)
                {
                    Logger.LogError("Only use templates containing exactly one element.");
                    if (elementAddedCount == 0)
                    {
                        return;
                    }
                    else
                    {
                        for (int i = elementAddedCount - 1; i > 0; i--)
                        {
                            containerAsScrollViewAuto.RemoveAt(firstElementIndex + i);
                        }
                    }
                }

                VisualElement element = list[firstElementIndex];
                element.name = data.name;

                if (!string.IsNullOrEmpty(data.label))
                {
                    TextElement textElement = element.Q<TextElement>(data.label);
                    if (textElement != null)
                    {
                        textElement.text = data.text;
                    }
                }
                else if (element is TextElement elementAsTextElement)
                {
                    elementAsTextElement.text = data.text;
                }
                else if (element is Toggle elementAsToggle)
                {
                    elementAsToggle.label = data.text;
                }

                if (element is Button elementAsButton)
                {
                    RegisterClick(menuObject, elementAsButton);
                }
                else if (element is Toggle elementAsToggle)
                {
                    RegisterClick(menuObject, elementAsToggle);
                }

                if (index < 0 || index >= containerAsScrollViewAuto.ChildCount)
                {
                    data.onAdd?.Invoke(element);
                    return;
                }

                containerAsScrollViewAuto.RemoveAt(firstElementIndex);
                containerAsScrollViewAuto.Insert(index, element);
                data.onAdd?.Invoke(element);
            }
            else
            {
                template.CloneTree(container, out int firstElementIndex, out int elementAddedCount);

                List<VisualElement> list = (List<VisualElement>)container.Children();

                if (elementAddedCount != 1)
                {
                    Logger.LogError("Only use templates containing exactly one element.");
                    if (elementAddedCount == 0)
                    {
                        return;
                    }
                    else
                    {
                        for (int i = elementAddedCount - 1; i > 0; i--)
                        {
                            container.RemoveAt(firstElementIndex + i);
                        }
                    }
                }

                VisualElement element = list[firstElementIndex];
                element.name = data.name;

                if (!string.IsNullOrEmpty(data.label))
                {
                    TextElement textElement = element.Q<TextElement>(data.label);
                    if (textElement != null)
                    {
                        textElement.text = data.text;
                    }
                }
                else if (element is TextElement elementAsTextElement)
                {
                    elementAsTextElement.text = data.text;
                }

                if (element is Button elementAsButton)
                {
                    RegisterClick(menuObject, elementAsButton);
                }
                else if (element is Toggle elementAsToggle)
                {
                    RegisterClick(menuObject, elementAsToggle);
                }

                if (index < 0 || index >= container.childCount)
                {
                    data.onAdd?.Invoke(element);
                    return;
                }

                container.RemoveAt(firstElementIndex);
                container.Insert(index, element);
                data.onAdd?.Invoke(element);
            }
        }

        /// <summary>
        /// Adds a range of UI elements to the specified menu using the provided list of <see cref="AddableItemData"/>.
        /// </summary>
        /// <param name="menu">The name of the menu to which the items will be added.</param>
        /// <param name="datas">A read-only list of <see cref="AddableItemData"/> representing the items to add.</param>
        public void AddRange(string menu, IReadOnlyList<AddableItemData> datas)
        {
            foreach (AddableItemData data in datas)
            {
                Add(menu, data);
            }
        }

        /// <summary>
        /// Removes a UI element with the specified name from the given menu's container.
        /// </summary>
        /// <param name="menu">The name of the menu containing the item.</param>
        /// <param name="item">The name of the item to remove.</param>
        public void Remove(string menu, string item)
        {
            if (!TryGetContainer(menu, out VisualElement container))
            {
                return;
            }

            VisualElement child = container.Q<VisualElement>(item);
            if (child != null)
            {
                if (container is ScrollViewAuto containerAsScrollViewAuto)
                {
                    containerAsScrollViewAuto.Remove(child);
                }
                else
                {
                    container.Remove(child);
                }
            }
        }
        #endregion

        #region Tabs
        private void OnTabLeft(InputAction.CallbackContext _)
        {
            TabInDirection(-1);
        }

        private void OnTabRight(InputAction.CallbackContext _)
        {
            TabInDirection(1);
        }

        private void TabInDirection(int direction)
        {
            if (m_globalMenu.menu.items.Length < 2)
            {
                return;
            }

            if (!TryGetMenu(GLOBAL_MENU, out VisualElement _))
            {
                return;
            }

            bool foundActiveMenu = TryGetActiveMenu(out Menu menu, out _);
            bool isSubordinate = false;

            if (!foundActiveMenu)
            {
                foreach (MenuManager subordinate in m_menus.subordinates)
                {
                    foundActiveMenu = subordinate.TryGetActiveMenu(out menu, out _);
                    if (foundActiveMenu)
                    {
                        isSubordinate = true;
                        break;
                    }
                }
            }

            if (!foundActiveMenu)
            {
                return;
            }

            if (!isSubordinate && !menu.hasGlobalMenu)
            {
                return;
            }

            int startIndex = -1;

            for (int i = 0; i < m_globalMenu.menu.items.Length; i++)
            {
                if (m_globalMenu.menu.items[i].targetMenu == menu.name)
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex == -1)
            {
                return;
            }

            int index = startIndex + direction;
            if (index < 0)
            {
                index = m_globalMenu.menu.items.Length - 1;
            }
            else if (index > m_globalMenu.menu.items.Length - 1)
            {
                index = 0;
            }

            while ((m_globalMenu.menu.items[index].type == MenuItemType.Normal && !TryGetMenu(m_globalMenu.menu.items[index].targetMenu, out Menu _)) || (m_globalMenu.menu.items[index].type == MenuItemType.Subordinate && !m_menus.subordinates[m_globalMenu.menu.items[index].targetSubordinate].TryGetMenu(m_globalMenu.menu.items[index].targetMenu, out Menu _)) || m_globalMenu.menu.items[index].type == MenuItemType.Dominant)
            {
                index += direction;
                if (index < 0)
                {
                    index = m_globalMenu.menu.items.Length - 1;
                }
                else if (index > m_globalMenu.menu.items.Length - 1)
                {
                    index = 0;
                }
            }

            m_sourceIsGlobal = true;
            if (m_globalMenu.menu.items[index].type == MenuItemType.Normal)
            {
                foreach(MenuManager subordinate in m_menus.subordinates)
                {
                    subordinate.HideAllMenus();
                }
                SetMenu(m_globalMenu.menu.items[index].targetMenu);
            }
            else if (m_globalMenu.menu.items[index].type == MenuItemType.Subordinate)
            {
                HideAllMenus(false);
                m_menus.subordinates[m_globalMenu.menu.items[index].targetSubordinate].SetMenu(m_globalMenu.menu.items[index].targetMenu);
                UpdateGlobalMenuHighlight(m_globalMenu.menu.items[index].targetMenu, m_globalMenu.menu.items[index].type, m_globalMenu.menu.items[index].targetSubordinate);
            }
        }

        private void OnButton(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnButtonInput(context.action);
            }
        }

        protected virtual void OnButtonInput(InputAction action)
        {
            
        }
        #endregion
    }
}
