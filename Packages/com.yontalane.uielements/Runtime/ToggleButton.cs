using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    /// <summary>
    /// A toggleable button UI element that extends <see cref="IconButton"/>, allowing users to switch between checked and unchecked states.
    /// </summary>
    [UxmlElement]
    public partial class ToggleButton : IconButton
    {
        private const string STYLESHEET_RESOURCE = "YontalaneToggleButton";
        public const string CHECKED_STYLE_CLASS = "checked";

        /// <summary>
        /// Represents the data for a change event on a <see cref="ToggleButton"/>, 
        /// including the previous value, the new value, and the button instance that triggered the event.
        /// </summary>
        public struct ToggleButtonChangeEvent
        {
            /// <summary>
            /// The value of the toggle before the change occurred.
            /// </summary>
            public bool oldValue;

            /// <summary>
            /// The value of the toggle after the change occurred.
            /// </summary>
            public bool newValue;

            /// <summary>
            /// The <see cref="ToggleButton"/> that triggered the change event.
            /// </summary>
            public ToggleButton target;
        }

        public delegate void ToggleButtonChangeEventHandler(ToggleButtonChangeEvent e);
        /// <summary>
        /// Event invoked when the value of the toggle button changes.
        /// </summary>
        public ToggleButtonChangeEventHandler OnChange = null;

        private bool m_value;

        /// <summary>
        /// Gets or sets the current checked state of the toggle button.
        /// Setting this property updates the visual state and triggers the <see cref="OnChange"/> event.
        /// </summary>
        [UxmlAttribute]
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleButton"/> class,
        /// setting up the toggle button's style, click behavior, and stylesheet.
        /// </summary>
        public ToggleButton() : base()
        {
            AddToClassList("yontalane-toggle-button");
            clicked += () =>
            {
                Value = !Value;
            };
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }

        /// <summary>
        /// Sets the value of the toggle button without invoking the <see cref="OnChange"/> event.
        /// Updates the visual state to reflect the new value.
        /// </summary>
        /// <param name="value">The new value to set for the toggle button.</param>
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
