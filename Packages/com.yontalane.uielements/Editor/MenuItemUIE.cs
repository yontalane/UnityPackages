using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace YontalaneEditor.UIElements
{
    [CustomPropertyDrawer(typeof(Yontalane.UIElements.MenuItem))]
    public class MenuItemUIE : PropertyDrawer
    {
        #region Private Variables
        private StyleSheet m_styleSheet;
        private VisualElement m_container;
        private SerializedProperty m_type;
        #endregion

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            m_container = new() { name = "MenuItem" };

            if (m_styleSheet == null)
            {
                m_styleSheet = Resources.Load<StyleSheet>(MenuPropertyDrawerUIE.STYLE_SHEET);
            }

            m_container.styleSheets.Add(m_styleSheet);

            SerializedProperty name = property.FindPropertyRelative("name");
            m_type = property.FindPropertyRelative("type");
            SerializedProperty targetMenu = property.FindPropertyRelative("targetMenu");
            SerializedProperty targetSubordinate = property.FindPropertyRelative("targetSubordinate");

            PropertyField nameField = new(name) { name = "Name" };
            PropertyField typeField = new(m_type) { name = "Type" };
            PropertyField targetMenuField = new(targetMenu) { name = "TargetMenu", label = "Target" };
            PropertyField targetSubordinateField = new(targetSubordinate) { name = "TargetSubordinate", label = "Target" };

            typeField.RegisterValueChangeCallback((e) => UpdateStyle());

            UpdateStyle();

            m_container.Add(nameField);
            m_container.Add(typeField);
            m_container.Add(targetSubordinateField);
            m_container.Add(targetMenuField);

            return m_container;
        }

        private void UpdateStyle()
        {
            switch (m_type.enumValueIndex)
            {
                case 0:
                    m_container.RemoveFromClassList("subordinateMenuItem");
                    m_container.RemoveFromClassList("dominantMenuItem");
                    m_container.AddToClassList("normalMenuItem");
                    break;
                case 1:
                    m_container.RemoveFromClassList("normalMenuItem");
                    m_container.RemoveFromClassList("dominantMenuItem");
                    m_container.AddToClassList("subordinateMenuItem");
                    break;
                case 2:
                    m_container.RemoveFromClassList("normalMenuItem");
                    m_container.RemoveFromClassList("subordinateMenuItem");
                    m_container.AddToClassList("dominantMenuItem");
                    break;
            }
        }
    }
}
