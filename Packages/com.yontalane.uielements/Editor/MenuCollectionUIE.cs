using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Yontalane.UIElements;

namespace YontalaneEditor.UIElements
{
    [CustomPropertyDrawer(typeof(MenuCollection))]
    public class MenuCollectionUIE : MenuPropertyDrawerUIE
    {
        #region Private Variables
        private SerializedProperty m_firstMenu;
        private SerializedProperty m_subordinates;
        private SerializedProperty m_menus;
        private readonly List<string> m_items = new();
        private DropdownField m_firstMenuDropdown;
        PropertyField m_subordinatesField;
        PropertyField m_menusField;
        #endregion

        protected override string HeaderText => "Menu Collection";

        protected override void MenuGUI(SerializedProperty property, VisualElement container)
        {
            m_firstMenu = property.FindPropertyRelative("firstMenu");
            m_subordinates = property.FindPropertyRelative("subordinates");
            m_menus = property.FindPropertyRelative("menus");

            m_firstMenuDropdown = new()
            {
                name = "FirstMenu",
                label = m_firstMenu.displayName
            };
            m_subordinatesField = new(m_subordinates) { name = "Subordinates" };
            m_menusField = new(m_menus) { name = "Menus" };

            BuildDropdown();

            m_firstMenuDropdown.RegisterValueChangedCallback((e) =>
            {
                m_firstMenu.intValue = m_items.IndexOf(e.newValue) - 1;
                m_firstMenu.serializedObject.ApplyModifiedProperties();
            });
            m_menusField.RegisterValueChangeCallback((e) => BuildDropdown());

            container.Add(m_firstMenuDropdown);
            container.Add(m_subordinatesField);
            container.Add(m_menusField);
        }

        private void BuildDropdown()
        {
            m_items.Clear();
            m_items.Add("None");
            for (int i = 0; i < m_menus.arraySize; i++)
            {
                m_items.Add(m_menus.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);
            }

            m_firstMenuDropdown.choices = m_items;
            m_firstMenuDropdown.index = m_firstMenu.intValue + 1;
        }
    }
}
