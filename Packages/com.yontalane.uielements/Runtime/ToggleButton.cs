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

        #region Private Variables
        private readonly Label m_stubLabel;
        private readonly VisualElement m_box;
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
                m_stubLabel.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.Flex : DisplayStyle.Flex;
                base.label = value;
                m_stubLabel.text = value;
                RemoveFocusable();
            }
        }
        #endregion

        #region Constructors
        public ToggleButton() : base()
        {
            m_box = this.Query<VisualElement>(className: "unity-toggle__input").First();
            m_box.name = "Frame";
            m_box.AddToClassList("yontalane-frame");

            m_stubLabel = new() { name = "Label" };
            m_stubLabel.AddToClassList(textUssClassName);
            m_box.Add(m_stubLabel);

            RemoveFocusable();

            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }
        #endregion

        private void RemoveFocusable()
        {
            m_stubLabel.pickingMode = PickingMode.Ignore;
            m_stubLabel.focusable = false;

            m_box.pickingMode = pickingMode;
            m_box.focusable = focusable;

            VisualElement checkmark = this.Q<VisualElement>("unity-checkmark");
            checkmark.pickingMode = PickingMode.Ignore;
            checkmark.focusable = false;

            if (m_Label != null)
            {
                m_Label.pickingMode = PickingMode.Ignore;
                m_Label.focusable = false;
            }
        }
    }
}
