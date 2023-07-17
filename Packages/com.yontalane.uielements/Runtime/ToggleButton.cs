using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    public class ToggleButton : Button
    {
        #region Constants
        private const string STYLESHEET_RESOURCE = "ToggleButton";
        #endregion

        #region UXML Methods
        public new class UxmlFactory : UxmlFactory<ToggleButton, UxmlTraits> { }

        public new class UxmlTraits : Button.UxmlTraits
        {
            private readonly UxmlBoolAttributeDescription m_value = new() { name = "value", defaultValue = false };
            private readonly UxmlStringAttributeDescription m_icon = new() { name = "icon", defaultValue = string.Empty };

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(visualElement, attributes, context);
                ToggleButton element = visualElement as ToggleButton;
                element.Value = m_value.GetValueFromBag(attributes, context);
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
        public static ToggleButtonChangeEventHandler OnChange = null;

        #region Private Variables
        private bool m_value;
        private string m_iconResource;
        private readonly VisualElement m_icon;
        #endregion

        #region Accessors
        public bool Value
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
        #endregion

        #region Constructors
        public ToggleButton() : base()
        {
            m_icon = new()
            {
                name = "icon",
                focusable = false,
                pickingMode = PickingMode.Ignore
            };
            Add(m_icon);

            clicked += () =>
            {
                Value = !Value;
            };
            
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }
        #endregion

        public void SetValueWithoutNotify(bool value)
        {
            m_value = value;
            if (m_value)
            {
                AddToClassList("checked");
            }
            else
            {
                RemoveFromClassList("checked");
            }
        }
    }
}
