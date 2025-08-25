using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Yontalane.Dialog;

namespace Yontalane.DialogUIElements
{
    /// <summary>
    /// Represents an event containing information about a dialog portrait,
    /// including the associated agent, line data, UI element, and speaker name.
    /// </summary>
    public struct PortraitEvent
    {
        [Tooltip("The dialog agent associated with this portrait event.")]
        public IDialogAgent agent;

        [Tooltip("The line data associated with this portrait event.")]
        public LineData data;

        [Tooltip("The UI element displaying the portrait.")]
        public VisualElement portraitUI;

        [Tooltip("The name of the speaker for this portrait event.")]
        public string speaker;
    }

    /// <summary>
    /// DialogUI is a singleton MonoBehaviour that implements the IDialogUI interface,
    /// managing the display and interaction of dialog UI elements, including speaker portraits,
    /// dialog text, audio cues, and user input for progressing dialog sequences.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog UI Toolkit/Dialog UI")]
    public sealed class DialogUI : Singleton<DialogUI>, IDialogUI
    {
        #region Delegates
        /// <summary>
        /// Represents an action that gets invoked when an audio clip is requested.
        /// </summary>
        [Serializable]
        public class GetAudioClipAction : UnityEvent<IDialogAgent, LineData, Action<AudioClip>> { }

        /// <summary>
        /// Represents an action that gets invoked when a sprite is requested.
        /// </summary>
        [Serializable]
        public class SetSpriteAction : UnityEvent<PortraitEvent> { }

        /// <summary>
        /// Represents an action that gets invoked when a sprite is requested.
        /// </summary>
        [Serializable]
        public class GetSpriteAction : UnityEvent<IDialogAgent, LineData, Action<Sprite>> { }
        #endregion

        #region Constants
        /// <summary>
        /// The delay before the continue handler are enabled.
        /// </summary>
        private const float HANDLER_DELAY = 0.15f;
        #endregion

        #region Private Fields
        private static AudioSource s_clickAudioSource = null;
        private static AudioSource s_clickLoopAudioSource = null;
        private static AudioSource s_blastAudioSource = null;

        private string m_speaker = "";
        private LineData m_line = null;
        private string m_text = null;
        private bool m_writingInProgress = false;
        private Action<string> m_lineCompleteCallback = null;
        private bool m_canUseContinueHandler = true;

        private DialogPane m_dialogPane;
        #endregion

        #region Serialized Fields
        [Header("Cosmetics")]

        [Tooltip("Color of player name in dialog header text.")]
        [SerializeField]
        private Color m_playerColor = Color.white;

        [Tooltip("Color of speaker name in dialog header text.")]
        [SerializeField]
        private Color m_speakerColor = Color.white;

        [Tooltip("Color of other speaker names in dialog header text.")]
        [SerializeField]
        private ColorSet[] m_speakerColors = new ColorSet[0];

        [Tooltip("Color of fallback speaker name in dialog header text.")]
        [SerializeField]
        private Color m_fallbackSpeakerColor = Color.white;

        [Tooltip("Whether to write out text character by character.")]
        [SerializeField]
        private bool m_useTypeCharacterInterval = true;

        [Tooltip("How long to wait between characters when writing out text.")]
        [SerializeField]
        [Min(0f)]
        private float m_typeCharacterInterval = 0.05f;

