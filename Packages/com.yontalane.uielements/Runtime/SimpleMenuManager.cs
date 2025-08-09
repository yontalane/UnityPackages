using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.UIElements
{
    /// <summary>
    /// A simple implementation of <see cref="MenuManager"/> that exposes UnityEvents for menu item and button interactions.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/UI Elements/Simple Menu Manager")]
    public class SimpleMenuManager : MenuManager
    {
        /// <summary>
        /// UnityEvent that is invoked when a menu item is interacted with.
        /// The first parameter is the menu name, and the second is the item name.
        /// </summary>
        [System.Serializable]
        public class MenuEvent : UnityEvent<string, string> { }

        /// <summary>
        /// UnityEvent that is invoked when a button is interacted with.
        /// The parameter is the button or item name.
        /// </summary>
        [System.Serializable]
        public class SimpleMenuEvent : UnityEvent<string> { }

        /// <summary>
        /// Event invoked when a menu item is interacted with.
        /// The first parameter is the menu name, and the second is the item name.
        /// </summary>
        [Tooltip("Event invoked when a menu item is interacted with. (string menuName, string itemName)")]
        public MenuEvent OnMenuItem;

        /// <summary>
        /// Event invoked when a button is interacted with.
        /// The parameter is the button or item name.
        /// </summary>
        [Tooltip("Event invoked when a button is interacted with. (string itemName)")]
        public SimpleMenuEvent OnButton;

        private void Reset()
        {
            OnMenuItem = null;
            OnButton = null;
        }

        protected override void OnClick(string menu, string item)
        {
            OnMenuItem?.Invoke(menu, item);
            OnButton?.Invoke(item);
        }
    }
}
