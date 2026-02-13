using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Yontalane.UIElements
{
    /// <summary>
    /// A navigation event handler.
    /// </summary>
    [System.Serializable]
    public class NavigationEvent : UnityEvent
    { }

    /// <summary>
    /// A navigation input event handler.
    /// </summary>
    [System.Serializable]
    public class NavigationInputEvent : UnityEvent<Vector2Int>
    { }

    /// <summary>
    /// A click event handler.
    /// </summary>
    [System.Serializable]
    public class ClickEvent : UnityEvent<ClickData>
    { }

    /// <summary>
    /// Represents the input configuration for menu controls, including input actions and button mappings.
    /// </summary>
    [System.Serializable]
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

        [Tooltip("The InputActionAsset containing all input actions for menu navigation.")]
        public InputActionAsset actions;

        [Tooltip("The action name or path for basic navigation.")]
        public string directionalInput;

        [Tooltip("The action name or path for navigating to the previous tab.")]
        public string tabLeft;

        [Tooltip("The action name or path for navigating to the next tab.")]
        public string tabRight;

        [Tooltip("An array of action names or paths for menu button actions (e.g., submit, cancel).")]
        public string[] buttons;
    }

    /// <summary>
    /// Contains UnityEvents for menu interactions such as clicking, navigation, tab changes, and cancel actions.
    /// </summary>
    [System.Serializable]
    public struct Listeners
    {
#if UNITY_EDITOR
#pragma warning disable IDE0079
        /// <summary>
        /// Used in the Unity Editor to track whether the struct is expanded in the inspector.
        /// </summary>
        public bool editorExpanded;
#pragma warning restore IDE0079
#endif
        [Tooltip("An event that is broadcast on menu item clicking.")]
        public ClickEvent onClick;

        [Tooltip("An event that is broadcast on navigation input.")]
        public NavigationInputEvent onNavigationInput;

        [Tooltip("An event that is broadcast on menu item navigation.")]
        public NavigationEvent onNavigation;

        [Tooltip("An event that is broadcast on menu tab navigation.")]
        public NavigationEvent onTabNavigation;

        [Tooltip("An event that is broadcast when the user presses the cancel key to back out of a menu.")]
        public NavigationEvent onCancel;
    }

    /// <summary>
    /// Contains optional AudioClips for menu sound effects, such as clicking, navigation, tab changes, and cancel actions.
    /// </summary>
    [System.Serializable]
    public struct Sounds
    {
#if UNITY_EDITOR
#pragma warning disable IDE0079
        /// <summary>
        /// Used in the Unity Editor to track whether the struct is expanded in the inspector.
        /// </summary>
        public bool editorExpanded;
#pragma warning restore IDE0079
#endif
        [Tooltip("Toggles whether sounds should play.")]
        public bool mute;

        [Tooltip("An optional sound effect for menu item clicking.")]
        public AudioClip click;

        [Tooltip("An optional sound effect for menu item navigation.")]
        public AudioClip navigation;

        [Tooltip("An optional sound effect for menu tab navigation.")]
        public AudioClip tab;

        [Tooltip("An optional sound effect for when the user presses the cancel key to back out of a menu.")]
        public AudioClip cancel;
    }

    /// <summary>
    /// Represents data about a menu click event, including the menu name, item name, and whether the item is set up for use in the inspector.
    /// </summary>
    public struct ClickData
    {
        /// <summary>
        /// The name of the menu where the click event occurred.
        /// </summary>
        public string menu;

        /// <summary>
        /// The name of the item that was clicked within the menu.
        /// </summary>
        public string item;

        /// <summary>
        /// Indicates whether the clicked item is set up for use in the inspector.
        /// </summary>
        public bool inUse;
    }

    /// <summary>
    /// An enum represeting the cardinal directions.
    /// </summary>
    public enum Directions
    {
        Up = 0,
        Right = 10,
        Down = 20,
        Left = 30,
    }

    #region Simple Menu Manager
    /// <summary>
    /// UnityEvent that is invoked when a menu item is interacted with.
    /// The first parameter is the menu name, and the second is the item name.
    /// </summary>
    [System.Serializable]
    public class MenuEvent : UnityEvent<string, string>
    { }

    /// <summary>
    /// UnityEvent that is invoked when a button is interacted with.
    /// The parameter is the button or item name.
    /// </summary>
    [System.Serializable]
    public class SimpleMenuEvent : UnityEvent<string>
    { }
    #endregion
}