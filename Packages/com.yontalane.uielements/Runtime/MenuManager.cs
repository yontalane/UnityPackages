using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    [DisallowMultipleComponent]
    public abstract class MenuManager : MonoBehaviour
    {
        #region Structs
        [System.Serializable]
        public struct ControlInput
        {
#if UNITY_EDITOR
#pragma warning disable IDE0079
            public bool editorExpanded;
#pragma warning restore IDE0079
#endif

            public InputActionAsset actions;
            public string tabLeft;
            public string tabRight;
        }
        #endregion

        #region Constants
        protected const string GLOBAL_MENU = "GLOBAL";
        protected const string CANCEL_EVENT = "CANCEL";
        public const string TAB_HIGHLIGHT_CLASS = "highlight";
        #endregion

        #region Private Variables
        private UIDocument m_document;
        private MenuManager m_dominant = null;
        private bool m_sourceIsGlobal = false;
        private bool m_sourceIsSubordinate = false;
        #endregion

        #region Serialized Fields
        [SerializeField]
        private GlobalMenu m_globalMenu;

        [SerializeField]
        private MenuCollection m_menus;

        [SerializeField]
        private ControlInput m_input;
        #endregion

        #region Accessors
        public UIDocument Document => m_document;
        public VisualElement Root => Document.rootVisualElement;
        #endregion

        private void Reset()
        {
            m_globalMenu = default;
            m_menus = default;
            m_input = new()
            {
                actions = null,
                tabLeft = "TabLeft",
                tabRight = "TabRight"
            };
        }

        protected virtual void Awake()
        {
            m_document = FindObjectOfType<UIDocument>();
            if (m_document == null)
            {
                Logger.LogError($"{GetType().Name} could not find the UIDocument.");
            }

            for (int i = 0; i < m_menus.subordinates.Length; i++)
            {
                if (m_menus.subordinates[i] == null || m_menus.subordinates[i] == this)
                {
                    Logger.LogError($"{GetType().Name} is not properly constructed.");
                    return;
                }
                foreach (MenuManager innerSubordinate in m_menus.subordinates[i].m_menus.subordinates)
                {
                    if (innerSubordinate == this)
                    {
                        Logger.LogError($"{GetType().Name}s cannot be subordinates of each other.");
                        return;
                    }
                }
                foreach (Menu menu in m_menus.menus)
                {
                    foreach (MenuItem menuItem in menu.items)
                    {
                        if (menuItem.type == MenuItemType.Subordinate && (menuItem.targetSubordinate < 0 || menuItem.targetSubordinate >= m_menus.subordinates.Length))
                        {
                            Logger.LogError($"{GetType().Name} is not properly constructed.");
                            return;
                        }
                    }
                    m_menus.subordinates[i].m_dominant = this;
                }
            }

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

        private void OnEnable()
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
        }

        private void OnDisable()
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

            List<Button> buttons = root.Query<Button>().ToList();
            foreach (Button item in buttons)
            {
                RegisterClick(menu, item);
            }

            List<BindableElement> bindables = root.Query<BindableElement>().ToList();
            foreach (BindableElement item in bindables)
            {
                if (item is Button)
                {
                    continue;
                }
                item.RegisterCallback((NavigationCancelEvent e) => OnClickInternal(menu, CANCEL_EVENT));
            }
        }

        private void RegisterClick(Menu menu, Button button)
        {
            button.clicked += () => OnClickInternal(menu, button.name);
            button.RegisterCallback((NavigationCancelEvent e) =>
            {
                OnClickInternal(menu, CANCEL_EVENT);
            });

            if (menu.name == m_globalMenu.menu.name)
            {
                button.focusable = false;
            }
        }

        private void OnClickInternal(Menu menu, string item)
        {
            if (item == CANCEL_EVENT && menu.hasCancelTarget)
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
                return;
            }

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

        protected abstract void OnClick(string menu, string item);
        #endregion

        #region Display Menus
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
                Root.Q<VisualElement>(m_globalMenu.menu.name).style.display = DisplayStyle.Flex;

                for (int i = 0; i < m_globalMenu.menu.items.Length; i++)
                {
                    if (m_globalMenu.menu.items[i].targetMenu == menu)
                    {
                        AddClass(m_globalMenu.menu.name, m_globalMenu.menu.items[i].name, TAB_HIGHLIGHT_CLASS);
                    }
                    else
                    {
                        RemoveClass(m_globalMenu.menu.name, m_globalMenu.menu.items[i].name, TAB_HIGHLIGHT_CLASS);
                    }
                }
            }
            else
            {
                Root.Q<VisualElement>(m_globalMenu.menu.name).style.display = DisplayStyle.None;
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
        public Menu[] GetMenus()
        {
            return m_menus.menus;
        }

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

        public bool TryGetMenu(string name, out Menu menu)
        {
            return TryGetMenu(name, out menu, out _, out _);
        }

        public bool TryGetMenu(string name, out VisualElement menu)
        {
            return TryGetMenu(name, out _, out menu, out _);
        }

        public bool TryGetContainer(string name, out VisualElement container)
        {
            return TryGetMenu(name, out _, out _, out container);
        }

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

        public void SetEnabled(string item, bool value)
        {
            VisualElement element = Root.Q<VisualElement>(item);

            if (element == null)
            {
                return;
            }

            element.SetEnabled(value);
        }

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

        public void SetText(string item, string value)
        {
            TextElement element = Root.Q<TextElement>(item);

            if (element == null)
            {
                return;
            }

            element.text = value;
        }

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
        public void Clear(string menu)
        {
            if (!TryGetContainer(menu, out VisualElement container))
            {
                return;
            }

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                container.RemoveAt(i);
            }
        }

        public void Add(string menu, AddableItemData data)
        {
            Insert(menu, -1, data);
        }

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

            if (index < 0 || index >= container.childCount)
            {
                return;
            }

            container.RemoveAt(firstElementIndex);
            container.Insert(index, element);
        }

        public void AddRange(string menu, IReadOnlyList<AddableItemData> datas)
        {
            foreach (AddableItemData data in datas)
            {
                Add(menu, data);
            }
        }

        public void Remove(string menu, string item)
        {
            if (!TryGetContainer(menu, out VisualElement container))
            {
                return;
            }

            VisualElement child = container.Q<VisualElement>(item);
            if (child != null)
            {
                container.Remove(child);
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

            if (!TryGetActiveMenu(out Menu menu, out _))
            {
                return;
            }

            if (!menu.hasGlobalMenu)
            {
                return;
            }

            if (!TryGetMenu(GLOBAL_MENU, out VisualElement globalMenu))
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

            while (!TryGetMenu(m_globalMenu.menu.items[index].targetMenu, out Menu _))
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
            SetMenu(m_globalMenu.menu.items[index].targetMenu);
        }
        #endregion
    }
}
