using UnityEngine;
using Yontalane.Dialog;

namespace Yontalane.Demos.Dialog
{
    [DisallowMultipleComponent]
    public sealed class InventoryManager : DialogResponder
    {
        [Header("Inventory")]

        [SerializeField] private string[] m_allItems = new string[] { "Apple", "Pear", "Banana", "Avocado", "Pineapple" };

        [Header("References")]

        [SerializeField] private RectTransform m_inventoryContainerUI = null;
        [SerializeField] private InventoryItemUI m_inventoryItemPrefabUI = null;

        private static InventoryManager s_instance = null;

        private void Awake() => s_instance = this;

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
        public override bool DialogFunction(string call, string parameter, out string result)
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
            if (s_instance == null) return null;

            for (int i = 0; i < s_instance.m_inventoryContainerUI.childCount; i++)
            {
                InventoryItemUI testInventoryItemUI = s_instance.m_inventoryContainerUI.GetChild(i).GetComponent<InventoryItemUI>();
                if (testInventoryItemUI != null && testInventoryItemUI.Name.Equals(itemName))
                {
                    return testInventoryItemUI;
                }
            }

            return null;
        }
    }
}