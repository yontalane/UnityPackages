using UnityEditor;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Yontalane.UIElements;
using UnityEngine.InputSystem;

namespace YontalaneEditor.UIElements
{
    [CustomPropertyDrawer(typeof(ControlInput))]
    public class ControlInputUIE : MenuPropertyDrawerUIE
    {
        private const string NONE = "None";

        private SerializedProperty m_actions;
        private SerializedProperty m_directionalInput;
        private SerializedProperty m_tabLeft;
        private SerializedProperty m_tabRight;
        private SerializedProperty m_buttons;
        private DropdownField m_directionalInputDropdown;
        private DropdownField m_tabLeftDropdown;
        private DropdownField m_tabRightDropdown;
        private readonly List<string> m_choices = new();

        protected override string HeaderText => "Extra Inputs";

        protected override void MenuGUI(SerializedProperty property, VisualElement container)
        {
            m_actions = property.FindPropertyRelative("actions");
            m_directionalInput = property.FindPropertyRelative("directionalInput");
            m_tabLeft = property.FindPropertyRelative("tabLeft");
            m_tabRight = property.FindPropertyRelative("tabRight");
            m_buttons = property.FindPropertyRelative("buttons");

            PropertyField actionsField = new(m_actions) { name = "Actions" };
            actionsField.RegisterValueChangeCallback((_) => BuildDropdowns());
            container.Add(actionsField);

            m_directionalInputDropdown = new()
            {
                name = "Navigation",
                label = m_directionalInput.displayName
            };
            m_directionalInputDropdown.AddToClassList("control");
            m_directionalInputDropdown.RegisterValueChangedCallback((e) =>
            {
                m_directionalInput.stringValue = !string.IsNullOrWhiteSpace(e.newValue) && e.newValue != NONE ? e.newValue : string.Empty;
                m_directionalInput.serializedObject.ApplyModifiedProperties();
            });

            m_tabLeftDropdown = new()
            {
                name = "TabLeft",
                label = m_tabLeft.displayName
            };
            m_tabLeftDropdown.AddToClassList("control");
            m_tabLeftDropdown.RegisterValueChangedCallback((e) =>
            {
                m_tabLeft.stringValue = !string.IsNullOrWhiteSpace(e.newValue) && e.newValue != NONE ? e.newValue : string.Empty;
                m_tabLeft.serializedObject.ApplyModifiedProperties();
            });

            m_tabRightDropdown = new()
            {
                name = "TabRight",
                label = m_tabRight.displayName
            };
            m_tabRightDropdown.AddToClassList("control");
            m_tabRightDropdown.RegisterValueChangedCallback((e) =>
            {
                m_tabRight.stringValue = !string.IsNullOrWhiteSpace(e.newValue) && e.newValue != NONE ? e.newValue : string.Empty;
                m_tabRight.serializedObject.ApplyModifiedProperties();
            });

            BuildDropdowns();

            container.Add(m_directionalInputDropdown);
            container.Add(m_tabLeftDropdown);
            container.Add(m_tabRightDropdown);

            PropertyField buttonsField = new(m_buttons) { name = "Buttons" };
            container.Add(buttonsField);
        }

        private void BuildDropdowns()
        {
            InputActionAsset inputActions = m_actions.objectReferenceValue as InputActionAsset;
            bool actionsExist = inputActions != null;
            m_directionalInputDropdown.SetEnabled(actionsExist);
            m_tabLeftDropdown.SetEnabled(actionsExist);
            m_tabRightDropdown.SetEnabled(actionsExist);
            m_choices.Clear();

            if (actionsExist)
            {
                m_choices.Add(NONE);
                foreach (InputActionMap map in inputActions.actionMaps)
                {
                    foreach (InputAction inputAction in map.actions)
                    {
                        m_choices.Add($"{map.name}/{inputAction.name}");
                    }
                }
            }
            else
            {
                m_choices.Add(string.Empty);
            }

            m_directionalInputDropdown.choices = m_choices;
            m_directionalInputDropdown.index = m_choices.IndexOf(!string.IsNullOrEmpty(m_directionalInput.stringValue) ? m_directionalInput.stringValue : NONE);
            m_tabLeftDropdown.choices = m_choices;
            m_tabLeftDropdown.index = m_choices.IndexOf(!string.IsNullOrEmpty(m_tabLeft.stringValue) ? m_tabLeft.stringValue : NONE);
            m_tabRightDropdown.choices = m_choices;
            m_tabRightDropdown.index = m_choices.IndexOf(!string.IsNullOrEmpty(m_tabRight.stringValue) ? m_tabRight.stringValue : NONE);
        }
    }
}
