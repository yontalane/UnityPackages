using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    #region Structs
    public struct QueryEvent
    {
        public string chosenResponseText;
        public int chosenResponseIndex;
        public string[] allResponses;
    }
    #endregion

    public class Query : VisualElement
    {
        #region Constants
        public const string DEFAULT_RESPONSE = "OK";
        private const string STYLESHEET_RESOURCE = "Query";
        public const string CANCEL = "Cancel";
        #endregion

        #region UXML Methods
        public new class UxmlFactory : UxmlFactory<Query, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_header = new UxmlStringAttributeDescription { name = "header", defaultValue = "Header" };
            UxmlStringAttributeDescription m_text = new UxmlStringAttributeDescription { name = "text", defaultValue = "Text" };
            UxmlStringAttributeDescription m_buttonLabel = new UxmlStringAttributeDescription { name = "button-label", defaultValue = Query.DEFAULT_RESPONSE };

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(visualElement, attributes, context);
                Query query = visualElement as Query;
                query.Header = m_header.GetValueFromBag(attributes, context);
                query.Text = m_text.GetValueFromBag(attributes, context);
                query.ButtonLabel = m_buttonLabel.GetValueFromBag(attributes, context);
            }
        }
        #endregion

        #region Private Variables
        private readonly VisualElement m_frame;
        private readonly Label m_header;
        private readonly Label m_text;
        private readonly VisualElement m_responseContainer;
        private readonly List<Button> m_responses = new();
        private Action<QueryEvent> m_callback;
        private Action<QueryEvent> m_onNavigate;
        #endregion

        #region Accessors
        public string Header
        {
            get => m_header.text;
            set => m_header.text = value;
        }

        public string Text
        {
            get => m_text.text;
            set
            {
                m_text.text = value;
                m_text.style.display = !string.IsNullOrEmpty(value) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public string[] Responses
        {
            get
            {
                string[] values = new string[m_responses.Count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = m_responses[i].text;
                }
                return values;
            }

            set
            {
                m_responses.Clear();
                m_responseContainer.Clear();

                for (int i = 0; i < value.Length; i++)
                {
                    int index = i;
                    m_responses.Add(new Button());
                    m_responses[^1].text = value[index];
                    m_responses[^1].clicked += () => OnRespond(index);
                    m_responses[^1].focusable = true;
                    m_responseContainer.Add(m_responses[^1]);
                }

                foreach(Button button in m_responses)
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

        public string ButtonLabel
        {
            get
            {
                string[] responses = Responses;
                if (responses.Length == 0)
                {
                    ButtonLabel = DEFAULT_RESPONSE;
                    return DEFAULT_RESPONSE;
                }
                else
                {
                    return responses[responses.Length - 1];
                }
            }

            set
            {
                string[] responses = Responses;
                if (responses.Length > 0)
                {
                    responses[^1] = value;
                    Responses = responses;
                }
                else
                {
                    Responses = new string[] { value };
                }
            }
        }

        public void SetCallback(Action<QueryEvent> callback) => m_callback = callback;

        public void SetOnNavigate(Action<QueryEvent> onNavigate) => m_onNavigate = onNavigate;

        public bool CanCancel { get; set; } = false;
        #endregion

        #region Constructors
        public Query(string header, string text, IReadOnlyList<string> responses, Action<QueryEvent> callback)
        {
            m_frame = new VisualElement() { name = "yontalane-query-frame" };
            m_header = new Label() { name = "yontalane-query-header" };
            m_text = new Label() { name = "yontalane-query-body" };
            m_responseContainer = new VisualElement() { name = "yontalane-query-responses" };

            string[] responsesArray = new string[responses.Count];

            Header = header;
            Text = text;
            Responses = responses.ToArray();
            SetCallback(callback);

            focusable = false;
            m_frame.focusable = false;
            m_header.focusable = false;
            m_text.focusable = false;
            m_responseContainer.focusable = false;

            m_frame.Add(m_header);
            m_frame.Add(m_text);
            m_frame.Add(m_responseContainer);
            Add(m_frame);

            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }

        public Query(string header, IReadOnlyList<string> responses, Action<QueryEvent> callback) : this(header, string.Empty, responses, callback) { }

        public Query(string header, string text, string response, Action<QueryEvent> callback) : this(header, text, new string[] { response }, callback) { }

        public Query(string header, string response, Action<QueryEvent> callback) : this(header, string.Empty, response, callback) { }

        public Query(string header, Action<QueryEvent> callback) : this(header, DEFAULT_RESPONSE, callback) { }

        public Query() : this(string.Empty, DEFAULT_RESPONSE, null) { }
        #endregion
        
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

        private void RegisterNavigation(Button button)
        {
            int index = m_responses.IndexOf(button);

            if (index == -1)
            {
                return;
            }

            button.RegisterCallback((NavigationMoveEvent e) =>
            {
                switch (e.direction)
                {
                    case NavigationMoveEvent.Direction.Up:
                    case NavigationMoveEvent.Direction.Left:
                        {
                            int newIndex = index > 0 ? index - 1 : m_responses.Count - 1;
                            m_responses[newIndex].Focus();

                            string[] responses = Responses;
                            m_onNavigate?.Invoke(new QueryEvent()
                            {
                                chosenResponseIndex = newIndex,
                                chosenResponseText = responses[newIndex],
                                allResponses = responses
                            });
                        }
                        break;
                    case NavigationMoveEvent.Direction.Down:
                    case NavigationMoveEvent.Direction.Right:
                        {
                            int newIndex = index < m_responses.Count - 1 ? index + 1 : 0;
                            m_responses[newIndex].Focus();

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
                e.PreventDefault();
            });
        }
    }
}
