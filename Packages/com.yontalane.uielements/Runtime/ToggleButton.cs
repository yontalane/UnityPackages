using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    public class ToggleButton : Button
    {
        #region Constants
        private const string STYLESHEET_RESOURCE = "ToggleButton";
        public const string CHECKED_STYLE_CLASS = "checked";
        #endregion

        #region UXML Methods
        public new class UxmlFactory : UxmlFactory<ToggleButton, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription m_bindingPath = new() { name = "binding-path", defaultValue = string.Empty };
            private readonly UxmlBoolAttributeDescription m_value = new() { name = "value", defaultValue = false };
            private readonly UxmlStringAttributeDescription m_text = new() { name = "text", defaultValue = string.Empty };
            private readonly UxmlStringAttributeDescription m_icon = new() { name = "icon", defaultValue = string.Empty };

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(visualElement, attributes, context);
                ToggleButton element = visualElement as ToggleButton;
                element.bindingPath = m_bindingPath.GetValueFromBag(attributes, context);
                element.value = m_value.GetValueFromBag(attributes, context);
                element.text = m_text.GetValueFromBag(attributes, context);
                element.Icon = m_icon.GetValueFromBag(attributes, context);
            }
        }
        #endregion

        public struct ToggleButtonChangeEvent
        {
            public bool oldValue;
            public bool newValue;
            public ToggleButton target;
        }

        public delegate void ToggleButtonChangeEventHandler(ToggleButtonChangeEvent e);
        public ToggleButtonChangeEventHandler OnChange = null;

        #region Private Variables
        private bool m_value;
        private string m_iconResource;
        private readonly VisualElement m_icon;
        private Label m_label;
        private string m_labelValue;
        #endregion

        #region Accessors
        public bool value
        {
            get => m_value;

            set
            {
                bool oldValue = m_value;
                SetValueWithoutNotify(value);
                OnChange?.Invoke(new ToggleButtonChangeEvent()
                {
                    oldValue = oldValue,
                    newValue = value,
                    target = this
                });
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
                    AddToClassList("with-image");
                }
                else
                {
                    RemoveFromClassList("with-image");
                }
            }
        }

        public new string text
        {
            get => m_labelValue;

            set
            {
                base.text = string.Empty;
                m_labelValue = value;
                m_label.text = value;
                if (!string.IsNullOrEmpty(value))
                {
                    AddToClassList("with-text");
                }
                else
                {
                    RemoveFromClassList("with-text");
                }
            }
        }
        #endregion

        #region Constructors
        public ToggleButton() : base()
        {
            base.text = string.Empty;

            m_icon = new()
            {
                name = "icon",
                focusable = false,
                pickingMode = PickingMode.Ignore
            };
            Add(m_icon);

            m_label = new()
            {
                name = "label",
                focusable = false,
                pickingMode = PickingMode.Ignore
            };
            Add(m_label);
            
            clicked += () =>
            {
                value = !value;
            };
            
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }
        #endregion

        public void SetValueWithoutNotify(bool value)
        {
            m_value = value;
            if (m_value)
            {
                AddToClassList(CHECKED_STYLE_CLASS);
            }
            else
            {
                RemoveFromClassList(CHECKED_STYLE_CLASS);
            }
        }
    }
}
