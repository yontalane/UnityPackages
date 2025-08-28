using UnityEngine.UI;

namespace Yontalane.DialogUGUI
{
    public static class Extensions
    {
        /// <summary>
        /// Sets the active or interactable state of a Button based on the specified method.
        /// </summary>
        /// <param name="button">The Button to modify.</param>
        /// <param name="value">The desired interactable state.</param>
        /// <param name="activeMethod">The method used to control the button's state.</param>
        internal static void SetActive(this Button button, bool value, ButtonActiveMethod activeMethod)
        {
            // Check if the button is null and exit early if so.
            if (button == null)
            {
                return;
            }

            // Set the button's state based on the specified active method.
            switch (activeMethod)
            {
                case ButtonActiveMethod.Interactable:
                    // Set the button's interactable property.
                    button.interactable = value;
                    break;

                case ButtonActiveMethod.Active:
                    // Set the button's GameObject active state.
                    button.gameObject.SetActive(value);
                    break;
            }
        }

        /// <summary>
        /// Gets the active or interactable state of a Button based on the specified method.
        /// </summary>
        /// <param name="button">The Button to check.</param>
        /// <param name="activeMethod">The method used to determine the button's state.</param>
        /// <returns>True if the button is active or interactable according to the method; otherwise, false.</returns>
        internal static bool GetActive(this Button button, ButtonActiveMethod activeMethod)
        {
            // Return false if the button is null or inactive in the hierarchy.
            if (button == null || button.gameObject.activeInHierarchy)
            {
                return false;
            }

            // Determine the button's state based on the specified active method.
            switch (activeMethod)
            {
                case ButtonActiveMethod.Interactable:
                    // Return the value of the button's interactable property.
                    return button.interactable;

                case ButtonActiveMethod.Active:
                    // Return whether the button's GameObject is active.
                    return button.gameObject.activeSelf;
            }

            // Return false if the active method does not match any known case.
            return false;
        }
    }
}
