using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    [UxmlElement]
    /// <summary>
    /// A customizable button that displays an icon and optional text, inheriting selectable states and events.
    /// </summary>
    public partial class IconButton : SelectableButton
    {
        private const string STYLESHEET_RESOURCE = "YontalaneIconButton";

        private readonly VisualElement m_iconUI;
        private readonly Label m_labelUI;
        private Vector2Int m_iconSize = new(16, 16);
        private bool m_iconOnLeft = true;
        private int m_gap = 4;

        /// <summary>
        /// Gets or sets the text displayed on the button.
        /// </summary>
        [UxmlAttribute]
        public string Text
        {
            get => m_labelUI.text;
            set
            {
                m_labelUI.text = value;
                base.text = string.Empty;

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

        /// <summary>
        /// Gets or sets the size of the icon.
        /// </summary>
        [UxmlAttribute]
        public Vector2Int IconSize
        {
            get => m_iconSize;
            set
            {
                m_iconSize = value;
                RefreshIconSize();
            }
        }

        /// <summary>
        /// Gets or sets the icon sprite.
        /// </summary>
        [UxmlAttribute]
        public Sprite Icon
        {
            get => m_iconUI.style.backgroundImage.value.sprite;
            set
            {
                if (value != null)
                {
                    m_iconUI.style.backgroundImage = new(value);
                    AddToClassList("with-image");
                }
                else
                {
                    m_iconUI.style.backgroundImage = new();
                    RemoveFromClassList("with-image");
                }

                RefreshIconSize();
            }
        }

        /// <summary>
        /// Gets or sets the gap (spacing) between the icon and the text.
        /// </summary>
        [UxmlAttribute]
        public int Gap
        {
            get => m_gap;
            set
            {
                m_gap = value;
                RefreshGap();
            }
        }

        /// <summary>
        /// Gets or sets whether the icon appears on the left side of the text.
        /// </summary>
        [UxmlAttribute]
        public bool IconOnLeft
        {
            get => m_iconOnLeft;

            set
            {
                m_iconOnLeft = value;

                if (m_iconOnLeft)
                {
                    RemoveFromClassList("image-on-right");
                }
                else
                {
                    AddToClassList("image-on-right");
                }

                RefreshGap();
            }
        }

        /// <summary>
        /// Does this button have an icon?
        /// </summary>
        public bool HasIcon => ClassListContains("with-image");

        /// <summary>
        /// Does this button have text?
        /// </summary>
        public bool HasText => ClassListContains("with-text");

        /// <summary>
        /// Initializes a new instance of the <see cref="IconButton"/> class, setting up the icon and label UI elements,
        /// applying default styles, and configuring initial layout and appearance.
        /// </summary>
        public IconButton() : base()
        {
            // Add the main style class for the icon button.
            AddToClassList("yontalane-icon-button");

            // Clear the base button text, as we use a custom label.
            base.text = string.Empty;

            // Create and add the icon VisualElement.
            m_iconUI = new()
            {
                name = "icon",
                focusable = false,
                pickingMode = PickingMode.Ignore
            };
            Add(m_iconUI);

            // Create and add the label VisualElement.
            m_labelUI = new()
            {
                name = "label",
                focusable = false,
                pickingMode = PickingMode.Ignore
            };
            Add(m_labelUI);

            // Set the icon size based on the current icon size property.
            RefreshIconSize();

            // Set the icon position (left or right) and refresh the gap.
            IconOnLeft = IconOnLeft;
            RefreshGap();

            // Add the stylesheet for the icon button.
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));
        }

        /// <summary>
        /// Updates the icon UI element's size to match the current icon size settings.
        /// </summary>
        private void RefreshIconSize()
        {
            // Calculate the size to use for the icon UI element.
            Vector2 size = !m_iconUI.style.backgroundImage.value.IsEmpty() ? m_iconSize : m_iconSize;

            // Set the width and height of the icon UI element.
            m_iconUI.style.width = new(new Length(size.x, LengthUnit.Pixel));
            m_iconUI.style.height = new(new Length(size.y, LengthUnit.Pixel));

            // Set the minimum width and height of the icon UI element.
            m_iconUI.style.minWidth = new(new Length(size.x, LengthUnit.Pixel));
            m_iconUI.style.minHeight = new(new Length(size.y, LengthUnit.Pixel));

            // Set the maximum width and height of the icon UI element.
            m_iconUI.style.maxWidth = new(new Length(size.x, LengthUnit.Pixel));
            m_iconUI.style.maxHeight = new(new Length(size.y, LengthUnit.Pixel));
        }

        /// <summary>
        /// Updates the spacing (gap) between the icon and label UI elements based on the current icon position and gap value.
        /// Resets all margins to zero, then applies the gap to the appropriate side of the icon depending on whether the icon is on the left or right.
        /// </summary>
        private void RefreshGap()
        {
            // Reset all margins for the label UI element to zero.
            m_labelUI.style.marginTop = new(new Length(0f, LengthUnit.Pixel));
            m_labelUI.style.marginBottom = new(new Length(0f, LengthUnit.Pixel));
            m_labelUI.style.marginLeft = new(new Length(0f, LengthUnit.Pixel));
            m_labelUI.style.marginRight = new(new Length(0f, LengthUnit.Pixel));

            // Reset top and bottom margins for the icon UI element to zero.
            m_iconUI.style.marginTop = new(new Length(0f, LengthUnit.Pixel));
            m_iconUI.style.marginBottom = new(new Length(0f, LengthUnit.Pixel));

            // Apply the gap to the correct side of the icon depending on its position.
            if (!HasIcon || !HasText)
            {
                // There can only be a gap if there is both an icon and text.
                m_iconUI.style.marginLeft = new(new Length(0f, LengthUnit.Pixel));
                m_iconUI.style.marginRight = new(new Length(0f, LengthUnit.Pixel));
            }
            else if (IconOnLeft)
            {
                // If the icon is on the left, set right margin to the gap and left margin to zero.
                m_iconUI.style.marginLeft = new(new Length(0f, LengthUnit.Pixel));
                m_iconUI.style.marginRight = new(new Length(m_gap, LengthUnit.Pixel));
            }
            else
            {
                // If the icon is on the right, set left margin to the gap and right margin to zero.
                m_iconUI.style.marginLeft = new(new Length(m_gap, LengthUnit.Pixel));
                m_iconUI.style.marginRight = new(new Length(0f, LengthUnit.Pixel));
            }
        }
    }
}
