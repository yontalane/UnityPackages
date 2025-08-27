using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Yontalane.UIElements;

namespace Yontalane.DialogUIElements
{
    #region Data Structures

    /// <summary>
    /// Specifies the types of dialog buttons available in the dialog UI.
    /// </summary>
    public enum DialogButtonType
    {
        Continue = 0,
        Skip = 10,
        Response = 20,
        Close = 30,
    }

    /// <summary>
    /// Represents an event triggered by dialog interactions, containing information about the dialog pane,
    /// the type of button pressed, the dialog pane's name, speaker, dialog text, response text, and response index.
    /// </summary>
    public struct DialogEvent
    {
        /// <summary>
        /// Reference to the DialogPane instance that triggered the event.
        /// </summary>
        public DialogPane dialogPane;

        /// <summary>
        /// The type of button that was pressed (e.g., Continue, Skip, Response, Close).
        /// </summary>
        public DialogButtonType buttonType;

        /// <summary>
        /// The name of the dialog pane, useful for identifying which dialog is active.
        /// </summary>
        public string dialogPaneName;

        /// <summary>
        /// The name of the speaker currently associated with the dialog.
        /// </summary>
        public string speaker;

        /// <summary>
        /// The main dialog text being displayed.
        /// </summary>
        public string dialogText;

        /// <summary>
        /// The text of the selected response, if applicable.
        /// </summary>
        public string responseText;

        /// <summary>
        /// The index of the selected response, if applicable.
        /// </summary>
        public int responseIndex;
    }

    #endregion

    /// <summary>
    /// A custom VisualElement for displaying dialog UI, including speaker, text, and interactive dialog buttons.
    /// </summary>
    [UxmlElement]
    public partial class DialogPane : VisualElement
    {
        #region Events

        public delegate void DialogEventHandler(DialogEvent dialogEvent);

        /// <summary>
        /// Static event triggered when the "Continue" dialog button is clicked on any DialogPane.
        /// </summary>
        public static DialogEventHandler OnClickDialogContinue = null;

        /// <summary>
        /// Static event triggered when the "Continue" button is selected on any DialogPane.
        /// </summary>
        public static DialogEventHandler OnHighlightDialogContinue = null;

        /// <summary>
        /// Static event triggered when the "Skip" dialog button is clicked on any DialogPane.
        /// </summary>
        public static DialogEventHandler OnClickDialogSkip = null;

        /// <summary>
        /// Static event triggered when a response button is clicked on any DialogPane.
        /// </summary>
        public static DialogEventHandler OnClickDialogResponse = null;

        /// <summary>
        /// Static event triggered when a response button is selected on any DialogPane.
        /// </summary>
        public static DialogEventHandler OnHighlightDialogResponse = null;

        /// <summary>
        /// Static event triggered when the "Close" dialog button is clicked on any DialogPane.
        /// </summary>
        public static DialogEventHandler OnClickDialogClose = null;

        /// <summary>
        /// Static event triggered when a close button is selected on any DialogPane.
        /// </summary>
        public static DialogEventHandler OnHighlightDialogClose = null;

        /// <summary>
        /// Instance event triggered when the "Continue" dialog button is clicked on this DialogPane.
        /// </summary>
        public DialogEventHandler OnClickContinue = null;

        /// <summary>
        /// Instance event triggered when the "Continue" button is selected on this DialogPane.
        /// </summary>
        public static DialogEventHandler OnHighlightContinue = null;

        /// <summary>
        /// Instance event triggered when the "Skip" dialog button is clicked on this DialogPane.
        /// </summary>
        public DialogEventHandler OnClickSkip = null;

        /// <summary>
        /// Instance event triggered when a response button is clicked on this DialogPane.
        /// </summary>
        public DialogEventHandler OnClickResponse = null;

        /// <summary>
        /// Instance event triggered when a response button is selected on this DialogPane.
        /// </summary>
        public DialogEventHandler OnHighlightResponse = null;

        /// <summary>
        /// Instance event triggered when the "Close" dialog button is clicked on this DialogPane.
        /// </summary>
        public DialogEventHandler OnClickClose = null;

