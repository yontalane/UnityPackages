using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    /// <summary>
    /// Represents the data required to add a UI element dynamically to a menu, including its name, display text, label identifier, template, and an optional callback to execute when the element is added.
    /// </summary>
    [Serializable]
    public struct AddableItemData
    {
        /// <summary>
        /// The unique UXML id for the new UI element instance.
        /// </summary>
        [Tooltip("The unique UXML id for the new UI element instance.")]
        public string name;

        /// <summary>
        /// The display text to assign to the UI element (e.g., button label).
        /// </summary>
        [Tooltip("The display text to assign to the UI element (e.g., button label).")]
        public string text;

        /// <summary>
        /// The UXML id of the label element within the template to set the text on.
        /// </summary>
        [Tooltip("The UXML id of the label element within the template to set the text on.")]
        public string label;

        /// <summary>
        /// The VisualTreeAsset template to use for this item. Leave null to use the menu's default template.
        /// </summary>
        [Tooltip("The VisualTreeAsset template to use for this item. Leave null to use the menu's default template.")]
        public VisualTreeAsset template;

        /// <summary>
        /// Optional callback to execute when the element is added to the menu.
        /// </summary>
        [Tooltip("Optional callback to execute when the element is added to the menu.")]
        public Action<VisualElement> onAdd;
    }

    /// <summary>
    /// Specifies the type of a menu item, determining its behavior and relationship within the menu system.
    /// </summary>
    [Serializable]
    public enum MenuItemType
    {
        Normal = 0,
        Subordinate = 1,
        Dominant = 2
    }

    /// <summary>
    /// Represents a single item within a menu, including its name, type, and navigation targets.
    /// </summary>
    [Serializable]
    public struct MenuItem
    {
        /// <summary>
        /// The unique name or identifier for this menu item.
        /// </summary>
        [Tooltip("The unique name or identifier for this menu item.")]
        public string name;

        /// <summary>
        /// The type of this menu item, determining its behavior (Normal, Subordinate, Dominant).
        /// </summary>
        [Tooltip("The type of this menu item, determining its behavior (Normal, Subordinate, Dominant).")]
        public MenuItemType type;

        /// <summary>
        /// The name of the target menu to navigate to when this item is selected (if applicable).
        /// </summary>
        [Tooltip("The name of the target menu to navigate to when this item is selected (if applicable).")]
        public string targetMenu;

        /// <summary>
        /// The index of the subordinate MenuManager to activate when this item is selected (used if type is Subordinate).
        /// </summary>
        [Tooltip("The index of the subordinate MenuManager to activate when this item is selected (used if type is Subordinate).")]
        [Min(0)]
        public int targetSubordinate;
    }

    /// <summary>
    /// Represents the global menu configuration, including the menu itself and whether to reset focus when activated.
    /// </summary>
    [Serializable]
    public struct GlobalMenu
    {
#if UNITY_EDITOR
#pragma warning disable IDE0079
        public bool editorExpanded;
#pragma warning restore IDE0079
#endif

        /// <summary>
        /// The global menu instance associated with this configuration.
        /// </summary>
        [Tooltip("The global menu instance associated with this configuration.")]
        public Menu menu;

        /// <summary>
        /// Whether to reset focus to the default item when the global menu is activated.
        /// </summary>
        [Tooltip("Whether to reset focus to the default item when the global menu is activated.")]
        public bool resetFocus;
    }

    /// <summary>
    /// Represents a collection of menus managed by a MenuManager, including subordinate managers and menu definitions.
    /// </summary>
    [Serializable]
    public struct MenuCollection
    {
#if UNITY_EDITOR
#pragma warning disable IDE0079
        public bool editorExpanded;
#pragma warning restore IDE0079
#endif

        /// <summary>
        /// The index of the first menu to be shown or selected by default. Use -1 for no default.
        /// </summary>
        [Tooltip("The index of the first menu to be shown or selected by default. Use -1 for no default.")]
        [Min(-1)]
        public int firstMenu;

        /// <summary>
        /// The array of subordinate MenuManagers managed by this collection.
        /// </summary>
        [Tooltip("The array of subordinate MenuManagers managed by this collection.")]
        public MenuManager[] subordinates;

        /// <summary>
        /// The array of menus included in this collection.
        /// </summary>
        [Tooltip("The array of menus included in this collection.")]
        public Menu[] menus;
    }

    /// <summary>
    /// Represents a UI menu, including its items, navigation behavior, and addable item configuration.
    /// </summary>
    [Serializable]
    public struct Menu
    {
        /// <summary>
        /// The unique name (uxml id) of the menu.
        /// </summary>
        [Tooltip("The unique name (uxml id) of the menu.")]
        public string name;

        /// <summary>
        /// The array of menu items contained in this menu.
        /// </summary>
        [Tooltip("The array of menu items contained in this menu.")]
        public MenuItem[] items;

        /// <summary>
        /// Whether this menu has a cancel target defined.
        /// </summary>
        [Tooltip("Whether this menu has a cancel target defined.")]
        public bool hasCancelTarget;

        /// <summary>
        /// The menu item to target when cancel is triggered.
        /// </summary>
        [Tooltip("The menu item to target when cancel is triggered.")]
        public MenuItem cancelTarget;

        /// <summary>
        /// Whether to block side navigation (e.g., left/right tabbing) in this menu.
        /// </summary>
        [Tooltip("Whether to block side navigation (e.g., left/right tabbing) in this menu.")]
        public bool blockSideNavigation;

        /// <summary>
        /// Whether this menu has an associated global menu.
        /// </summary>
        [Tooltip("Whether this menu has an associated global menu.")]
        public bool hasGlobalMenu;

        /// <summary>
        /// The uxml id of the container for addable items in this menu.
        /// </summary>
        [Tooltip("The uxml id of the container for addable items in this menu.")]
        public string addableContainer;

        /// <summary>
        /// The default VisualTreeAsset template for addable items in this menu.
        /// </summary>
        [Tooltip("The default VisualTreeAsset template for addable items in this menu.")]
        public VisualTreeAsset defaultAddableItemTemplate;

        /// <summary>
        /// The uxml id of the item to focus by default in this menu.
        /// </summary>
        [Tooltip("The uxml id of the item to focus by default in this menu.")]
        public string focusItem;
    }
}
