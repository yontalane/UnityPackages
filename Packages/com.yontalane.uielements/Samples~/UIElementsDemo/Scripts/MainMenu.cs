using UnityEngine;
using Yontalane.UIElements;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace YontalaneDemos.UIElements
{
    public class MainMenu : MenuManager
    {
        private const int MAX_INVENTORY = 5;
        private readonly string[] FRUIT = new string[] { "Apple", "Pear", "Mango", "Durian" };

        private Query m_query;
        private int m_inventoryID;
        private readonly List<string> m_inventory = new();

        protected override void Start()
        {
            base.Start();

            m_query = new("Sample Query", "What fruit is best?", FRUIT, (e) =>
            {
                SetFocus("PopupButton");
                if (e.chosenResponseText == Query.CANCEL)
                {
                    return;
                }
                Document.rootVisualElement.Add(new Notification(Document, "Sample Notification", $"{e.chosenResponseText} is best."));
            })
            {
                CanCancel = true
            };

            m_inventoryID = 0;
            m_inventory.Clear();

            SetEnabled("Inventory", "Drop", false);
        }

        protected override void OnClick(string menu, string item)
        {
            switch(menu)
            {
                case "Main":
                    switch (item)
                    {
                        case "PopupButton":
                            Document.rootVisualElement.Add(m_query);
                            m_query.Focus();
                            break;
                    }
                    break;
                case "Inventory":
                    switch (item)
                    {
                        case "Add":
                            string text = FRUIT[Mathf.FloorToInt(FRUIT.Length * Random.value)];
                            string id = $"{text}{m_inventoryID}";
                            Add(menu, new()
                            {
                                text = text,
                                name = id
                            });
                            m_inventoryID++;
                            m_inventory.Add(id);
                            SetEnabled(menu, "Add", m_inventory.Count < MAX_INVENTORY);
                            SetEnabled(menu, "Drop", true);
                            if (m_inventory.Count >= MAX_INVENTORY)
                            {
                                SetFocus("Drop");
                            }
                            break;
                        case "Drop":
                            Clear(menu);
                            m_inventory.Clear();
                            SetEnabled(menu, "Add", true);
                            SetEnabled(menu, "Drop", false);
                            SetFocus("Add");
                            break;
                        default:
                            _ = TryGetContainer(menu, out VisualElement container);
                            TextElement inventoryItem = container.Q<TextElement>(item);
                            int index = container.IndexOf(inventoryItem);

                            Remove(menu, item);
                            m_inventory.Remove(item);
                            SetEnabled(menu, "Add", m_inventory.Count < MAX_INVENTORY);
                            SetEnabled(menu, "Drop", m_inventory.Count > 0);

                            if (container.childCount == 0)
                            {
                                SetFocus("Add");
                            }
                            else if (index >= container.childCount)
                            {
                                SetFocus(m_inventory[^1]);
                            }
                            else
                            {
                                SetFocus(m_inventory[index]);
                            }
                            break;
                    }
                    break;
            }
        }
    }
}