using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    public class Notification : VisualElement
    {
        #region Constants
        private const string STYLESHEET_RESOURCE = "Notification";
        private const string SOLO_HEADER_CLASS = "solo";
        private const string SHOWN_HEADER_CLASS = "shown";
        private const int FRAME_TOP = 15;
        private const int FRAME_HEIGHT = 85;
        #endregion

        #region UXML Methods
        public new class UxmlFactory : UxmlFactory<Notification, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_header = new UxmlStringAttributeDescription { name = "header", defaultValue = "Header" };
            UxmlStringAttributeDescription m_text = new UxmlStringAttributeDescription { name = "text", defaultValue = "Text" };

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(visualElement, attributes, context);
                Notification notification = visualElement as Notification;
                notification.Header = m_header.GetValueFromBag(attributes, context);
                notification.Text = m_text.GetValueFromBag(attributes, context);
            }
        }
        #endregion

        #region Private Variables
        private int m_slot;
        private Label m_header;
        private Label m_text;
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

        public float Duration { get; set; } = 4f;
        #endregion

        #region Constructors
        public Notification(UIDocument document, string header, string text)
        {
            m_header = new Label() { name = "yontalane-notification-header" };
            m_text = new Label() { name = "yontalane-notification-body" };

            Header = header;
            Text = text;

            focusable = false;
            m_header.focusable = false;
            m_text.focusable = false;

            Add(m_header);
            Add(m_text);

            m_slot = -1;
            List<int> slots = new();
            if (document != null)
            {
                document.rootVisualElement.Query<Notification>().ForEach((Notification item) => slots.Add(item.m_slot));
            }
            slots.Sort();
            for (int i = 0; i < slots.Count; i++)
            {
                if ((i == 0 && slots[i] > 0) || (i > 0 && slots[i] > slots[i - 1] + 1))
                {
                    m_slot = i == 0 ? 0 : slots[i - 1] + 1;
                    slots.Insert(i, m_slot);
                    break;
                }
            }
            m_slot = m_slot == -1 ? slots.Count : m_slot;
            style.top = new StyleLength(new Length(FRAME_TOP + m_slot * FRAME_HEIGHT, LengthUnit.Pixel));

            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));

            if (document != null)
            {
                document.StartCoroutine(DelayedShow());
            }
            else
            {
                AddToClassList(SHOWN_HEADER_CLASS);
            }
        }

        public Notification(string header, string text) : this(null, header, text) { }

        public Notification(UIDocument document, string header) : this(document, header, string.Empty) { }

        public Notification(string header) : this(null, header, string.Empty) { }

        public Notification() : this("Notification") { }
        #endregion

        private IEnumerator DelayedShow()
        {
            yield return new WaitForEndOfFrame();
            AddToClassList(SHOWN_HEADER_CLASS);
            yield return new WaitForSeconds(Duration);
            RemoveFromClassList(SHOWN_HEADER_CLASS);
            yield return new WaitForSeconds(1f);
            Remove();
        }

        public void Remove()
        {
            parent.Remove(this);
        }
    }
}
