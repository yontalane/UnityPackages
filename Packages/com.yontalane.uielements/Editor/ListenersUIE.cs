using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Yontalane.UIElements;

namespace YontalaneEditor.UIElements
{
    [CustomPropertyDrawer(typeof(MenuManager.Listeners))]
    public class ListenersUIE : MenuPropertyDrawerUIE
    {
        protected override string HeaderText => "Listeners";

        protected override void MenuGUI(SerializedProperty property, VisualElement container)
        {
            SerializedProperty click = property.FindPropertyRelative("onClick");
            SerializedProperty navigation = property.FindPropertyRelative("onNavigation");
            SerializedProperty tab = property.FindPropertyRelative("onTabNavigation");
            SerializedProperty cancel = property.FindPropertyRelative("onCancel");

            PropertyField clickField = new(click) { name = "Click" };
            PropertyField navigationField = new(navigation) { name = "Navigation" };
            PropertyField tabField = new(tab) { name = "Tab" };
            PropertyField cancelField = new(cancel) { name = "Cancel" };

            container.Add(clickField);
            container.Add(navigationField);
            container.Add(tabField);
            container.Add(cancelField);
        }
    }
}
