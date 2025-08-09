using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    [UxmlElement]
    /// <summary>
    /// A custom UIElements VisualElement for displaying notifications with a header and optional text.
    /// </summary>
    public partial class Notification : VisualElement
    {
        private const string STYLESHEET_RESOURCE = "YontalaneNotification";
        private const string SHOWN_HEADER_CLASS = "shown";
        private const int FRAME_TOP = 15;
        private const int FRAME_HEIGHT = 85;

        private readonly int m_slotNumber;
        private readonly Label m_headerLabel;
        private readonly Label m_textLabel;

        /// <summary>
        /// Gets or sets the header text displayed at the top of the notification.
        /// </summary>
        [UxmlAttribute]
        public string Header
        {
            get => m_headerLabel.text;
            set => m_headerLabel.text = value;
        }

        /// <summary>
        /// Gets or sets the main body text of the notification.
        /// Automatically hides the label if the text is null or empty.
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
        /// Gets or sets the duration (in seconds) for which the notification is displayed.
        /// </summary>
        public float Duration { get; set; } = 4f;

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class with the specified document, header, and text.
        /// Determines the notification's slot position and sets up its visual elements.
        /// </summary>
        /// <param name="document">The <see cref="UIDocument"/> to which this notification belongs, or null if standalone.</param>
        /// <param name="header">The header text to display at the top of the notification.</param>
        /// <param name="text">The main body text of the notification.</param>
        public Notification(UIDocument document, string header, string text)
        {
            // Create and configure the header and body labels for the notification.
            m_headerLabel = new Label() { name = "yontalane-notification-header" };
            m_textLabel = new Label() { name = "yontalane-notification-body" };

            // Set the header and text content.
            Header = header;
            Text = text;

            // Set focusable properties to false for the notification and its labels.
            focusable = false;
            m_headerLabel.focusable = false;
            m_textLabel.focusable = false;

            // Add the header and body labels to the notification visual element.
            Add(m_headerLabel);
            Add(m_textLabel);

            // Determine the slot number for this notification to avoid overlap with others.
            m_slotNumber = -1;
            List<int> slots = new();
            if (document != null)
            {
                // Collect slot numbers of existing notifications in the document.
                document.rootVisualElement.Query<Notification>().ForEach((Notification item) => slots.Add(item.m_slotNumber));
            }
            // Sort the slot numbers to find the first available slot.
            slots.Sort();
            for (int i = 0; i < slots.Count; i++)
            {
                // Find the first gap in the slot sequence or use the next available slot.
                if ((i == 0 && slots[i] > 0) || (i > 0 && slots[i] > slots[i - 1] + 1))
                {
                    m_slotNumber = i == 0 ? 0 : slots[i - 1] + 1;
                    slots.Insert(i, m_slotNumber);
                    break;
                }
            }
            // If no gap was found, use the next available slot at the end.
            m_slotNumber = m_slotNumber == -1 ? slots.Count : m_slotNumber;
            // Position the notification based on its slot number.
            style.top = new StyleLength(new Length(FRAME_TOP + m_slotNumber * FRAME_HEIGHT, LengthUnit.Pixel));

            // Add the notification stylesheet for styling.
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));

            // Show the notification with animation if a document is provided, otherwise show immediately.
            if (document != null)
            {
                document.StartCoroutine(DelayedShow());
            }
            else
            {
                AddToClassList(SHOWN_HEADER_CLASS);
            }
        }

        /// <summary>
        /// Creates a Notification with the specified header and body text, without attaching it to a UIDocument.
        /// </summary>
        /// <param name="header">The header text of the notification.</param>
        /// <param name="text">The body text of the notification.</param>
        public Notification(string header, string text) : this(null, header, text) { }

        /// <summary>
        /// Creates a Notification with a specified UIDocument and header, with an empty body text.
        /// </summary>
        /// <param name="document">The UIDocument to attach the notification to.</param>
        /// <param name="header">The header text of the notification.</param>
        public Notification(UIDocument document, string header) : this(document, header, string.Empty) { }

        /// <summary>
        /// Creates a Notification with a specified header and no document or body text.
        /// </summary>
        /// <param name="header">The header text of the notification.</param>
        public Notification(string header) : this(null, header, string.Empty) { }

        /// <summary>
        /// Creates a Notification with a default header ("Notification") and no document or body text.
        /// </summary>
        public Notification() : this("Notification") { }

        private IEnumerator DelayedShow()
        {
            yield return new WaitForEndOfFrame();
            AddToClassList(SHOWN_HEADER_CLASS);
            yield return new WaitForSeconds(Duration);
            RemoveFromClassList(SHOWN_HEADER_CLASS);
            yield return new WaitForSeconds(1f);
            Remove();
        }

        /// <summary>
        /// Removes this notification from its parent visual element, effectively dismissing it from the UI.
        /// </summary>
        public void Remove()
        {
            parent.Remove(this);
        }
    }
}
