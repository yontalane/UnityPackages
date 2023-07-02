using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Yontalane.UIElements;

namespace YontalaneEditor.UIElements
{
    [CustomPropertyDrawer(typeof(GlobalMenu))]
    public class GlobalMenuUIE : MenuPropertyDrawerUIE
    {
        protected override string HeaderText => "Global Menu";

        protected override void MenuGUI(SerializedProperty property, VisualElement container)
        {
            SerializedProperty menu = property.FindPropertyRelative("menu");
            SerializedProperty name = menu.FindPropertyRelative("name");
            SerializedProperty items = menu.FindPropertyRelative("items");
            SerializedProperty addableContainer = menu.FindPropertyRelative("addableContainer");
            SerializedProperty defaultAddableItemTemplate = menu.FindPropertyRelative("defaultAddableItemTemplate");

            PropertyField nameField = new(name) { name = "Name" };
            PropertyField itemsField = new(items) { name = "Items" };
            PropertyField addableContainerField = new(addableContainer) { name = "Addable" };
            PropertyField defaultAddableItemTemplateField = new(defaultAddableItemTemplate) { name = "Template" };

            container.Add(nameField);
            container.Add(itemsField);
            container.Add(addableContainerField);
            container.Add(defaultAddableItemTemplateField);
        }
    }
}
