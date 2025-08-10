using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    #region Structs
    /// <summary>
    /// Contains information about a user's response to a Query dialog, including the selected response text, its index, and all possible responses.
    /// </summary>
    public struct QueryEvent
    {
        /// <summary>
        /// The text of the response chosen by the user.
        /// </summary>
        public string chosenResponseText;

        /// <summary>
        /// The index of the chosen response in the list of all responses.
        /// </summary>
        public int chosenResponseIndex;

        /// <summary>
        /// An array containing all possible response texts presented to the user.
        /// </summary>
        public string[] allResponses;
    }
    #endregion

    [UxmlElement]
    /// <summary>
    /// A custom UIElements VisualElement that displays a query dialog with a header, text, and configurable response buttons.
    /// Supports callbacks for response selection and navigation.
    /// </summary>
    public partial class Query : VisualElement
    {
        public delegate void ChangeSelectedButtonHandler(SelectableButton newSelectedButton);
        public ChangeSelectedButtonHandler OnChangeSelectedButton = null;

        private const string STYLESHEET_RESOURCE = "YontalaneQuery";
        public const string DEFAULT_RESPONSE = "OK";
        public const string CANCEL = "Cancel";

        private readonly VisualElement m_frame;
        private readonly Label m_headerLabel;
        private readonly Label m_textLabel;
        private readonly VisualElement m_responseContainer;
        private readonly List<SelectableButton> m_responses = new();
        private Action<QueryEvent> m_callback;
        private Action<QueryEvent> m_onNavigate;

        /// <summary>
        /// Gets or sets the header text displayed at the top of the query dialog.
        /// </summary>
        [UxmlAttribute]
        public string Header
        {
            get => m_headerLabel.text;
            set => m_headerLabel.text = value;
        }

        /// <summary>
        /// Gets or sets the main body text displayed in the query dialog.
        /// </summary>
        [UxmlAttribute]
        public string Text
        {
            get => m_textLabel.text;
            set
            {
                m_textLabel.text = value;
                m_textLabel.style.display = !string.IsNullOrEmpty(value) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// Gets or sets the label of the last response button.
        /// If there are no responses, returns or sets the default response label.
        /// </summary>
        [UxmlAttribute]
        public string ButtonLabel
        {
            get
            {
                // If there are no responses, set and return the default response label.
                if (Responses.Length == 0)
                {
                    ButtonLabel = DEFAULT_RESPONSE;
                    return DEFAULT_RESPONSE;
                }
                // Otherwise, return the label of the last response button.
                else
                {
                    return Responses[^1];
                }
            }

            set
            {
                // If there are existing responses, update the label of the last response button.
                string[] responses = Responses;
                if (responses.Length > 0)
                {
                    responses[^1] = value;
                    Responses = responses;
                }
                // If there are no responses, create a new array with the provided value as the only response.
                else
                {
                    Responses = new string[] { value };
                }
            }
        }

        /// <summary>
        /// Gets or sets the array of response button labels for the query dialog.
        /// When set, updates the response buttons and their navigation/cancel behavior.
        /// </summary>
        public string[] Responses
        {
            get
            {
                // Create a string array to hold the text of each response button
                string[] values = new string[m_responses.Count];
                // Populate the array with the text from each button in m_responses
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = m_responses[i].text;
                }
                // Return the array of response button texts
                return values;
            }

            set
            {
                // Clear the current list of response buttons and the response container in the UI.
                m_responses.Clear();
                m_responseContainer.Clear();

                // Create a new Button for each response label in the provided value array.
                for (int i = 0; i < value.Length; i++)
                {
                    int index = i;
                    m_responses.Add(new SelectableButton());
                    m_responses[^1].text = value[index];
                    // Register the click event to call OnRespond with the button's index.
                    m_responses[^1].clicked += () => OnRespond(index);
                    m_responses[^1].focusable = true;
                    m_responseContainer.Add(m_responses[^1]);
                }

                // Register navigation and cancel event handlers for each response button.
                foreach(SelectableButton button in m_responses)
                {
                    RegisterNavigation(button);
                    button.RegisterCallback((NavigationCancelEvent e) =>
                    {
                        if (CanCancel)
                        {
                            OnRespond(-1);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// The response buttons.
        /// </summary>
        public List<SelectableButton> ResponseButtons => m_responses;

        /// <summary>
        /// Sets the callback to be invoked when a response is chosen.
        /// </summary>
        /// <param name="callback">The callback action to invoke with the query event.</param>
        public void SetCallback(Action<QueryEvent> callback) => m_callback = callback;

        /// <summary>
        /// Sets the callback to be invoked when a navigation event occurs.
        /// </summary>
        /// <param name="onNavigate">The callback action to invoke on navigation.</param>
        public void SetOnNavigate(Action<QueryEvent> onNavigate) => m_onNavigate = onNavigate;

        /// <summary>
        /// Gets or sets a value indicating whether the query can be cancelled (e.g., via Escape key).
        /// </summary>
        public bool CanCancel { get; set; } = false;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with the specified header, body text, response options, and callback.
        /// </summary>
        /// <param name="header">The header text displayed at the top of the query dialog.</param>
        /// <param name="text">The main body text of the query dialog.</param>
        /// <param name="responses">A list of response button labels to display as options.</param>
        /// <param name="callback">The callback action to invoke when a response is selected.</param>
        public Query(string header, string text, IReadOnlyList<string> responses, Action<QueryEvent> callback)
        {
            // Create the main frame container for the query dialog.
            m_frame = new VisualElement() { name = "yontalane-query-frame" };

            // Create the header label for displaying the query's title.
            m_headerLabel = new Label() { name = "yontalane-query-header" };

            // Create the body label for displaying the main text of the query.
            m_textLabel = new Label() { name = "yontalane-query-body" };

            // Create the container for the response buttons.
            m_responseContainer = new VisualElement() { name = "yontalane-query-responses" };

            // Prepare an array to hold the response strings (not used directly here, but could be for future logic).
            string[] responsesArray = new string[responses.Count];

            // Set the header, text, and responses for the query dialog.
            Header = header;
            Text = text;
            Responses = responses.ToArray();

            // Set the callback to be invoked when a response is selected.
            SetCallback(callback);

            // Set focusable properties to false for the query and its main elements.
            focusable = false;
            m_frame.focusable = false;
            m_headerLabel.focusable = false;
            m_textLabel.focusable = false;
            m_responseContainer.focusable = false;

            // Add the header, text, and response container to the main frame.
            m_frame.Add(m_headerLabel);
            m_frame.Add(m_textLabel);
            m_frame.Add(m_responseContainer);

            // Add the main frame to the root of this VisualElement.
            Add(m_frame);

            // Give the query focus.
            Focus();

            // Listen for selectable button events.
            SelectableButton.OnButtonEvent += (SelectableButtonEventInfo eventInfo) =>
            {
                // If the event is of type focus-in and the button belongs to this query, then invoke the change button delegate.
                if (eventInfo.type == SelectableButtonEventType.FocusIn && eventInfo.hasFocus && eventInfo.target != null && ResponseButtons.Contains(eventInfo.target))
                {
                    OnChangeSelectedButton?.Invoke(eventInfo.target);
                }
            };

            // Add the stylesheet for styling the query dialog.
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with a header, a list of responses, and a callback.
        /// </summary>
        /// <param name="header">The header text displayed at the top of the query dialog.</param>
        /// <param name="responses">A list of response button labels to display as options.</param>
        /// <param name="callback">The callback action to invoke when a response is selected.</param>
        public Query(string header, IReadOnlyList<string> responses, Action<QueryEvent> callback)
            : this(header, string.Empty, responses, callback) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with a header, body text, a single response, and a callback.
        /// </summary>
        /// <param name="header">The header text displayed at the top of the query dialog.</param>
        /// <param name="text">The main body text of the query dialog.</param>
        /// <param name="response">The response button label to display as the only option.</param>
        /// <param name="callback">The callback action to invoke when the response is selected.</param>
        public Query(string header, string text, string response, Action<QueryEvent> callback)
            : this(header, text, new string[] { response }, callback) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with a header, a single response, and a callback.
        /// </summary>
        /// <param name="header">The header text displayed at the top of the query dialog.</param>
        /// <param name="response">The response button label to display as the only option.</param>
        /// <param name="callback">The callback action to invoke when the response is selected.</param>
        public Query(string header, string response, Action<QueryEvent> callback)
            : this(header, string.Empty, response, callback) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with a header and a callback, using the default response.
        /// </summary>
        /// <param name="header">The header text displayed at the top of the query dialog.</param>
        /// <param name="callback">The callback action to invoke when the response is selected.</param>
        public Query(string header, Action<QueryEvent> callback)
            : this(header, DEFAULT_RESPONSE, callback) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with default values.
        /// </summary>
        public Query()
            : this(string.Empty, DEFAULT_RESPONSE, null) { }
        #endregion

        /// <summary>
        /// Handles the user's response selection by invoking the callback with the chosen response index and text,
        /// then removes the query dialog from the UI.
        /// </summary>
        /// <param name="chosenResponseIndex">The index of the response selected by the user.</param>
        private void OnRespond(int chosenResponseIndex)
        {
            string[] responses = Responses;

            m_callback?.Invoke(new QueryEvent()
            {
                chosenResponseIndex = chosenResponseIndex,
                chosenResponseText = chosenResponseIndex >= 0 ? responses[chosenResponseIndex] : CANCEL,
                allResponses = responses
            });

            RemoveFromHierarchy();
        }

        /// <summary>
        /// Sets focus to the Query dialog, ensuring the first response button is focused for user interaction.
        /// </summary>
        public new void Focus()
        {
            UIDocument document = UnityEngine.Object.FindAnyObjectByType<UIDocument>();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(document.gameObject);
            }

            if (m_responses.Count > 0)
            {
                m_responses[0].Focus();
            }
        }

        /// <summary>
        /// Registers navigation event handlers for the specified response button, enabling keyboard navigation
        /// (up/down/left/right) between response buttons in the query dialog. When a navigation key is pressed,
        /// focus moves to the appropriate button and the navigation callback is invoked.
        /// </summary>
        /// <param name="button">The response button to register navigation for.</param>
        private void RegisterNavigation(SelectableButton button)
        {
            // Get the index of the button in the list of response buttons.
            int index = m_responses.IndexOf(button);

            // If the button is not found in the list, exit early.
            if (index == -1)
            {
                return;
            }

            // Register a callback to handle navigation move events for this button.
            button.RegisterCallback((NavigationMoveEvent e) =>
            {
                // Handle navigation based on the direction of the event.
                switch (e.direction)
                {
                    // If navigating up or left, move focus to the previous button (wrap around if at the start).
                    case NavigationMoveEvent.Direction.Up:
                    case NavigationMoveEvent.Direction.Left:
                        {
                            int newIndex = index > 0 ? index - 1 : m_responses.Count - 1;
                            m_responses[newIndex].Focus();

                            // Invoke the navigation callback with the new selection.
                            string[] responses = Responses;
                            m_onNavigate?.Invoke(new QueryEvent()
                            {
                                chosenResponseIndex = newIndex,
                                chosenResponseText = responses[newIndex],
                                allResponses = responses
                            });
                        }
                        break;
                    // If navigating down or right, move focus to the next button (wrap around if at the end).
                    case NavigationMoveEvent.Direction.Down:
                    case NavigationMoveEvent.Direction.Right:
                        {
                            int newIndex = index < m_responses.Count - 1 ? index + 1 : 0;
                            m_responses[newIndex].Focus();

                            // Invoke the navigation callback with the new selection.
                            string[] responses = Responses;
                            m_onNavigate?.Invoke(new QueryEvent()
                            {
                                chosenResponseIndex = newIndex,
                                chosenResponseText = responses[newIndex],
                                allResponses = responses
                            });
                        }
                        break;
                }

                // Stop further propagation of the event and tell the focus controller to ignore it.
                e.StopPropagation();
                focusController.IgnoreEvent(e);
            });
        }
    }
}
