using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    public class ToggleButton : Toggle
    {
        #region Constants
        private const string STYLESHEET_RESOURCE = "ToggleButton";
        #endregion

        #region UXML Methods
        public new class UxmlFactory : UxmlFactory<ToggleButton, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription m_bindingPath = new() { name = "binding-path", defaultValue = string.Empty };
            private readonly UxmlBoolAttributeDescription m_value = new() { name = "value", defaultValue = false };
            private readonly UxmlStringAttributeDescription m_label = new() { name = "label", defaultValue = "Label" };

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(visualElement, attributes, context);
                ToggleButton element = visualElement as ToggleButton;
                element.bindingPath = m_bindingPath.GetValueFromBag(attributes, context);
                element.value = m_value.GetValueFromBag(attributes, context);
                element.label = m_label.GetValueFromBag(attributes, context);
            }
        }
        #endregion

        #region Accessors
        public new string label
        {
            get
            {
                return base.label;
            }
            set
            {
                this.Q<Label>("StubLabel").style.display = string.IsNullOrEmpty(value) ? DisplayStyle.Flex : DisplayStyle.None;
                base.label = value;
            }
        }
        #endregion

        #region Constructors
        public ToggleButton() : base()
        {
            Label stubLabel = new() { name = "StubLabel" };
            stubLabel.AddToClassList(textUssClassName);
            Add(stubLabel);
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }
        #endregion
    }
}
