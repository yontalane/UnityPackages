using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Yontalane.UIElements;

namespace YontalaneEditor.UIElements
{
    [CustomPropertyDrawer(typeof(Sounds))]
    public class SoundsUIE : MenuPropertyDrawerUIE
    {
        protected override string HeaderText => "Sounds";

        protected override void MenuGUI(SerializedProperty property, VisualElement container)
        {
            SerializedProperty mute = property.FindPropertyRelative("mute");
            SerializedProperty click = property.FindPropertyRelative("click");
            SerializedProperty navigation = property.FindPropertyRelative("navigation");
            SerializedProperty tab = property.FindPropertyRelative("tab");
            SerializedProperty cancel = property.FindPropertyRelative("cancel");

            PropertyField muteField = new(mute) { name = "Mute" };
            PropertyField clickField = new(click) { name = "Click" };
            PropertyField navigationField = new(navigation) { name = "Navigation" };
            PropertyField tabField = new(tab) { name = "Tab" };
            PropertyField cancelField = new(cancel) { name = "Cancel" };

            container.Add(muteField);
            container.Add(clickField);
            container.Add(navigationField);
            container.Add(tabField);
            container.Add(cancelField);
        }
    }
}
