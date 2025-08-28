using System.Collections.Generic;
using UnityEngine;
using Yontalane.Dialog;

namespace Yontalane.Demos.Dialog
{
    /// <summary>
    /// Manages the player's inventory, handles inventory UI, and responds to dialog system queries about inventory state.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Dialog/Inventory Manager")]
    public sealed class InventoryManager : Singleton<InventoryManager>, IDialogResponder
    {
        [Header("Inventory")]

        [Tooltip("A list of all possible items that can exist in the player's inventory.")]
        [SerializeField]
        private string[] m_allItems = new string[] { "Apple", "Pear", "Banana", "Avocado", "Pineapple" };

        [Header("References")]

        [Tooltip("The UI container that will hold all inventory item UI elements.")]
        [SerializeField]
        private RectTransform m_inventoryContainerUI = null;

        [Tooltip("The prefab used to instantiate UI elements for each inventory item.")]
        [SerializeField]
        private InventoryItemUI m_inventoryItemPrefabUI = null;

        /// <summary>
        /// Instantiate UI fields for each inventory item.
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < m_allItems.Length; i++)
            {
                InventoryItemUI instance = Instantiate(m_inventoryItemPrefabUI.gameObject).GetComponent<InventoryItemUI>();
                instance.Name = m_allItems[i];
                instance.Count = 0;
                instance.transform.SetParent(m_inventoryContainerUI);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localEulerAngles = Vector3.zero;
                instance.transform.localScale = Vector3.one;
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
            switch (call)
            {
                case "Possesses":
                    for (int i = 0; i < m_inventoryContainerUI.childCount; i++)
                    {
                        InventoryItemUI itemUI = m_inventoryContainerUI.GetChild(i).GetComponent<InventoryItemUI>();
                        if (itemUI.Name.Equals(parameter) && itemUI.Count > 0)
                        {
                            result = true.ToString();
                            return true;
                        }
                    }
                    break;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Modify count of inventory UI field.
        /// </summary>
        public static void Add(string itemName)
        {
            InventoryItemUI itemUI = GetItemUI(itemName);
            if (itemUI != null) itemUI.AddItem();
        }

        /// <summary>
        /// Modify count of inventory UI field.
        /// </summary>
        public static void Remove(string itemName)
        {
            InventoryItemUI itemUI = GetItemUI(itemName);
            if (itemUI != null) itemUI.RemoveItem();
        }

        /// <summary>
        /// Get inventory UI field by item name.
        /// </summary>
        private static InventoryItemUI GetItemUI(string itemName)
        {
            if (Instance == null)
            {
                return null;
            }

            for (int i = 0; i < Instance.m_inventoryContainerUI.childCount; i++)
            {
                InventoryItemUI testInventoryItemUI = Instance.m_inventoryContainerUI.GetChild(i).GetComponent<InventoryItemUI>();
                if (testInventoryItemUI != null && testInventoryItemUI.Name.Equals(itemName))
                {
                    return testInventoryItemUI;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the value associated with the specified inventory-related keyword.
        /// </summary>
        public bool GetKeyword(string key, out string result)
        {
            result = null;
            return false;
        }

        /// <summary>
        /// Adds inline image replacement information for inventory-related keywords to the provided list.
        /// </summary>
        public bool GetInlineImageInfo(List<InlineImageReplacementInfo> info)
        {
            return false;
        }

        /// <summary>
        /// Attempts to construct or modify a LineData object for a given dialog line specific to the inventory system.
        /// </summary>
        /// <param name="call">The text of the dialog line to process.</param>
        /// <param name="lineData">The resulting LineData object after processing, if any.</param>
        /// <returns>True if the line builder was invoked and lineData was set; otherwise, false.</returns>
        public bool GetLineDataBuilderResult(string call, out LineData lineData)
        {
            lineData = null;
            return false;
        }
    }
}