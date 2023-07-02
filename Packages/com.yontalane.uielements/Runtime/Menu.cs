using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    [Serializable]
    public struct AddableItemData
    {
        public string name; // uxml id for new instance
        public string text;
        public string label; // uxml id for the label
        public VisualTreeAsset template; // leave null to use the default
    }

    [Serializable]
    public enum MenuItemType
    {
        Normal = 0,
        Subordinate = 1,
        Dominant = 2
    }

    [Serializable]
    public struct MenuItem
    {
        public string name;
        public MenuItemType type;
        public string targetMenu;

        [Min(0)]
        public int targetSubordinate;
    }

    [Serializable]
    public struct GlobalMenu
    {
#if UNITY_EDITOR
#pragma warning disable IDE0079
        public bool editorExpanded;
#pragma warning restore IDE0079
#endif

        public Menu menu;
    }

    [Serializable]
    public struct MenuCollection
    {
#if UNITY_EDITOR
#pragma warning disable IDE0079
        public bool editorExpanded;
#pragma warning restore IDE0079
#endif

        [Min(-1)]
        public int firstMenu;
        public MenuManager[] subordinates;
        public Menu[] menus;
    }

    [Serializable]
    public struct Menu
    {
        public string name; // uxml id
        public MenuItem[] items; // button id, target menu id
        public bool hasCancelTarget;
        public MenuItem cancelTarget;
        public bool hasGlobalMenu;

        public string addableContainer; // uxml id
        public VisualTreeAsset defaultAddableItemTemplate;
    }
}
