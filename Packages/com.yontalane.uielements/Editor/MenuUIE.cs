using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace YontalaneEditor.UIElements
{
    [CustomPropertyDrawer(typeof(Yontalane.UIElements.Menu))]
    public class MenuUIE : PropertyDrawer
    {
        #region Constants
        private const string CAN_CANCEL_CLASS = "can-cancel";
        private const string CANCEL_NORMAL_CLASS = "cancel-normal";
        private const string CANCEL_SUBORDINATE_CLASS = "cancel-subordinate";
        private const string CANCEL_DOMINANT_CLASS = "cancel-dominant";
        #endregion

        #region Private Variables
        private StyleSheet m_styleSheet;
        #endregion

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new() { name="Menu" };

            if (m_styleSheet == null)
            {
                m_styleSheet = Resources.Load<StyleSheet>(MenuPropertyDrawerUIE.STYLE_SHEET);
            }

            container.styleSheets.Add(m_styleSheet);

            SerializedProperty name = property.FindPropertyRelative("name");
            SerializedProperty items = property.FindPropertyRelative("items");
            SerializedProperty addableContainer = property.FindPropertyRelative("addableContainer");
            SerializedProperty defaultAddableItemTemplate = property.FindPropertyRelative("defaultAddableItemTemplate");
            SerializedProperty hasCancelTarget = property.FindPropertyRelative("hasCancelTarget");
            SerializedProperty cancelTarget = property.FindPropertyRelative("cancelTarget");
            SerializedProperty cancelTargetType = cancelTarget.FindPropertyRelative("type");
            SerializedProperty cancelTargetMenu = cancelTarget.FindPropertyRelative("targetMenu");
            SerializedProperty cancelTargetSubordinate = cancelTarget.FindPropertyRelative("targetSubordinate");
            SerializedProperty blockSideNavigation = property.FindPropertyRelative("blockSideNavigation");
            SerializedProperty hasGlobalMenu = property.FindPropertyRelative("hasGlobalMenu");

            if (!hasGlobalMenu.boolValue)
            {
                hasGlobalMenu.boolValue = true;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            PropertyField nameField = new(name) { name = "Name" };
            PropertyField itemsField = new(items) { name = "Items" };
            PropertyField addableContainerField = new(addableContainer) { name = "Addable" };
            PropertyField defaultAddableItemTemplateField = new(defaultAddableItemTemplate) { name = "Template" };
            PropertyField hasCancelTargetField = new(hasCancelTarget) { name = "HasCancelTarget" };
            PropertyField cancelTargetTypeField = new(cancelTargetType) { name = "CancelTargetType" };
            PropertyField cancelTargetMenuField = new(cancelTargetMenu) { name = "CancelTargetMenu" };
            PropertyField cancelTargetSubordinateField = new(cancelTargetSubordinate) { name = "CancelTargetSubordinate" };
            PropertyField blockSideNavigationField = new(blockSideNavigation) { name = "BlockSideNavigation" };

            container.Add(nameField);
            container.Add(itemsField);
            container.Add(hasCancelTargetField);
            container.Add(cancelTargetTypeField);
            container.Add(cancelTargetSubordinateField);
            container.Add(cancelTargetMenuField);
            container.Add(blockSideNavigationField);
            container.Add(addableContainerField);
            container.Add(defaultAddableItemTemplateField);

            hasCancelTargetField.RegisterValueChangeCallback((_) => UpdateCancelSettings(hasCancelTarget.boolValue, container));
            cancelTargetTypeField.RegisterValueChangeCallback((_) => UpdateCancelType(cancelTargetType.enumValueIndex, container));

            UpdateCancelSettings(hasCancelTarget.boolValue, container);
            UpdateCancelType(cancelTargetType.enumValueIndex, container);

            return container;
        }

        private void UpdateCancelSettings(bool canCancel, VisualElement root)
        {
            switch (canCancel)
            {
                case true:
                    root.AddToClassList(CAN_CANCEL_CLASS);
                    break;
                case false:
                    root.RemoveFromClassList(CAN_CANCEL_CLASS);
                    break;
            }
        }

        private void UpdateCancelType(int type, VisualElement root)
        {
            switch (type)
            {
                case 0:
                    root.AddToClassList(CANCEL_NORMAL_CLASS);
                    root.RemoveFromClassList(CANCEL_SUBORDINATE_CLASS);
                    root.RemoveFromClassList(CANCEL_DOMINANT_CLASS);
                    break;
                case 1:
                    root.RemoveFromClassList(CANCEL_NORMAL_CLASS);
                    root.AddToClassList(CANCEL_SUBORDINATE_CLASS);
                    root.RemoveFromClassList(CANCEL_DOMINANT_CLASS);
                    break;
                case 2:
                    root.RemoveFromClassList(CANCEL_NORMAL_CLASS);
                    root.RemoveFromClassList(CANCEL_SUBORDINATE_CLASS);
                    root.AddToClassList(CANCEL_DOMINANT_CLASS);
                    break;
            }
        }
    }
}
