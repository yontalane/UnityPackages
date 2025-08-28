using UnityEngine;
using Yontalane.UIElements;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace YontalaneDemos.UIElements
{
    /// <summary>
    /// Controls the main menu UI logic for the UI Elements demo, including handling queries, inventory management, and button interactions.
    /// </summary>
    public class MainMenu : MenuManager
    {
        private const int MAX_INVENTORY = 10;
        private readonly string[] FRUIT = new string[] { "Apple", "Pear", "Mango", "Durian" };

        private Query m_query;
        private int m_inventoryID;
        private readonly List<string> m_inventory = new();

        /// <summary>
        /// Initializes the main menu, sets up the sample query, and prepares the inventory state.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            // Create a new Query instance for the popup, with a callback to handle the user's response.
            m_query = new("Sample Query", "What fruit is best?", FRUIT, (e) =>
            {
                // Refocus the popup button after the query closes.
                SetFocus("PopupButton");
                // If the query was cancelled, do nothing further.
                if (e.chosenResponseText == Query.CANCEL)
                {
                    return;
                }
                // Show a notification with the selected fruit.
                Document.rootVisualElement.Add(new Notification(Document, "Sample Notification", $"{e.chosenResponseText} is best."));
            })
            {
                CanCancel = true
            };

            // Reset inventory state.
            m_inventoryID = 0;
            m_inventory.Clear();

            // Disable the "Drop" button initially since inventory is empty.
            SetEnabled("Inventory", "Drop", false);
        }

        /// <summary>
        /// Handles click events for menu items in the main menu and inventory.
        /// </summary>
        /// <param name="menu">The name of the menu where the click occurred.</param>
        /// <param name="item">The name of the clicked item.</param>
        protected override void OnClick(ClickData clickData)
        {
            switch(clickData.menu)
            {
                case "Main":
                    // Handle clicks in the "Main" menu.
                    switch (clickData.item)
                    {
                        case "PopupButton":
                            // Show the sample query popup and focus it.
                            Document.rootVisualElement.Add(m_query);
                            m_query.Focus();
                            break;
                    }
                    break;
                case "Inventory":
                    // Handle clicks in the "Inventory" menu.
                    switch (clickData.item)
                    {
                        case "Add":
                            // Add a random fruit to the inventory.
                            string text = FRUIT[Mathf.FloorToInt(FRUIT.Length * Random.value)];
                            string id = $"{text}{m_inventoryID}";
                            Add(clickData.menu, new()
                            {
                                text = text,
                                name = id
                            });
                            m_inventoryID++;
                            m_inventory.Add(id);
                            // Enable/disable buttons based on inventory count.
                            SetEnabled(clickData.menu, "Add", m_inventory.Count < MAX_INVENTORY);
                            SetEnabled(clickData.menu, "Drop", true);
                            // If inventory is full, focus the "Drop" button.
                            if (m_inventory.Count >= MAX_INVENTORY)
                            {
                                SetFocus("Drop");
                            }
                            break;
                        case "Drop":
                            // Clear the inventory and reset buttons/focus.
                            Clear(clickData.menu);
                            m_inventory.Clear();
                            SetEnabled(clickData.menu, "Add", true);
                            SetEnabled(clickData.menu, "Drop", false);
                            SetFocus("Add");
                            break;
                        default:
                            // Skip processing if the item is set up for use in the inspector
                            if (clickData.inUse)
                            {
                                break;
                            }

                            // Remove a specific inventory item and update state.
                            _ = TryGetContainer(clickData.menu, out VisualElement container);
                            TextElement inventoryItem = container.Q<TextElement>(clickData.item);
                            int index = container.IndexOf(inventoryItem);

                            Remove(clickData.menu, clickData.item);
                            m_inventory.Remove(clickData.item);
                            SetEnabled(clickData.menu, "Add", m_inventory.Count < MAX_INVENTORY);
                            SetEnabled(clickData.menu, "Drop", m_inventory.Count > 0);

                            // Set focus to the appropriate item or button after removal.
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