        [Tooltip("How long to wait before we can play another type sound effect. If zero, play the sound as soon as a new character is written. If one, wait for the previous sound to finish before playing. If 0.5, we can play new typing sound after the previous is at least halfway done.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float m_typeSoundDelay = 0f;

        [Tooltip("Whether to continue onto the next line immediately after finishing the current. Otherwise, wait for player input.")]
        [SerializeField]
        private bool m_autoContinue = false;

        [Header("Scene UI")]

        [Tooltip("The UI Document containing the root VisualElement for the dialog UI.")]
        [SerializeField]
        private UIDocument m_document = null;

        [Tooltip("The name of the dialog UI.")]
        [SerializeField]
        private string m_dialogPaneName = null;

        [Tooltip("Sound effect for clicking a button.")]
        [SerializeField]
        private AudioClip m_buttonClick = null;

        [Tooltip("Sound effect for typing during the text writing sequence.")]
        [SerializeField]
        private AudioClip m_typingDefault = null;

        [Tooltip("Sound effect to loop during typing during the text writing sequence.")]
        [SerializeField]
        private AudioClip m_typingLoopDefault = null;

        [Tooltip("Sound effect for highlighting the continue button or a response button.")]
        [SerializeField]
        private AudioClip m_buttonHighlightMain = null;

        [Tooltip("Sound effect for highlighting the close button.")]
        [SerializeField]
        private AudioClip m_buttonHighlightClose = null;

        [Tooltip("If a line of text has an accompanying sound blast, whether or not to cut the blast short if the text finishes before the sound does.")]
        [SerializeField]
        private bool m_stopBlastAfterText = true;

        [Header("Overrides")]

        [Tooltip("A callback that can be used to set a custom portrait instead of using the one requested by the dialog data. SetPortrait overrides GetPortrait, which in turn overrides the dialog data.")]
        [SerializeField]
        private SetSpriteAction m_setPortrait = new();

        [Tooltip("A callback that can be used to get a custom portrait instead of the one requested by the dialog data. SetPortrait overrides GetPortrait, which in turn overrides the dialog data.")]
        [SerializeField]
        private GetSpriteAction m_getPortrait = new();

        [Tooltip("A callback that can be used to get a custom sound instead of the one requested by the dialog data.")]
        [SerializeField]
        private GetAudioClipAction m_getSound = new();

        [Tooltip("A callback that can be used to get a custom voice instead of the one requested by the dialog data.")]
        [SerializeField]
        private GetAudioClipAction m_getVoice = new();

        [Header("Callbacks")]

        [Tooltip("An action that gets invoked when the dialog starts being typed out.")]
        [SerializeField]
        private UnityEvent m_onStartTyping = new();

        [Tooltip("An action that gets invoked while the dialog is being typed out.")]
        [SerializeField]
        private UnityEvent m_onType = new();

        [Tooltip("An action that gets invoked when the dialog is fully typed out.")]
        [SerializeField]
        private UnityEvent m_onDisplayLine = new();

        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the static audio source for playing click sounds.
        /// </summary>
        private static AudioSource ClickAudioSource
        {
            get
            {
                if (s_clickAudioSource == null)
                {
                    s_clickAudioSource = new GameObject().AddComponent<AudioSource>();
                    s_clickAudioSource.playOnAwake = false;
                    s_clickAudioSource.loop = false;
                    s_clickAudioSource.transform.SetParent(Camera.main.transform, false);
                }

                return s_clickAudioSource;
            }
        }

        /// <summary>
        /// Gets the static audio source for playing click loop sounds.
        /// </summary>
        private static AudioSource ClickLoopAudioSource
        {
            get
            {
                if (s_clickLoopAudioSource == null)
                {
                    s_clickLoopAudioSource = new GameObject().AddComponent<AudioSource>();
                    s_clickLoopAudioSource.playOnAwake = false;
                    s_clickLoopAudioSource.loop = true;
                    s_clickLoopAudioSource.transform.SetParent(Camera.main.transform, false);
                }

                return s_clickLoopAudioSource;
            }
        }

        /// <summary>
        /// Gets the static audio source for playing blast sounds.
        /// </summary>
        private static AudioSource BlastAudioSource
        {
            get
            {
                if (s_blastAudioSource == null)
                {
                    s_blastAudioSource = new GameObject().AddComponent<AudioSource>();
                    s_blastAudioSource.playOnAwake = false;
                    s_blastAudioSource.loop = false;
                    s_blastAudioSource.transform.SetParent(Camera.main.transform, false);
                }

                return s_blastAudioSource;
            }
        }
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            DialogPane.OnHighlightDialogContinue += OnHighlightButton;
            DialogPane.OnHighlightDialogResponse += OnHighlightButton;
            DialogPane.OnHighlightDialogClose += OnHighlightCloseButton;
        }

        private void OnDisable()
        {
            DialogPane.OnHighlightDialogContinue -= OnHighlightButton;
            DialogPane.OnHighlightDialogResponse -= OnHighlightButton;
            DialogPane.OnHighlightDialogClose -= OnHighlightCloseButton;
        }

        private void Start()
        {
            // Ensure m_document is assigned, either from the existing field, a component on this GameObject, or by searching the scene.
            m_document = m_document != null ? m_document : TryGetComponent(out UIDocument doc) ? doc : FindAnyObjectByType<UIDocument>();

            // If m_document is still null, log an error and exit.
            if (m_document == null)
            {
                Debug.LogError($"{nameof(UIDocument)} could not be found.");
                return;
            }

            // Attempt to find the DialogPane in the UIDocument's root visual element.
            m_dialogPane = m_document.rootVisualElement.Q<DialogPane>(m_dialogPaneName);

            // If m_dialogPane is not found, log an error and exit.
            if (m_dialogPane == null)
            {
                Debug.LogError($"{nameof(DialogPane)} could not be found.");
                return;
            }

            // Register event handlers for skip, continue, response, and close clicks.
            m_dialogPane.OnClickSkip += SkipWriteOut;
            m_dialogPane.OnClickContinue += OnClickResponse;
            m_dialogPane.OnClickResponse += OnClickResponse;
            m_dialogPane.OnClickClose += _ =>
            {
                // Play the button click sound effect if one is assigned.
                if (m_buttonClick != null)
                {
                    ClickAudioSource.PlayOneShot(m_buttonClick);
                }

                DialogProcessor.Instance.KillDialog();
            };

            // Initialize dialog pane UI state: hide responses, disable skip and continue buttons.
            m_dialogPane.ResponsesAreVisible = false;
            m_dialogPane.SkipButtonActive = false;
            m_dialogPane.ContinueButtonInteractable = false;

            // Clear any existing responses from the dialog pane.
            m_dialogPane.ClearResponses();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the dialog UI for a new line of dialog, setting up speaker, text, portrait, and callbacks.
        /// </summary>
        /// <param name="line">The dialog line data to display.</param>
        /// <param name="lineCompleteCallback">Callback to invoke when the line is fully written out or completed.</param>
        /// <param name="replaceInlineText">Function to replace inline keywords in the text and speaker fields.</param>
        public void Initiate(LineData line, Action<string> lineCompleteCallback, Func<string, string> replaceInlineText)
        {
            // Disable the continue button at the start of a new line.
            m_dialogPane.ContinueButtonInteractable = false;

            // Allow the continue handler to be used for this line.
            m_canUseContinueHandler = true;

            // Set up the speaker name and color, or clear if none.
            if (string.IsNullOrEmpty(line.speaker))
            {
                m_speaker = string.Empty;
            }
            else
            {
                m_dialogPane.SpeakerColor = GetSpeakerColor(line.speaker);
                m_speaker = replaceInlineText(line.speaker);
            }

            // Replace inline keywords in the main dialog text.
            m_text = replaceInlineText(line.text);

            // Replace inline keywords in the portrait field.
            line.portrait = replaceInlineText(line.portrait);

            // Store the current line and callback, and clear dialog pane text and speaker.
            m_line = line;
            m_lineCompleteCallback = lineCompleteCallback;
            m_dialogPane.Text = string.Empty;
            m_dialogPane.Speaker = string.Empty;

            // Attempt to get the portrait sprite via callback, or load from resources if not found.
            Sprite portrait = null;
            m_getPortrait?.Invoke(DialogProcessor.Instance.DialogAgent, line, (sprite) => portrait = sprite);

            if (portrait == null)
            {
                portrait = Resources.Load<Sprite>(line.portrait);
            }

            // Set or clear the portrait in the dialog pane.
            if (portrait != null)
            {
                m_dialogPane.SetPortrait(portrait);
            }
            else
            {
                m_dialogPane.ClearPortrait();
            }

            // Optionally invoke the SetPortrait callback, which may override the portrait UI.
            if (m_setPortrait != null)
            {
                m_setPortrait.Invoke(new()
                {
                    agent = DialogProcessor.Instance.DialogAgent,
                    data = line,
                    portraitUI = m_dialogPane.Q<VisualElement>("yontalane-dialog-portrait-field"),
                    speaker = m_speaker,
                });

                m_dialogPane.PortraitVisible = m_dialogPane.Portrait != null;
            }

            // Play a sound effect if specified in the line, using callback or loading from resources.
            if (!string.IsNullOrEmpty(line.sound))
            {
                AudioClip blast = null;
                m_getSound?.Invoke(DialogProcessor.Instance.DialogAgent, line, (clip) => blast = clip);
                if (blast == null)
                {
                    blast = Resources.Load<AudioClip>(line.sound);
                }

                if (blast != null)
                {
                    BlastAudioSource.PlayOneShot(blast);
                    if (m_autoContinue)
                    {
                        StartCoroutine(CheckForBlastComplete());
                    }
                }
            }

            // Play a voice clip if specified in the line, using callback or loading from resources.
            if (!string.IsNullOrEmpty(line.voice))
            {
                AudioClip blast = null;
                m_getVoice?.Invoke(DialogProcessor.Instance.DialogAgent, line, (clip) => blast = clip);
                if (blast == null)
                {
                    blast = Resources.Load<AudioClip>(line.voice);
                }

                if (blast != null)
                {
                    BlastAudioSource.PlayOneShot(blast);
                    if (m_autoContinue)
                    {
                        StartCoroutine(CheckForBlastComplete());
                    }
                }
            }

            // Clear any existing response buttons from the dialog pane.
            m_dialogPane.ClearResponses();

            // Add response buttons for each response in the line, formatting inline text.
            for (int i = 0; i < line.responses.Length; i++)
            {
                string responseText = FormatInlineText(replaceInlineText(line.responses[i].text));
                _ = m_dialogPane.AddResponseButton(responseText);
            }

            // Show the dialog pane.
            m_dialogPane.style.display = DisplayStyle.Flex;

            // Set focus to the first response button if any, otherwise to the skip button.
            if (m_dialogPane.ResponseButtonCount > 0)
            {
                m_dialogPane.FocusOnFirstResponseButton();
            }
            else
            {
                m_dialogPane.FocusOnSkipButton();
            }
            
            // Mark that writing is in progress.
            m_writingInProgress = true;

            // Start typing out the dialog text character by character, or end the line immediately.
            if (m_useTypeCharacterInterval)
            {
                m_onStartTyping?.Invoke();
                StartCoroutine(WriteOut());
            }
            else
            {
                EndLine();
            }
        }

        /// <summary>
        /// Hides the dialog pane by setting its display style to None.
        /// </summary>
        public void Close() => m_dialogPane.style.display = DisplayStyle.None;
        #endregion

        #region Internal Methods
        /// <summary>
        /// Handles the skip action for dialog text. If the skip button is active and the continue button is not interactable,
        /// this method skips the text write-out, disables the continue handler temporarily, and starts a delay coroutine to re-enable it.
        /// </summary>
        internal void SkipHandler()
        {
            // If the skip button is not active, do nothing and return.
            if (!m_dialogPane.SkipButtonActive)
            {
                return;
            }

            // If the continue button is already interactable, do nothing and return.
            if (m_dialogPane.ContinueButtonInteractable)
            {
                return;
            }

            // Skip the text write-out and temporarily disable the continue handler.
            SkipWriteOut(default);
            m_canUseContinueHandler = false;

            // If this GameObject is active, start a coroutine to re-enable the continue handler after a delay.
            if (gameObject.activeSelf)
            {
                StartCoroutine(HandlerDelay());
            }
        }

        /// <summary>
        /// Handles the continue action for dialog text. If the continue handler is enabled and the continue button is interactable,
        /// this method disables the continue button and triggers the response click logic.
        /// </summary>
        internal void ContinueHandler()
        {
            // Check if the continue handler is currently disabled; if so, exit early.
            if (!m_canUseContinueHandler)
            {
                return;
            }

            // Check if the continue button is not interactable; if so, exit early.
            if (!m_dialogPane.ContinueButtonInteractable)
            {
                return;
            }

            // Disable the continue button to prevent multiple triggers.
            m_dialogPane.ContinueButtonInteractable = false;

            // Trigger the response click logic to continue the dialog.
            OnClickResponse(default);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the color of the speaker's name in the dialog header text.
        /// </summary>
        /// <param name="speaker">The name of the speaker to get the color for.</param>
        /// <returns>The color of the speaker's name in the dialog header text.</returns>
        private Color GetSpeakerColor(string speaker)
        {
            SpeakerType speakerType = DialogProcessor.GetSpeakerType(speaker);

            // Return the color of the speaker's name in the dialog header text based on the speaker type
            switch (speakerType)
            {
                case SpeakerType.Player:
                    return m_playerColor;
                case SpeakerType.Self:
                    return m_speakerColor;
            }

            // Return the color of the speaker's name in the dialog header text based on the speaker colors
            foreach (ColorSet colorSet in m_speakerColors)
            {
                if (speaker.Contains(colorSet.speaker))
                {
                    return colorSet.color;
                }
            }

            // Return the fallback color of the speaker's name in the dialog header text
            return m_fallbackSpeakerColor;
        }

        private IEnumerator WriteOut()
        {
            // Summary:
            // This section prepares the typing and typing loop audio clips for the dialog line. It attempts to load custom sounds
            // specified in the line data, falling back to default sounds if none are provided. It also calculates the length of the
            // typing sound, initializes the last typed time, and starts playing the typing loop sound if available. Finally, it
            // initiates a coroutine to delay the highlight of the skip button.

            // If the line has a typing sound, load the sound
            // If the line has no typing sound, use the default typing sound
            AudioClip typing = null;
            if (!string.IsNullOrEmpty(m_line.typing))
            {
                typing = Resources.Load<AudioClip>(m_line.typing);
            }
            if (typing == null)
            {
                typing = m_typingDefault;
            }

            // Length in seconds of the typing sound
            float typingSoundLength = typing != null ? typing.length : 0f;

            // The time when the last character was typed; to be used with the typing sound
            float lastTypedTime = Time.time - 10f;

            // If the line has a typing loop sound, load the sound
            // If the line has no typing loop sound, use the default typing loop sound
            AudioClip typingLoop = null;
            if (!string.IsNullOrEmpty(m_line.typingLoop))
            {
                typingLoop = Resources.Load<AudioClip>(m_line.typingLoop);
            }
            if (typingLoop == null)
            {
                typingLoop = m_typingLoopDefault;
            }

            // If the line has a typing loop sound, play it
            if (typingLoop != null)
            {
                ClickLoopAudioSource.clip = typingLoop;
                ClickLoopAudioSource.Play();
            }

            // Summary:
            // This section prepares the speaker's name and dialog text for display. If a separate speaker field UI element exists,
            // it sets the speaker's name there; otherwise, it prepares to show the speaker's name inline with the dialog text.
            // It ensures rich text formatting is enabled, calculates the parsed lengths of the speaker and combined text,
            // and initially limits visible characters to only the speaker's name (if any), so the dialog text can be revealed character by character.

            // Format the speaker's name for inline display using rich text formatting.
            string formattedSpeakerText = FormatInlineText(m_speaker);

            // Set the dialog pane's text, combining the formatted speaker and the dialog text, with no visible characters initially.
            m_dialogPane.SetText(formattedSpeakerText, m_text, 0);

            // Wait for the end of the frame to ensure UI layout is updated before measuring text.
            yield return new WaitForEndOfFrame();

            // Get the total number of parsed (visible) characters in the dialog pane's text for the typewriter effect.
            int parsedTextLength = m_dialogPane.GetParsedTextLength();

            // Summary:
            // This loop reveals the dialog text one character at a time, starting after the speaker's name (if present).
            // For each character revealed, it updates the visible character count, invokes an optional "on type" event,
            // plays a typing sound if appropriate, refreshes layout groups if needed, updates the last typed time,
            // and waits for a specified interval before revealing the next character.

            // Start a coroutine to delay the highlight of the skip button
            StartCoroutine(DelayHighlightSkipButton());

            // Loop through each character in the parsed text to create a typewriter effect
            for (int currentIndex = 0; currentIndex < parsedTextLength; currentIndex++)
            {
                // Reveal one more character in the dialog pane
                m_dialogPane.MaxVisibleCharacters = currentIndex + 1;

                // Invoke the onType event if it exists
                m_onType?.Invoke();

                // Play the typing sound if enough time has passed since the last sound
                if (typing != null && Time.time - lastTypedTime >= typingSoundLength * m_typeSoundDelay)
                {
                    ClickAudioSource.PlayOneShot(typing);
                }

                // Update the last typed time to the current time
                lastTypedTime = Time.time;

                // Wait for the specified interval before revealing the next character
                yield return new WaitForSecondsRealtime(m_typeCharacterInterval);
            }

            // Stop the typing loop sound after the typewriter effect is complete
            ClickLoopAudioSource.Stop();

            // End the line and finalize the dialog display
            EndLine();
        }

        /// <summary>
        /// Coroutine that waits for the end of the frame before highlighting and activating the skip button in the dialog pane.
        /// Ensures UI layout is updated before making the skip button interactable and focused.
        /// </summary>
        private IEnumerator DelayHighlightSkipButton()
        {
            yield return new WaitForEndOfFrame();
            m_dialogPane.ResponsesAreVisible = false;
            m_dialogPane.SkipButtonActive = true;
            m_dialogPane.FocusOnSkipButton();
        }

        /// <summary>
        /// Formats the inline text.
        /// </summary>
        /// <param name="text">The text to format.</param>
        /// <returns>The formatted text.</returns>
        private static string FormatInlineText(string text)
        {
            // Initialize the open state
            bool isOpen = true;

            // While the text contains %% tags, format the text
            while (text.Contains("%%"))
            {
                int i = text.IndexOf("%%");
                text = text[..i] + (isOpen ? "<" : ">") + text[(i + 2)..];
                isOpen = !isOpen;
            }

            return text;
        }

        /// <summary>
        /// Immediately ends the typewriter effect and displays the full dialog line.
        /// </summary>
        /// <param name="_">The dialog event triggering the skip action (unused).</param>
        private void SkipWriteOut(DialogEvent _)
        {
            // Play the button click sound effect if one is assigned.
            if (m_buttonClick != null)
            {
                ClickAudioSource.PlayOneShot(m_buttonClick);
            }

            StopAllCoroutines();
            EndLine();
        }

        /// <summary>
        /// Finalizes the current dialog line by stopping the typewriter effect, displaying the full text,
        /// updating the UI to show responses or the continue button, and invoking any display callbacks.
        /// </summary>
        private void EndLine()
        {
            // Mark that writing is no longer in progress.
            m_writingInProgress = false;

            // Set the speaker name in the dialog pane, formatting any inline tags.
            m_dialogPane.Speaker = FormatInlineText(m_speaker);

            // Reveal all characters in the dialog text immediately.
            m_dialogPane.MakeAllCharactersVisible();

            // Show the response buttons in the dialog pane.
            m_dialogPane.ResponsesAreVisible = true;

            // Hide or deactivate the skip button.
            m_dialogPane.SkipButtonActive = false;

            // Invoke any registered callbacks for when the line is fully displayed.
            m_onDisplayLine?.Invoke();

            // If there are response buttons, focus on the first one; otherwise, enable and focus the continue button.
            if (m_dialogPane.ResponseButtonCount > 0)
            {
                m_dialogPane.FocusOnFirstResponseButton();
            }
            else
            {
                m_dialogPane.ContinueButtonInteractable = true;
                m_dialogPane.FocusOnContinueButton();
            }
        }

        /// <summary>
        /// Waits for a short delay before enabling the continue handler, preventing immediate input after a dialog event.
        /// </summary>
        private IEnumerator HandlerDelay()
        {
            yield return new WaitForSecondsRealtime(HANDLER_DELAY);
            m_canUseContinueHandler = true;
        }

        /// <summary>
        /// Handles the event when a response button or the continue button is clicked in the dialog UI.
        /// Stops any ongoing blast audio if configured, plays the button click sound, and invokes the line completion callback
        /// with the appropriate response link or null if no response text is present.
        /// </summary>
        /// <param name="dialogEvent">The dialog event containing information about the clicked response.</param>
        private void OnClickResponse(DialogEvent dialogEvent)
        {
            // Stop the blast audio if configured to do so after text finishes.
            if (m_stopBlastAfterText)
            {
                BlastAudioSource.Stop();
            }

            // Play the button click sound effect if one is assigned.
            if (m_buttonClick != null)
            {
                ClickAudioSource.PlayOneShot(m_buttonClick);
            }

            // Invoke the line completion callback with the appropriate response link or null if no response text.
            if (string.IsNullOrEmpty(dialogEvent.responseText))
            {
                m_lineCompleteCallback?.Invoke(null);
            }
            else
            {
                m_lineCompleteCallback?.Invoke(m_line.responses[dialogEvent.responseIndex].link);
            }
        }

        /// <summary>
        /// Coroutine that waits for the blast audio to finish playing while text is being written out.
        /// Once both the writing and audio are complete, it triggers the continue handler.
        /// </summary>
        private IEnumerator CheckForBlastComplete()
        {
            // Loop while both writing is in progress and the blast audio is playing.
            while (m_writingInProgress && BlastAudioSource.isPlaying)
            {
                // Wait until the next frame.
                yield return new WaitForEndOfFrame();

                // If both writing and blast audio have finished, trigger the continue handler and exit the loop.
                if (!m_writingInProgress && !BlastAudioSource.isPlaying)
                {
                    ContinueHandler();
                    break;
                }
            }
        }

        private void OnHighlightButton(DialogEvent dialogEvent)
        {
            if (dialogEvent.dialogPane != m_dialogPane)
            {
                return;
            }

            // Play the button highlight sound effect if one is assigned.
            if (m_buttonHighlightMain != null)
            {
                ClickAudioSource.PlayOneShot(m_buttonHighlightMain);
            }
        }

        private void OnHighlightCloseButton(DialogEvent dialogEvent)
        {
            if (dialogEvent.dialogPane != m_dialogPane)
            {
                return;
            }

            // Play the button highlight sound effect if one is assigned.
            if (m_buttonHighlightClose != null)
            {
                ClickAudioSource.PlayOneShot(m_buttonHighlightClose);
            }
        }
        #endregion
    }
}