        /// <summary>
        /// Instance event triggered when the close button is selected on this DialogPane.
        /// </summary>
        public DialogEventHandler OnHighlightClose = null;

        #endregion

        private const string STYLESHEET_RESOURCE = "DialogPane";

        #region Private Fields

        private readonly List<IconButton> m_responseButtons = new();
        private readonly VisualElement m_frame;
        private readonly VisualElement m_portraitField;
        private readonly Label m_speakerField;
        private readonly Label m_textField;
        private readonly IconButton m_continueButton;
        private readonly IconButton m_skipButton;
        private readonly IconButton m_closeButton;
        private readonly VisualElement m_responseContainer;
        private string m_fullText;
        private int m_maxVisibleCharacters = int.MaxValue;
        private string m_speakerText;
        private string m_speakerTextAfter = ": ";
        private bool m_speakerIsInline = false;
        private bool m_speakerIsBold = true;
        private bool m_speakerIsCapitalized = false;
        private Color m_speakerColor = Color.yellow;
        private string m_visibilityStyleClass = string.Empty;

        #endregion

        #region Uxml Attributes

        /// <summary>
        /// The name of the speaker to display in the dialog.
        /// </summary>
        [Tooltip("The name of the speaker to display in the dialog.")]
        [UxmlAttribute]
        public string Speaker
        {
            get => m_speakerText;
            set
            {
                m_speakerText = value;
                RefreshTextAndSpeakerFields();
            }
        }

        /// <summary>
        /// The text to display after the speaker's name (e.g., ": ").
        /// </summary>
        [Tooltip("The text to display after the speaker's name (e.g., ': ').")]
        [UxmlAttribute]
        public string SpeakerTextAfter
        {
            get => m_speakerTextAfter;
            set
            {
                m_speakerTextAfter = value;
                RefreshTextAndSpeakerFields();
            }
        }

        /// <summary>
        /// Whether the speaker's name should be displayed inline with the dialog text.
        /// </summary>
        [Tooltip("Whether the speaker's name should be displayed inline with the dialog text.")]
        [UxmlAttribute]
        public bool SpeakerIsInline
        {
            get => m_speakerIsInline;
            set
            {
                m_speakerIsInline = value;
                RefreshTextAndSpeakerFields();
            }
        }

        /// <summary>
        /// Whether the speaker's name should be displayed in bold.
        /// </summary>
        [Tooltip("Whether the speaker's name should be displayed in bold.")]
        [UxmlAttribute]
        public bool SpeakerIsBold
        {
            get => m_speakerIsBold;
            set
            {
                m_speakerIsBold = value;
                RefreshTextAndSpeakerFields();
            }
        }

        /// <summary>
        /// Whether the speaker's name should be capitalized.
        /// </summary>
        [Tooltip("Whether the speaker's name should be capitalized.")]
        [UxmlAttribute]
        public bool SpeakerIsCapitalized
        {
            get => m_speakerIsCapitalized;
            set
            {
                m_speakerIsCapitalized = value;
                RefreshTextAndSpeakerFields();
            }
        }

        /// <summary>
        /// The color to use for the speaker's name.
        /// </summary>
        [Tooltip("The color to use for the speaker's name.")]
        [UxmlAttribute]
        public Color SpeakerColor
        {
            get => m_speakerColor;
            set
            {
                m_speakerColor = value;
                RefreshTextAndSpeakerFields();
            }
        }

        /// <summary>
        /// The main dialog text to display.
        /// </summary>
        [Tooltip("The main dialog text to display.")]
        [UxmlAttribute]
        public string Text
        {
            get => m_fullText;
            set
            {
                m_fullText = value;
                RefreshTextAndSpeakerFields();
            }
        }

        /// <summary>
        /// The label text for the continue button.
        /// </summary>
        [Tooltip("The label text for the continue button.")]
        [UxmlAttribute]
        public string ContinueButtonLabel
        {
            get => m_continueButton.Text;
            set
            {
                m_continueButton.Text = value;
            }
        }

