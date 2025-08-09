using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    /// <summary>
    /// Contains information about a SelectableButton event, including the target button, event type, focus state, and button state.
    /// </summary>
    public struct SelectableButtonEventInfo
    {
        /// <summary>
        /// The SelectableButton that triggered the event.
        /// </summary>
        public SelectableButton target;

        /// <summary>
        /// The type of event that occurred.
        /// </summary>
        public SelectableButtonEventType type;

        /// <summary>
        /// Indicates whether the button currently has focus.
        /// </summary>
        public bool hasFocus;

        /// <summary>
        /// The current state of the button (Normal, Hover, Active).
        /// </summary>
        public ButtonState state;
    }

    /// <summary>
    /// Enumerates the types of events that a SelectableButton can trigger.
    /// </summary>
    public enum SelectableButtonEventType
    {
        /// <summary>No event.</summary>
        None = 0,
        /// <summary>The button has received focus.</summary>
        FocusIn = 10,
        /// <summary>The button has lost focus.</summary>
        FocusOut = 11,
        /// <summary>The pointer has entered the button area.</summary>
        PointerEnter = 20,
        /// <summary>The pointer has exited the button area.</summary>
        PointerExit = 21,
        /// <summary>The pointer is pressed down on the button.</summary>
        PointerDown = 30,
        /// <summary>The pointer is released from the button.</summary>
        PointerUp = 31,
        /// <summary>The pointer interaction was canceled.</summary>
        Cancel = 40,
    }

    /// <summary>
    /// Represents the visual state of a SelectableButton.
    /// </summary>
    public enum ButtonState
    {
        /// <summary>The button is in its normal state.</summary>
        Normal = 0,
        /// <summary>The button is being hovered over.</summary>
        Hover = 1,
        /// <summary>The button is in its active (pressed) state.</summary>
        Active = 2,
    }

    /// <summary>
    /// A custom UIElements button that supports selectable states (Normal, Hover, Active) and exposes events for focus and pointer interactions.
    /// </summary>
    [UxmlElement]
    public partial class SelectableButton : Button
    {
        /// <summary>
        /// Delegate for handling SelectableButton events.
        /// </summary>
        /// <param name="info">Information about the button event.</param>
        public delegate void SelectableButtonEventHandler(SelectableButtonEventInfo info);

        /// <summary>
        /// Static event invoked when a SelectableButton event occurs.
        /// </summary>
        public static SelectableButtonEventHandler OnButtonEvent = null;

        private const string STYLESHEET_RESOURCE = "YontalaneSelectableButton";

        private bool m_hasFocus;
        private ButtonState m_state;
        private bool m_pointerIsIn;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableButton"/> class, 
        /// setting up styles, state, and event callbacks for focus and pointer interactions.
        /// </summary>
        public SelectableButton()
        {
            // Add the custom stylesheet to the button.
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
            AddToClassList("yontalane-selectable-button");

            // Initialize internal state variables.
            m_hasFocus = false;
            m_state = ButtonState.Normal;
            m_pointerIsIn = false;

            // Register event callbacks for focus and pointer interactions.
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerOutEvent>(OnPointerOut);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        }

        private void OnFocusIn(FocusInEvent _)
        {
            m_hasFocus = true;
            DoButtonEvent(SelectableButtonEventType.FocusIn);
        }

        private void OnFocusOut(FocusOutEvent _)
        {
            m_hasFocus = false;
            DoButtonEvent(SelectableButtonEventType.FocusOut);
        }

        private void OnPointerEnter(PointerEnterEvent _)
        {
            m_pointerIsIn = true;
            m_state = m_state != ButtonState.Active ? ButtonState.Hover : ButtonState.Active;
            DoButtonEvent(SelectableButtonEventType.PointerEnter);
        }

        private void OnPointerOut(PointerOutEvent _)
        {
            m_pointerIsIn = false;
            m_state = m_state != ButtonState.Active ? ButtonState.Normal : ButtonState.Active;
            DoButtonEvent(SelectableButtonEventType.PointerExit);
        }

        private void OnPointerCancel(PointerCancelEvent _)
        {
            m_pointerIsIn = false;
            m_state = ButtonState.Normal;
            DoButtonEvent(SelectableButtonEventType.Cancel);
        }

        private void OnPointerDown(PointerDownEvent _)
        {
            m_state = m_pointerIsIn ? ButtonState.Active : ButtonState.Normal;
            DoButtonEvent(SelectableButtonEventType.PointerDown);
        }

        private void OnPointerUp(PointerUpEvent _)
        {
            m_state = m_pointerIsIn ? ButtonState.Hover : ButtonState.Normal;
            DoButtonEvent(SelectableButtonEventType.PointerUp);
        }

        private void DoButtonEvent(SelectableButtonEventType eventType) => OnButtonEvent?.Invoke(new()
        {
            target = this,
            type = eventType,
            hasFocus = m_hasFocus,
            state = m_state,
        });
    }
}
