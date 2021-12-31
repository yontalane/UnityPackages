using UnityEngine;
using DEF.Dialog;

namespace DEF.Test
{
    public class TestInventoryManager : DialogResponder
    {
        [SerializeField] private RectTransform m_inventoryContainerUI = null;
        [SerializeField] private TestInventoryItemUI m_inventoryItemPrefabUI = null;
        [SerializeField] private string[] m_allItems = new string[] { "Apple", "Pear", "Banana", "Avocado", "Pineapple" };

        private void Start()
        {
            for (int i = 0; i < m_allItems.Length; i++)
            {
                TestInventoryItemUI instance = Instantiate(m_inventoryItemPrefabUI.gameObject).GetComponent<TestInventoryItemUI>();
                instance.Name = m_allItems[i];
                instance.Count = 0;
                instance.transform.SetParent(m_inventoryContainerUI);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localEulerAngles = Vector3.zero;
                instance.transform.localScale = Vector3.one;
            }
        }

        public override bool DialogFunction(string call, string parameter, out string result)
        {
            switch (call)
            {
                case "Possesses":
                    for (int i = 0; i < m_inventoryContainerUI.childCount; i++)
                    {
                        TestInventoryItemUI itemUI = m_inventoryContainerUI.GetChild(i).GetComponent<TestInventoryItemUI>();
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

        public static void Add(string itemName)
        {
            TestInventoryItemUI itemUI = GetItemUI(itemName);
            if (itemUI != null)
                itemUI.AddItem();
        }

        public static void Remove(string itemName)
        {
            TestInventoryItemUI itemUI = GetItemUI(itemName);
            if (itemUI != null)
                itemUI.RemoveItem();
        }

        private static TestInventoryItemUI GetItemUI(string itemName)
        {
            TestInventoryManager testInventoryManager = FindObjectOfType<TestInventoryManager>();
            if (testInventoryManager != null)
                for (int i = 0; i < testInventoryManager.m_inventoryContainerUI.childCount; i++)
                {
                    TestInventoryItemUI testInventoryItemUI = testInventoryManager.m_inventoryContainerUI.GetChild(i).GetComponent<TestInventoryItemUI>();
                    if (testInventoryItemUI != null && testInventoryItemUI.Name.Equals(itemName))
                        return testInventoryItemUI;
                }
            return null;
        }
    }
}