using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Yontalane.Dialog;

namespace Yontalane.Demos.DialogUIElements
{
    /// <summary>
    /// Manages the player's inventory, handles inventory UI, and responds to dialog system queries about inventory state.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Dialog UI Toolkit/Inventory Manager")]
    public sealed class InventoryManager : Singleton<InventoryManager>, IDialogResponder
    {
        [Header("Inventory")]

        [Tooltip("A list of all possible items that can exist in the player's inventory.")]
        [SerializeField]
        private string[] m_allItems = new string[] { "Apple", "Pear", "Banana", "Avocado", "Pineapple" };

        [Header("References")]

        [Tooltip("The UI Document containing the root VisualElement for the inventory UI.")]
        [SerializeField]
        private UIDocument m_document = null;

        [Tooltip("The name of the VisualElement container in the UI Document where inventory items will be displayed.")]
        [SerializeField]
        private string m_inventoryContainer = null;

        /// <summary>
        /// Gets the VisualElement container in the UI Document where inventory items are displayed.
        /// </summary>
        private static VisualElement InventoryContainer => Instance.m_document.rootVisualElement.Q<VisualElement>(Instance.m_inventoryContainer);

        /// <summary>
        /// Instantiate UI fields for each inventory item.
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < m_allItems.Length; i++)
            {
                InventoryContainer.Add(new SliderInt()
                {
                    showInputField = true,
                    label = m_allItems[i],
                    lowValue = 0,
                    highValue = 5,
                    value = 0
                });
            }
        }

        /// <summary>
        /// Callback to be used in DialogProcess thanks to this class inheriting from DialogResponder.
        /// Checks if we're querying the "Possesses" keyword and, if so, checks if we possess the item in our inventory.
        /// </summary>
        /// <param name="call">The callback query.</param>
        /// <param name="parameter">The query's parameter (the item we want to check our inventory for).</param>
        /// <param name="result">Whether or not we own the item.</param>
        /// <returns>Whether or not the function call was a success. In this case, that is equal to <c>result</c>.</returns>
        public bool DialogFunction(string call, string parameter, out string result)
        {
            // Switch on the function call to determine which inventory-related query to process
            switch (call)
            {
                // Handle the "Possesses" query to check if the player owns the specified item
                case "Possesses":
                    bool resultBool = false;
                    // Iterate through all SliderInt UI elements in the inventory container
                    InventoryContainer.Query<SliderInt>().ForEach(sliderInt =>
                    {
                        // If the slider's label matches the parameter and its value is greater than 0, the item is possessed
                        if (sliderInt.label == parameter && sliderInt.value > 0)
                        {
                            resultBool = true;
                            return;
                        }
                    });
                    // Set the result to the string representation of whether the item is possessed
                    result = resultBool.ToString();
                    return true;
            }
            // If the call does not match any known query, set result to null and return false
            result = null;
            return false;
        }

        /// <summary>
        /// Modify count of inventory UI field.
        /// </summary>
        public static void Add(string itemName)
        {
            InventoryContainer.Query<SliderInt>().ForEach(sliderInt =>
            {
                if (sliderInt.label == itemName)
                {
                    sliderInt.value++;
                    return;
                }
            });
        }

        /// <summary>
        /// Decreases the count of the specified item in the inventory UI field by one.
        /// </summary>
        /// <param name="itemName">The name of the item to remove from the inventory.</param>
        public static void Remove(string itemName)
        {
            InventoryContainer.Query<SliderInt>().ForEach(sliderInt =>
            {
                if (sliderInt.label == itemName)
                {
                    sliderInt.value--;
                    return;
                }
            });
        }

        /// <summary>
        /// Retrieves the value associated with the specified keyword from the inventory system.
        /// </summary>
        /// <param name="key">The keyword to look up.</param>
        /// <param name="result">The output value associated with the keyword, if found.</param>
        /// <returns>True if the keyword was found and a value was retrieved; otherwise, false.</returns>
        public bool GetKeyword(string key, out string result)
        {
            result = null;
            return false;
        }

        /// <summary>
        /// Retrieves inline image replacement information for the inventory system.
        /// </summary>
        /// <param name="info">A list to populate with inline image replacement information.</param>
        /// <returns>True if any inline image information was found and added; otherwise, false.</returns>
        public bool GetInlineImageInfo(List<InlineImageReplacementInfo> info)
        {
            return false;
        }

        /// <summary>
        /// Handles custom line data builder function calls for the inventory system.
        /// This implementation does not process any calls and always returns false.
        /// </summary>
        /// <param name="call">The name of the function to execute.</param>
        /// <param name="lineData">The line data to be processed.</param>
        /// <returns>Always returns false, as no line data builder functions are handled.</returns>
        public bool GetLineDataBuilderResult(string call, LineData lineData) => false;
    }
}