        /// <summary>
        /// The number of characters of the dialog text to display (for typewriter effect).
        /// </summary>
        [Tooltip("The number of characters of the dialog text to display (for typewriter effect).")]
        [UxmlAttribute]
        public int MaxVisibleCharacters
        {
            get => m_maxVisibleCharacters;
            set
            {
                m_maxVisibleCharacters = Mathf.Clamp(value, 0, !string.IsNullOrEmpty(m_fullText) ? m_fullText.Length : int.MaxValue);
                RefreshTextAndSpeakerFields();
            }
        }

        /// <summary>
        /// The portrait image to display in the dialog.
        /// </summary>
        [Tooltip("The portrait image to display in the dialog.")]
        [UxmlAttribute]
        public Sprite Portrait
        {
            get => m_portraitField.style.backgroundImage.value.sprite;
            set => m_portraitField.style.backgroundImage = value != null ? new(value) : new();
        }

        /// <summary>
        /// Whether the portrait image is visible.
        /// </summary>
        [Tooltip("Whether the portrait image is visible.")]
        [UxmlAttribute]
        public bool PortraitVisible
        {
            get => m_portraitField.style.display == DisplayStyle.Flex;
            set => m_portraitField.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Whether the skip button is visible and active.
        /// </summary>
        [Tooltip("Whether the skip button is visible and active.")]
        [UxmlAttribute]
        public bool SkipButtonActive
        {
            get => m_skipButton.style.display == DisplayStyle.Flex;
            set => m_skipButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Whether the continue button is interactable.
        /// </summary>
        [Tooltip("Whether the continue button is interactable.")]
        [UxmlAttribute]
        public bool ContinueButtonInteractable
        {
            get => m_continueButton.enabledSelf;
            set => m_continueButton.SetEnabled(value);
        }

        /// <summary>
        /// The style class that, when applied to the <see cref="DialogPane"/>, causes it to be visible. If left blank, visibility will be controlled by <see cref="DisplayStyle"/>.
        /// </summary>
        [Tooltip("The style class that, when applied to the DialogPane, causes it to be visible. If left blank, visibility will be controlled by DisplayStyle.")]
        [UxmlAttribute]
        public string VisibilityStyleClass
        {
            get => m_visibilityStyleClass;
            set => m_visibilityStyleClass = value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether the <see cref="DialogPane"/> is visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                if (!string.IsNullOrEmpty(VisibilityStyleClass))
                {
                    return ClassListContains(VisibilityStyleClass);
                }
                else
                {
                    return style.display == DisplayStyle.Flex;
                }
            }

            set
            {
                if (!string.IsNullOrEmpty(VisibilityStyleClass))
                {
                    if (value && !ClassListContains(VisibilityStyleClass))
                    {
                        AddToClassList(VisibilityStyleClass);
                    }
                    else if (!value && ClassListContains(VisibilityStyleClass))
                    {
                        RemoveFromClassList(VisibilityStyleClass);
                    }
                }
                else
                {
                    style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        /// <summary>
        /// Gets the number of response buttons currently displayed in the dialog pane.
        /// </summary>
        public int ResponseButtonCount => m_responseButtons.Count;

        /// <summary>
        /// Gets or sets whether the response options are currently visible in the dialog pane.
        /// </summary>
        public bool ResponsesAreVisible
        {
            get => m_responseContainer.style.display == DisplayStyle.Flex;
            set => m_responseContainer.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        #endregion

        #region Constructor

        public DialogPane() : base()
        {
            // Add the main class to the dialog pane for styling.
            AddToClassList("yontalane-dialog-pane");

            // Ensure the background frame cannot take focus.
            pickingMode = PickingMode.Ignore;
            focusable = false;

            // Create and add the main frame element to contain all dialog UI elements.
            m_frame = new()
            {
                name = "yontalane-dialog-frame",
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            Add(m_frame);

            // Create and add the portrait field for displaying the speaker's image.
            m_portraitField = new()
            {
                name = "yontalane-dialog-portrait-field",
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            m_frame.Add(m_portraitField);

            // Create and add the label for displaying the speaker's name.
            m_speakerField = new()
            {
                name = "yontalane-dialog-speaker-field",
                enableRichText = true,
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            m_frame.Add(m_speakerField);

            // Create and add the label for displaying the dialog text.
            m_textField = new()
            {
                name = "yontalane-dialog-text-field",
                enableRichText = true,
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            m_frame.Add(m_textField);

            // Create and add the continue button for progressing the dialog.
            m_continueButton = new()
            {
                name = "yontalane-dialog-continue-button",
                enableRichText = true,
            };
            m_frame.Add(m_continueButton);

            // Create and add the close button for closing the dialog.
            m_closeButton = new()
            {
                name = "yontalane-dialog-close-button",
                Text = "×",
                enableRichText = true,
            };
            m_frame.Add(m_closeButton);

            // Create and add the container for response buttons.
            m_responseContainer = new()
            {
                name = "yontalane-dialog-response-container",
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            m_frame.Add(m_responseContainer);

            // Create and add the skip button for skipping the dialog.
            m_skipButton = new()
            {
                name = "yontalane-dialog-skip-button",
            };
            m_frame.Add(m_skipButton);

            // Register event listeners for the continue button.
            m_continueButton.clicked += () =>
            {
                DialogEvent dialogEvent = new()
                {
                    dialogPane = this,
                    dialogPaneName = name,
                    speaker = Speaker,
                    dialogText = Text,
                    buttonType = DialogButtonType.Continue,
                    responseIndex = -1,
                    responseText = string.Empty,
                };

                OnClickDialogContinue?.Invoke(dialogEvent);
                OnClickContinue?.Invoke(dialogEvent);
            };

            // Register event listeners for the skip button.
            m_skipButton.clicked += () =>
            {
                DialogEvent dialogEvent = new()
                {
                    dialogPane = this,
                    dialogPaneName = name,
                    speaker = Speaker,
                    dialogText = Text,
                    buttonType = DialogButtonType.Skip,
                    responseIndex = -1,
                    responseText = string.Empty,
                };

                OnClickDialogSkip?.Invoke(dialogEvent);
                OnClickSkip?.Invoke(dialogEvent);
            };

            // Register event listeners for the close button.
            m_closeButton.clicked += () =>
            {
                DialogEvent dialogEvent = new()
                {
                    dialogPane = this,
                    dialogPaneName = name,
                    speaker = Speaker,
                    dialogText = Text,
                    buttonType = DialogButtonType.Close,
                    responseIndex = -1,
                    responseText = string.Empty,
                };

                OnClickDialogClose?.Invoke(dialogEvent);
                OnClickClose?.Invoke(dialogEvent);
            };

            SelectableButton.OnButtonEvent += OnButtonEvent;

            // Add the stylesheet resource for dialog pane styling.
            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));

            // Set default values for speaker, text, and continue button label.
            Speaker = "Speaker Name";
            Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            ContinueButtonLabel = "Continue";

            // Refresh the text and speaker fields to reflect initial values.
            RefreshTextAndSpeakerFields();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttonEventInfo"></param>
        private void OnButtonEvent(SelectableButtonEventInfo buttonEventInfo)
        {
            if (buttonEventInfo.type != SelectableButtonEventType.FocusIn)
            {
                return;
            }

            if (buttonEventInfo.target == m_continueButton)
            {
                DialogEvent dialogEvent = new()
                {
                    dialogPane = this,
                    dialogPaneName = name,
                    speaker = Speaker,
                    dialogText = Text,
                    buttonType = DialogButtonType.Continue,
                    responseIndex = -1,
                    responseText = string.Empty,
                };

                OnHighlightContinue?.Invoke(dialogEvent);
                OnHighlightDialogContinue?.Invoke(dialogEvent);
            }
            else if (buttonEventInfo.target == m_closeButton)
            {
                DialogEvent dialogEvent = new()
                {
                    dialogPane = this,
                    dialogPaneName = name,
                    speaker = Speaker,
                    dialogText = Text,
                    buttonType = DialogButtonType.Close,
                    responseIndex = -1,
                    responseText = string.Empty,
                };

                OnHighlightClose?.Invoke(dialogEvent);
                OnHighlightDialogClose?.Invoke(dialogEvent);
            }
            else if (buttonEventInfo.target.parent == m_responseContainer && m_responseContainer != null)
            {
                DialogEvent dialogEvent = new()
                {
                    dialogPane = this,
                    dialogPaneName = name,
                    speaker = Speaker,
                    dialogText = Text,
                    buttonType = DialogButtonType.Response,
                    responseIndex = -1,
                    responseText = string.Empty,
                };

                OnHighlightResponse?.Invoke(dialogEvent);
                OnHighlightDialogResponse?.Invoke(dialogEvent);
            }
        }

        #endregion

        #region Response Button Management

        /// <summary>
        /// Removes all response buttons from the dialog pane and clears the internal response button list.
        /// </summary>
        public void ClearResponses()
        {
            m_responseContainer.Clear();
            m_responseButtons.Clear();

            // Show the continue button when no response buttons are present.
            m_continueButton.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Adds a response button with the specified text to the dialog pane and registers its click event.
        /// </summary>
        /// <param name="text">The label text for the response button.</param>
        /// <returns>The created <see cref="IconButton"/> instance.</returns>
        public IconButton AddResponseButton(string text)
        {
            // Create a new IconButton for the response and set its style and text.
            IconButton button = new();
            button.AddToClassList("yontalane-dialog-response-button");
            button.enableRichText = true;
            button.Text = text;
            m_responseContainer.Add(button);

            // Calculate the index of the new response button within the response container.
            int buttonIndex = m_responseContainer.childCount - 1;

            // Register the click event handler for the response button.
            button.clicked += () =>
            {
                // Construct a DialogEvent describing this response button click.
                DialogEvent dialogEvent = new()
                {
                    dialogPane = this,
                    dialogPaneName = name,
                    speaker = Speaker,
                    dialogText = Text,
                    buttonType = DialogButtonType.Response,
                    responseIndex = buttonIndex,
                    responseText = text,
                };

                // Invoke both static and instance response click event handlers.
                OnClickDialogResponse?.Invoke(dialogEvent);
                OnClickResponse?.Invoke(dialogEvent);
            };

            // Add the new button to the internal list of response buttons.
            m_responseButtons.Add(button);

            // Hide the continue button when one or more response button is present.
            m_continueButton.style.display = DisplayStyle.None;

            // Return the created response button.
            return button;
        }

        /// <summary>
        /// Attempts to retrieve the text of a response button at the specified index.
        /// </summary>
        /// <param name="index">The index of the response button.</param>
        /// <param name="text">The text of the response button, if found.</param>
        /// <returns>True if the response text was successfully retrieved; otherwise, false.</returns>
        public bool TryGetResponseText(int index, out string text)
        {
            // Check if the index is out of bounds; if so, set text to empty and return false.
            if (index < 0 || index >= ResponseButtonCount)
            {
                text = string.Empty;
                return false;
            }

            // Retrieve the text of the response button at the specified index.
            text = m_responseButtons[index].Text;

            // Return true to indicate the response text was successfully retrieved.
            return true;
        }

        /// <summary>
        /// Retrieves the text of the response button at the specified index.
        /// </summary>
        /// <param name="index">The index of the response button.</param>
        /// <returns>The text of the response button if found; otherwise, an empty string.</returns>
        public string GetResponseText(int index) => TryGetResponseText(index, out string text) ? text : string.Empty;

        /// <summary>
        /// Attempts to set the text of a response button at the specified index.
        /// </summary>
        /// <param name="index">The index of the response button.</param>
        /// <param name="text">The new text to set for the response button.</param>
        /// <returns>True if the response text was successfully set; otherwise, false.</returns>
        public bool TrySetResponseText(int index, string text)
        {
            // Check if the index is out of bounds; if so, return false.
            if (index < 0 || index >= ResponseButtonCount)
            {
                return false;
            }

            // Set the text of the response button at the specified index.
            m_responseButtons[index].Text = text;

            // Return true to indicate the response text was successfully set.
            return true;
        }

        /// <summary>
        /// Sets the text of the response button at the specified index.
        /// </summary>
        /// <param name="index">The index of the response button.</param>
        /// <param name="text">The new text to set for the response button.</param>
        public void SetResponseText(int index, string text) => _ = TrySetResponseText(index, text);

        #endregion

        #region Text and Speaker Management

        /// <summary>
        /// Calculates the length of the dialog text after parsing, excluding any markup tags (e.g., text within angle brackets).
        /// </summary>
        /// <returns>The number of visible characters in the parsed dialog text.</returns>
        public int GetParsedTextLength()
        {
            // Return 0 if the dialog text is null or empty.
            if (string.IsNullOrEmpty(m_fullText))
            {
                return 0;
            }

            // Initialize counters for visible characters and current index.
            int characterCounter = 0;
            int characterIndex = 0;

            // Iterate through the dialog text to count visible characters, skipping markup tags.
            while (true)
            {
                // Break if we've reached the end of the text.
                if (characterIndex >= m_fullText.Length)
                {
                    break;
                }

                // Get the current character as a string.
                string currentCharacter = m_fullText.Substring(characterIndex, 1);

                // If we encounter a '<', skip all characters until after the next '>'.
                if (currentCharacter == "<")
                {
                    // Continue advancing the index until we find a '>'.
                    while (currentCharacter != ">")
                    {
                        currentCharacter = m_fullText.Substring(++characterIndex, 1);
                    }

                    // Move past the '>'.
                    characterIndex++;

                    // If we've reached the end after skipping the tag, break.
                    if (characterIndex >= m_fullText.Length)
                    {
                        break;
                    }
                }

                // Count the current visible character and move to the next.
                characterCounter++;
                characterIndex++;
            }

            // Return the total count of visible characters.
            return characterCounter;
        }

        /// <summary>
        /// Sets the speaker name, dialog text, and the number of characters to display, then refreshes the UI fields.
        /// </summary>
        /// <param name="speaker">The name of the speaker.</param>
        /// <param name="text">The dialog text to display.</param>
        /// <param name="characterCount">The number of characters from the dialog text to display.</param>
        public void SetText(string speaker, string text, int characterCount)
        {
            // Set the speaker name field and full dialog text field to the provided strings.
            m_speakerText = speaker;
            m_fullText = text;

            // Clamp the number of visible characters between 0 and the length of the full text (or int.MaxValue if the text is null or empty).
            m_maxVisibleCharacters = Mathf.Clamp(characterCount, 0, !string.IsNullOrEmpty(m_fullText) ? m_fullText.Length : int.MaxValue);

            // Refresh the UI fields to reflect the updated speaker and text.
            RefreshTextAndSpeakerFields();
        }

        /// <summary>
        /// Sets the speaker name and dialog text, and optionally makes all characters in the dialog text visible.
        /// </summary>
        /// <param name="speaker">The name of the speaker.</param>
        /// <param name="text">The dialog text to display.</param>
        /// <param name="makeAllCharactersVisible">If true, all characters in the dialog text will be shown.</param>
        public void SetText(string speaker, string text, bool makeAllCharactersVisible = false)
        {
            // Set the speaker name field and full dialog text field to the provided strings.
            m_speakerText = speaker;
            m_fullText = text;

            // If all characters should be made visible, set MaxVisibleCharacters to int.MaxValue.
            if (makeAllCharactersVisible)
            {
                MaxVisibleCharacters = int.MaxValue;
            }

            // Refresh the UI fields to reflect the updated speaker and text.
            RefreshTextAndSpeakerFields();
        }

        /// <summary>
        /// Sets the dialog text and the number of characters to display, then refreshes the UI fields.
        /// </summary>
        /// <param name="text">The dialog text to display.</param>
        /// <param name="characterCount">The number of characters from the dialog text to display.</param>
        public void SetText(string text, int characterCount)
        {
            // Set the full dialog text field to the provided string.
            m_fullText = text;

            // Clamp the number of visible characters between 0 and the length of the full text (or int.MaxValue if the text is null or empty).
            m_maxVisibleCharacters = Mathf.Clamp(characterCount, 0, !string.IsNullOrEmpty(m_fullText) ? m_fullText.Length : int.MaxValue);

            // Refresh the UI fields to reflect the updated speaker and text.
            RefreshTextAndSpeakerFields();
        }

        /// <summary>
        /// Sets the dialog text and optionally makes all characters visible.
        /// </summary>
        /// <param name="text">The dialog text to display.</param>
        /// <param name="makeAllCharactersVisible">If true, all characters in the dialog text will be shown.</param>
        public void SetText(string text, bool makeAllCharactersVisible = false)
        {
            // Set the full dialog text field to the provided string.
            m_fullText = text;

            // If all characters should be made visible, set MaxVisibleCharacters to int.MaxValue.
            if (makeAllCharactersVisible)
            {
                MaxVisibleCharacters = int.MaxValue;
            }

            // Refresh the UI fields to reflect the updated speaker and text.
            RefreshTextAndSpeakerFields();
        }

        /// <summary>
        /// Returns the first word from the given text, stopping at a space, tab, newline, or hyphen.
        /// </summary>
        /// <param name="text">The input string to extract the first word from.</param>
        /// <returns>The first word found in the input string.</returns>
        private static string GetFirstWord(string text)
        {
            // Return an empty string if the input text is null or empty.
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }

            // Initialize an empty string to accumulate the first word.
            string word = string.Empty;

            // Iterate through each character in the input text.
            for (int i = 0; i < text.Length; i++)
            {
                // Extract the current character as a string.
                string character = text.Substring(i, 1);

                // Break if the character is any common word-breaking character.
                if (character == " " || character == "\t" || character == "\n" || character == "\r" || character == "-" || character == "_" || character == "." || character == "," || character == ";" || character == ":" || character == "!" || character == "?" || character == "/" || character == "\\" || character == "(" || character == ")" || character == "[" || character == "]" || character == "{" || character == "}" || character == "\"" || character == "'" || character == "|" || character == "<" || character == ">" || character == "=" || character == "+" || character == "*" || character == "&" || character == "^" || character == "%" || character == "$" || character == "#" || character == "@" || character == "~" || character == "`") break;

                // Append the current character to the word.
                word = $"{word}{character}";
            }

            // Return the accumulated word.
            return word;
        }

        /// <summary>
        /// Updates the UI fields for the dialog text and speaker name, ensuring that only the allowed number of characters
        /// (including handling of rich text tags) are visible in the dialog display.
        /// </summary>
        private void RefreshTextAndSpeakerFields()
        {
            // Initialize the output string that will accumulate the visible text.
            string s = string.Empty;
            
            // Initialize the index to track the current character position in the full text.
            int characterIndex = 0;

            // Only process if there are characters to show and the full text is not null or empty.
            if (m_maxVisibleCharacters > 0 && !string.IsNullOrEmpty(m_fullText))
            {
                // Counter to keep track of the number of visible (non-tag) characters processed so far.
                int characterCounter = 0;

                // Loop through the text until we reach the end or the max visible characters.
                while (true)
                {
                    // Break if we've reached the end of the string.
                    if (characterIndex >= m_fullText.Length)
                    {
                        break;
                    }

                    // Get the current character at the current index.
                    string currentCharacter = m_fullText.Substring(characterIndex, 1);

                    // Initialize a string to hold any tag we encounter.
                    string tag = string.Empty;

                    // If the current character is the start of a rich text tag.
                    if (currentCharacter == "<")
                    {
                        tag = currentCharacter;

                        // Continue appending characters to the tag until we reach the closing '>'.
                        while (currentCharacter != ">")
                        {
                            currentCharacter = m_fullText.Substring(++characterIndex, 1);
                            tag = $"{tag}{currentCharacter}";
                        }

                        // Move to the next character after the tag.
                        characterIndex++;

                        // If we've reached the end after the tag, break out of the loop.
                        if (characterIndex >= m_fullText.Length)
                        {
                            break;
                        }

                        // Get the next character after the tag.
                        currentCharacter = m_fullText.Substring(characterIndex, 1);
                    }

                    // Append the tag (if any) and the current character to the output string.
                    s = $"{s}{tag}{currentCharacter}";

                    // Increment the visible character counter and move to the next character.
                    characterCounter++;
                    characterIndex++;

                    // If we've reached the maximum number of visible characters, exit the loop.
                    if (characterCounter >= MaxVisibleCharacters)
                    {
                        break;
                    }
                }
            }

            // Get the next word after the currently visible characters, to maintain proper word wrapping.
            string remainingWord = string.Empty;
            if (!string.IsNullOrEmpty(m_fullText) && characterIndex < m_fullText.Length - 1)
            {
                remainingWord = GetFirstWord(m_fullText[characterIndex..]);
            }

            // Optionally capitalize the speaker's name.
            string speakerText = m_speakerIsCapitalized ? m_speakerText.ToUpper() : m_speakerText;

            // Format the speaker's name and the text that follows, applying the speaker's color.
            speakerText = $"<color=#{ColorUtility.ToHtmlStringRGB(m_speakerColor)}>{speakerText}{m_speakerTextAfter}</color>";

            // If the speaker's name should be bold, wrap it in bold tags.
            if (m_speakerIsBold)
            {
                speakerText = $"<b>{speakerText}</b>";
            }

            // Set the text fields based on whether the speaker's name should be inline with the dialog text.
            if (!m_speakerIsInline)
            {
                // Speaker name is shown in its own field, dialog text is separate.
                m_speakerField.text = speakerText;
                m_textField.text = $"{s}<alpha=#00>{remainingWord}";
            }
            else
            {
                // Speaker name is prepended inline to the dialog text.
                m_speakerField.text = string.Empty;
                m_textField.text = $"{speakerText}{s}<alpha=#00>{remainingWord}";
            }
        }

        /// <summary>
        /// Makes all characters in the dialog text visible by setting the maximum visible characters to the largest possible value.
        /// </summary>
        public void MakeAllCharactersVisible() => MaxVisibleCharacters = int.MaxValue;

        #endregion

        #region Portrait Management

        /// <summary>
        /// Set the sprite of the portrait image displayed in the dialog pane and optionally shows the portrait element.
        /// </summary>
        /// <param name="showElement">Whether to show the portrait element.</param>
        public void SetPortrait(Sprite sprite, bool showElement = true)
        {
            Portrait = sprite;

            if (showElement)
            {
                PortraitVisible = true;
            }
        }

        /// <summary>
        /// Clears the portrait image displayed in the dialog pane and optionally hide the portrait element.
        /// </summary>
        /// <param name="hideElement">Whether to hide the portrait element.</param>
        public void ClearPortrait(bool hideElement = true)
        {
            Portrait = null;

            if (hideElement)
            {
                PortraitVisible = false;
            }
        }

        #endregion

        #region Focus Management

        /// <summary>
        /// Sets keyboard focus to the specified button in the dialog pane.
        /// </summary>
        private void FocusOnButton(Button button)
        {
            UIDocument document = Object.FindAnyObjectByType<UIDocument>();

            if (document != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(document.gameObject);
            }

            button.Focus();
        }

        /// <summary>
        /// Sets keyboard focus to the "Continue" button in the dialog pane.
        /// </summary>
        public void FocusOnContinueButton() => FocusOnButton(m_continueButton);

        /// <summary>
        /// Sets keyboard focus to the "Skip" button in the dialog pane.
        /// </summary>
        public void FocusOnSkipButton() => FocusOnButton(m_skipButton);

        /// <summary>
        /// Sets keyboard focus to the "Close" button in the dialog pane.
        /// </summary>
        public void FocusOnCloseButton() => FocusOnButton(m_closeButton);

        /// <summary>
        /// Sets keyboard focus to the first response button in the dialog pane, if any exist.
        /// </summary>
        public void FocusOnFirstResponseButton()
        {
            if (ResponseButtonCount < 1)
            {
                return;
            }

            FocusOnButton(m_responseButtons[0]);
        }

        #endregion
    }
}
