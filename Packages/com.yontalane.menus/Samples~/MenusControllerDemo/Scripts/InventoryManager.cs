using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yontalane.Menus;

namespace Yontalane.Demos.Menus
{
    [DisallowMultipleComponent]
    public class InventoryManager : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        [Tooltip("The details inspector window alongside the inventory list.")]
        private GameObject m_detailsView = null;

        [SerializeField]
        [Tooltip("The field for the item's name within the inventory details inspector.")]
        private Text m_itemNameField = null;

        private Selectable m_activeItem = null;

        #endregion

        #region Initialization

        private void Start() => m_detailsView.SetActive(false);

        /// <summary>
        /// Add listeners for menu interaction events.
        /// </summary>
        private void OnEnable()
        {
            MenuManager.OnActivateMenu += OnActivateMenu;
            MenuManager.OnMenuItemClick += OnMenuItemClick;
            MenuManager.OnMenuItemSelect += OnMenuItemSelect;
            MenuManager.OnMenuButton += OnMenuButton;
        }

        /// <summary>
        /// Remove listeners for menu interaction events.
        /// </summary>
        private void OnDisable()
        {
            MenuManager.OnActivateMenu -= OnActivateMenu;
            MenuManager.OnMenuItemClick -= OnMenuItemClick;
            MenuManager.OnMenuItemSelect -= OnMenuItemSelect;
            MenuManager.OnMenuButton -= OnMenuButton;
        }

        #endregion

        #region Event Receivers

        /// <summary>
        /// When a menu (not a menu item) is activated...
        /// </summary>
        private void OnActivateMenu(Menu menu)
        {
            switch (menu.name)
            {
                case "Inventory":
                    {
                        Selectable item = menu.selectables[menu.activeSelectable];
                        if (item != null && item.name.Contains("Item "))
                        {
                            m_activeItem = item;
                            m_itemNameField.text = ActiveItemLabel;
                            m_detailsView.SetActive(true);
                        }
                        else
                        {
                            m_activeItem = null;
                            m_detailsView.SetActive(false);
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// When a controller button or key is pressed in a menu...
        /// </summary>
        private void OnMenuButton(Menu menu, string buttonName)
        {
            if (m_activeItem == null) return;

            switch (menu.name)
            {
                case "Inventory":
                    switch (buttonName)
                    {
                        case "Face Button West":
                            menu.Remove(m_activeItem);
                            break;
                        case "Face Button North":
                            Add(menu, ActiveItemLabel, m_activeItem);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// When a menu button item is activated...
        /// </summary>
        private void OnMenuItemClick(Menu menu, Selectable item)
        {
            switch (menu.name)
            {
                case "Main":
                    if (item.name == "Continue Button")
                    {
                        Debug.Log($"<b>Continue Game</b>");
                    }
                    else if (item.name == "Quit Button")
                    {
                        Debug.Log($"<b>Quit Game</b>");
                    }
                    break;
                case "Inventory":
                    if (item.name == "Add Button")
                    {
                        Add(menu);
                    }
                    else if (item.name == "Clear Button")
                    {
                        menu.Clear();
                    }
                    break;
            }
        }

        /// <summary>
        /// When a menu item is highlighted...
        /// </summary>
        private void OnMenuItemSelect(Menu menu, Selectable item)
        {
            if (menu.name == "Inventory" && item.name.Contains("Item "))
            {
                m_activeItem = item;
                m_itemNameField.text = ActiveItemLabel;
                m_detailsView.SetActive(true);
            }
            else
            {
                m_activeItem = null;
                m_detailsView.SetActive(false);
            }
        }

        #endregion

        /// <summary>
        /// Generate a new item with a random name in the inventory list.
        /// </summary>
        private void Add(Menu menu, string label = null, Selectable targetLocation = null) => menu.Add("Item " + Mathf.FloorToInt(Random.value * 100f).ToString(), label, targetLocation, false);

        /// <summary>
        /// The label text of the currently selected menu item.
        /// </summary>
        private string ActiveItemLabel => m_activeItem != null ? m_activeItem.GetComponentInChildren<TMP_Text>().text : "";
    }
}