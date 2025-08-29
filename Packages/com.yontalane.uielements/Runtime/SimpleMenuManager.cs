using UnityEngine;

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

        /// <summary>
        /// Handles menu item click events by invoking UnityEvents.
        /// Invokes <see cref="OnMenuItem"/> with the menu and item names,
        /// and <see cref="OnButton"/> with the item name, unless the item is marked as "in use".
        /// </summary>
        /// <param name="clickData">Information about the clicked menu item.</param>
        protected override void OnClick(ClickData clickData)
        {
            if (clickData.inUse)
            {
                return;
            }

            OnMenuItem?.Invoke(clickData.menu, clickData.item);
            OnButton?.Invoke(clickData.item);
        }
    }
}
