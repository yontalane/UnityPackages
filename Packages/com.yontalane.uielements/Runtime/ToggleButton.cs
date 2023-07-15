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
            private readonly UxmlStringAttributeDescription m_icon = new() { name = "icon", defaultValue = string.Empty };

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(visualElement, attributes, context);
                ToggleButton element = visualElement as ToggleButton;
                element.bindingPath = m_bindingPath.GetValueFromBag(attributes, context);
                element.value = m_value.GetValueFromBag(attributes, context);
                element.label = m_label.GetValueFromBag(attributes, context);
                element.Icon = m_icon.GetValueFromBag(attributes, context);
            }
        }
        #endregion

        #region Accessors
        public new string label
        {
            get => base.label;

            set
            {
                base.label = value;
                RemoveFocusable();
            }
        }

        public string Icon
        {
            get => m_iconResource;

            set
            {
                m_iconResource = value;
                Sprite sprite = Resources.Load<Sprite>(m_iconResource);
                m_icon.style.backgroundImage = new StyleBackground(sprite);
                if (sprite != null)
                {
                    m_icon.AddToClassList("with-image");
                }
                else
                {
                    m_icon.RemoveFromClassList("with-image");
                }
            }
        }
        #endregion

        #region Private Variables
        private string m_iconResource;
        private readonly VisualElement m_icon;
        private readonly VisualElement m_checkmark;
        #endregion

        #region Constructors
        public ToggleButton() : base()
        {
            AddToClassList("yontalane-toggle-button");

            m_checkmark = this.Q<VisualElement>("unity-checkmark");

            m_icon = new();
            m_icon.AddToClassList("icon");
            Add(m_icon);

            RemoveFocusable();

            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }
        #endregion

        private void RemoveFocusable()
        {
            m_checkmark.pickingMode = PickingMode.Ignore;
            m_checkmark.focusable = false;

            m_icon.pickingMode = PickingMode.Ignore;
            m_icon.focusable = false;

            if (m_Label != null)
            {
                m_Label.pickingMode = PickingMode.Ignore;
                m_Label.focusable = false;
            }
        }
    }
}
