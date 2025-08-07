using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Main UI controller for displaying and managing dialog interactions, including speaker display, text typing, responses, and associated UI elements.
    /// </summary>
    [Obsolete("DialogUI has been moved to a separate package and will be removed from the Dialog package in a future update. Please replace thie component in your project with the DialogUI component found in the DialogUGUI package.")]
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog/Dialog UI")]
    public sealed class DialogUI : Singleton<DialogUI>
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
        /// The name of the parameter used to control the visibility of the dialog UI.
        /// </summary>
        private const string ANIMATION_PARAMETER = "Dialog Visible";

        /// <summary>
        /// The delay before the continue handler are enabled.
        /// </summary>
        private const float HANDLER_DELAY = 0.15f;
        #endregion

        #region Private Fields
        private static readonly List<Button> s_tempResponseButtons = new();
        private static readonly List<InlineImageReplacementInfo> s_inlineImageReplacementInfo = new();
        private static readonly List<InlineImageReplacementPostProcessingInfo> s_inlineImageReplacementPostProcessingInfo = new();
        private static AudioSource s_clickAudioSource = null;
        private static AudioSource s_clickLoopAudioSource = null;
        private static AudioSource s_blastAudioSource = null;

        private string m_speaker = "";
        private LineData m_line = null;
        private string m_text = null;
        private bool m_writingInProgress = false;
        private Action<string> m_lineCompleteCallback = null;
        private bool m_canUseContinueHandler = true;
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

        [Tooltip("Whether or not the speaker name is bold.")]
        [SerializeField]
        private bool m_speakerBold = false;

        [Tooltip("Whether or not the speaker name is in caps.")]
        [SerializeField]
        private bool m_speakerCaps = false;

        [Tooltip("The string to appear between the speaker name and dialog text.")]
        [SerializeField]
        private string m_speakerSeparator = ": ";

        [Tooltip("Line breaks between the speaker name and the dialog text.")]
        [SerializeField]
        [Range(0, 2)]
        private int m_speakerLineBreaks = 0;

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

        [Tooltip("The root container of the dialog UI. Used to recursively refresh child layout groups.")]
        [SerializeField]
        private RectTransform m_dialogRoot = null;

        [Tooltip("Whether to force layout groups to refresh while typing.")]
        [SerializeField]
        private bool m_refreshLayoutGroups = true;

        [Tooltip("The Animator that controls the dialog UI.")]
        [SerializeField]
        private Animator m_animator = null;

        [Tooltip("The field for displaying dialog text.")]
        [SerializeField]
        private TMP_Text m_textField = null;

        [Tooltip("The optional field for displaying speaker's name. If left blank, the speaker's name will be displayed in the main text field.")]
        [SerializeField]
        private TMP_Text m_speakerField = null;

        [Tooltip("Button for skipping text writing sequence. May be the same as the continue button.")]
        [SerializeField]
        private Button m_skipButton = null;

        [Tooltip("Button for proceeding to the next line of dialog. May be the same as the skip button.")]
        [SerializeField]
        private Button m_continueButton = null;

        [Tooltip("The object within which dialog response button prefabs will be instantiated.")]
        [SerializeField]
        private RectTransform m_responseContainer = null;

        [Tooltip("The object that contains the portrait. Having a reference to both the container and the portrait helps with layout.")]
        [SerializeField]
        private RectTransform m_portraitContainer = null;

        [Tooltip("The Image that will display the speaker's portrait (if there is one).")]
        [SerializeField]
        private Image m_portrait = null;

        [Tooltip("Sound effect for clicking a response button.")]
        [SerializeField]
        private AudioClip m_buttonClick = null;

        [Tooltip("Sound effect for typing during the text writing sequence.")]
        [SerializeField]
        private AudioClip m_typingDefault = null;

        [Tooltip("Sound effect to loop during typing during the text writing sequence.")]
        [SerializeField]
        private AudioClip m_typingLoopDefault = null;

        [Tooltip("If a line of text has an accompanying sound blast, whether or not to cut the blast short if the text finishes before the sound does.")]
        [SerializeField]
        private bool m_stopBlastAfterText = true;

        [Header("Prefabs")]

        [Tooltip("The prefab to use for responses.")]
        [SerializeField]
        private Button m_responseButtonPrefab = null;

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
        /// <summary>
        /// Unity Start method. Initializes button listeners and sets initial UI state for dialog controls.
        /// </summary>
        private void Start()
        {
            // Add listeners to skip and continue buttons, and hide the continue button initially
            m_skipButton.onClick.AddListener(delegate { SkipWriteOut(); });
            m_continueButton.onClick.AddListener(delegate { OnClickResponse(null); });
            m_continueButton.gameObject.SetActive(false);

            // Hide the response container at start, if it exists
            if (m_responseContainer != null)
            {
                m_responseContainer.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the dialog UI with the given line data.
        /// </summary>
        /// <param name="line">The line data to initialize the dialog UI with.</param>
        /// <param name="lineCompleteCallback">The callback to invoke when the line is complete.</param>
        [Obsolete("DialogUI has been moved to a separate package and will be removed from the Dialog package in a future update. Please replace thie component in your project with the DialogUI component found in the DialogUGUI package.")]
        public void Initiate(LineData line, Action<string> lineCompleteCallback, Func<string, string> replaceInlineText)
        {
            s_inlineImageReplacementPostProcessingInfo.Clear();

            m_continueButton.gameObject.SetActive(false);

            m_canUseContinueHandler = true;
            string speaker = string.Empty;

            string lineBreak = string.Empty;
            for (int i = 0; i < m_speakerLineBreaks; i++)
            {
                lineBreak += "\n";
            }
            if (string.IsNullOrEmpty(line.speaker))
            {
                m_speaker = string.Empty;
            }
            else
            {
                string speakerColor = ColorUtility.ToHtmlStringRGBA(GetSpeakerColor(line.speaker));
                speaker = replaceInlineText(line.speaker);
                if (m_speakerCaps)
                {
                    speaker = speaker.ToUpper();
                }
                if (m_speakerBold)
                {
                    m_speaker = "<color=#" + speakerColor + "><b>" + speaker + m_speakerSeparator + "</b></color>" + lineBreak;
                }
                else
                {
                    m_speaker = "<color=#" + speakerColor + ">" + speaker + m_speakerSeparator + "</color>" + lineBreak;
                }
            }
            m_text = replaceInlineText(line.text);

            // Destroy any inline images from the previous dialog line before initializing the new one
            DestroyInlineImages();

            line.portrait = replaceInlineText(line.portrait);

            m_line = line;
            m_lineCompleteCallback = lineCompleteCallback;
            m_textField.text = "";

            if (m_speakerField != null)
            {
                m_speakerField.text = string.Empty;
            }

            // Portrait UI code below
            if (m_portraitContainer != null && m_portrait != null)
            {
                Sprite portrait = null;

                // Trying to get portrait from callback
                m_getPortrait?.Invoke(DialogProcessor.Instance.DialogAgent, line, (sprite) => portrait = sprite);

                // If callback did not provide a portrait, then try to get portrait from line data
                if (portrait == null)
                {
                    portrait = Resources.Load<Sprite>(line.portrait);
                }

                // If portrait is not null, then display it
                if (portrait != null)
                {
                    // Set the portrait UI image sprite
                    m_portrait.sprite = portrait;

                    // If the image has an aspect ratio fitter, then update that
                    if (m_portrait.TryGetComponent(out AspectRatioFitter fitter))
                    {
                        fitter.aspectRatio = portrait.rect.width / portrait.rect.height;
                    }

                    // Make the portrait UI container visible
                    m_portraitContainer.gameObject.SetActive(true);
                }
                else
                // If the portrait is null, then hide the UI
                {
                    // Set the portrait UI image sprite
                    m_portrait.sprite = null;

                    // Make the portrait UI container invisible
                    m_portraitContainer.gameObject.SetActive(false);
                }

                // If there is a SetPortrait callback, then invoke it
                if (m_setPortrait != null)
                {
                    // Invoke the SetPortrait callback (this may override the portrait UI code above)
                    m_setPortrait.Invoke(new()
                    {
                        agent = DialogProcessor.Instance.DialogAgent,
                        data = line,
                        image = m_portrait,
                        speaker = speaker,
                    });

                    // Show or hide the portrait UI depending on if the portrait sprite exists
                    m_portraitContainer.gameObject.SetActive(m_portrait.sprite != null);
                }
            }

            // Play the sound effect specified in the line, if any
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

            // Play the voice audio specified in the line, if any
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

            // If the response container exists, clear its children and set up new response buttons for each response in the line
            if (m_responseContainer != null)
            {
                // Destroy all existing child GameObjects in the response container
                for (int i = m_responseContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(m_responseContainer.GetChild(i).gameObject);
                }

                // Hide the response container and clear the temporary response buttons list
                m_responseContainer.gameObject.SetActive(false);
                s_tempResponseButtons.Clear();

                // Create a new button for each response in the line
                for (int i = 0; i < line.responses.Length; i++)
                {
                    // Instantiate a new response button and set up its click listener
                    Button instance = Instantiate(m_responseButtonPrefab.gameObject).GetComponent<Button>();
                    instance.onClick.AddListener(delegate
                    {
                        OnClickResponse(instance);
                    });

                    // Set the button's text to the formatted response text
                    TMP_Text text = instance.GetComponentInChildren<TMP_Text>();
                    text.text = FormatInlineText(replaceInlineText(line.responses[i].text));

                    // Set the button's transform properties and add it to the response container
                    instance.GetComponent<RectTransform>().SetParent(m_responseContainer);
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localEulerAngles = Vector3.zero;
                    instance.transform.localScale = Vector3.one;
                    instance.navigation = Navigation.defaultNavigation;

                    // Add the button to the temporary response buttons list
                    s_tempResponseButtons.Add(instance);

                    // If this is the first button, skip navigation setup
                    if (i == 0) continue;

                    // Set up explicit navigation between this button and the previous one
                    Button prev = s_tempResponseButtons[^2];

                    // Set up explicit navigation for the current button to point left and up to the previous button
                    Navigation currNav = instance.navigation;
                    currNav.mode = Navigation.Mode.Explicit;
                    currNav.selectOnLeft = prev;
                    currNav.selectOnUp = prev;
                    instance.navigation = currNav;

                    // Set up explicit navigation for the previous button to point right and down to the current button
                    Navigation prevNav = prev.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    prevNav.selectOnRight = instance;
                    prevNav.selectOnDown = instance;
                    prev.navigation = prevNav;
                }

                // Clear the temporary response buttons list after setup
                s_tempResponseButtons.Clear();
            }

            // If an animator is assigned, trigger the dialog open animation by setting the appropriate parameter.
            if (m_animator != null)
            {
                m_animator.SetBool(ANIMATION_PARAMETER, true);
            }

            // Mark that the dialog is currently writing out text.
            m_writingInProgress = true;

            // If typewriter effect is enabled, invoke the start typing event and begin the coroutine to write out text character by character.
            if (m_useTypeCharacterInterval)
            {
                m_onStartTyping?.Invoke();
                StartCoroutine(WriteOut());
            }
            // Otherwise, immediately end the line (show all text at once).
            else
            {
                EndLine();
            }
        }

        /// <summary>
        /// Closes the dialog UI.
        /// </summary>
        public void Close()
        {
            if (m_animator != null)
            {
                m_animator.SetBool(ANIMATION_PARAMETER, false);
            }

            DestroyInlineImages();
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Handles the skip button click.
        /// </summary>
        internal void SkipHandler()
        {
            // If the skip button doesn't exist, do nothing
            if (m_skipButton == null)
            {
                return;
            }

            // If the skip button is not active, do nothing
            if (!m_skipButton.gameObject.activeInHierarchy)
            {
                return;
            }

            // If the continue button doesn't exist, do nothing
            if (m_continueButton == null)
            {
                return;
            }

            // If the continue button is active, do nothing
            if (m_continueButton.gameObject.activeSelf)
            {
                return;
            }

            // Skip the text writing sequence and disable the continue handler
            SkipWriteOut();
            m_canUseContinueHandler = false;

            // If the dialog UI is active, start a delay to enable the continue handler
            if (gameObject.activeSelf)
            {
                StartCoroutine(HandlerDelay());
            }
        }

        /// <summary>
        /// Handles the continue button click.
        /// </summary>
        internal void ContinueHandler()
        {
            // If the continue handler is not enabled, do nothing
            if (!m_canUseContinueHandler)
            {
                return;
            }

            // If the continue button doesn't exist, do nothing
            if (m_continueButton == null)
            {
                return;
            }

            if (!m_continueButton.gameObject.activeInHierarchy)
            {
                return;
            }

            // Hide the continue button and trigger the response handler when continue is pressed
            m_continueButton.gameObject.SetActive(false);
            OnClickResponse(null);
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

        /// <summary>
        /// Writes out the dialog text character by character.
        /// </summary>
        /// <returns>An enumerator that writes out the dialog text character by character.</returns>
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

            // Start a coroutine to delay the highlight of the skip button
            StartCoroutine(DelayHighlightSkipButton());


            // Summary:
            // This section prepares the speaker's name and dialog text for display. If a separate speaker field UI element exists,
            // it sets the speaker's name there; otherwise, it prepares to show the speaker's name inline with the dialog text.
            // It ensures rich text formatting is enabled, calculates the parsed lengths of the speaker and combined text,
            // and initially limits visible characters to only the speaker's name (if any), so the dialog text can be revealed character by character.

            // Check if the speaker field UI element exists
            bool speakerFieldExists = m_speakerField != null;

            // Format the speaker's name for inline display (e.g., replacing keywords)
            string formattedSpeakerText = FormatInlineText(m_speaker);

            // If the speaker field exists, set its text to the formatted speaker name
            if (speakerFieldExists)
            {
                m_speakerField.text = formattedSpeakerText;
                PrepareInlineImages(ref m_speakerField, null);
            }

            // Ensure the main text field supports rich text formatting
            m_textField.richText = true;

            // If the speaker field does not exist, prepare to display the speaker name inline with the dialog text
            string inlineSpeakerText = speakerFieldExists ? string.Empty : formattedSpeakerText;

            // Set the text field to only the inline speaker text and force a mesh update to parse it
            // Get the length of the parsed speaker text
            m_textField.text = inlineSpeakerText;
            PrepareInlineImages(ref m_textField, null);
            m_textField.ForceMeshUpdate(true, true);
            string inlineSpeakerTextParsed = m_textField.GetParsedText();
            int inlineSpeakerTextParsedLength = inlineSpeakerTextParsed.Length;

            // Set the text field to the full dialog line (speaker + text) and force a mesh update to parse it
            m_textField.text = $"{inlineSpeakerText}{m_text}";
            yield return new WaitForEndOfFrame();
            PrepareInlineImages(ref m_textField, s_inlineImageReplacementPostProcessingInfo);

            RefreshInlineImages(-1);

            m_textField.ForceMeshUpdate(true, true);
            string combinedTextParsed = m_textField.GetParsedText();
            int combinedTextParsedLength = combinedTextParsed.Length;

            // Initially, only show the speaker text (if any) by limiting visible characters
            m_textField.maxVisibleCharacters = inlineSpeakerTextParsedLength;

            // Summary:
            // This loop reveals the dialog text one character at a time, starting after the speaker's name (if present).
            // For each character revealed, it updates the visible character count, invokes an optional "on type" event,
            // plays a typing sound if appropriate, refreshes layout groups if needed, updates the last typed time,
            // and waits for a specified interval before revealing the next character.
            for (int currentIndex = inlineSpeakerTextParsedLength; currentIndex < combinedTextParsedLength; currentIndex++)
            {
                // Reveal the next character in the dialog text
                m_textField.maxVisibleCharacters = currentIndex + 1;

                // Invoke the on type event
                m_onType?.Invoke();

                // If the line has a typing sound, play it
                if (typing != null && Time.time - lastTypedTime >= typingSoundLength * m_typeSoundDelay)
                {
                    ClickAudioSource.PlayOneShot(typing);
                }

                // If the refresh layout groups flag is set, refresh the layout groups
                if (m_refreshLayoutGroups)
                {
                    Utility.RefreshLayoutGroupsImmediateAndRecursive(m_dialogRoot.gameObject);
                }

                // Update the last typed character time
                lastTypedTime = Time.time;

                RefreshInlineImages(currentIndex);

                // Wait for the next character
                yield return new WaitForSecondsRealtime(m_typeCharacterInterval);
            }

            // Stop the looping click audio source to end any ongoing typing sound effect
            ClickLoopAudioSource.Stop();

            // Call EndLine to finalize the current dialog line and trigger any end-of-line logic
            EndLine();
        }

        /// <summary>
        /// Delays the highlight of the skip button.
        /// </summary>
        /// <returns>An enumerator that delays the highlight of the skip button.</returns>
        private IEnumerator DelayHighlightSkipButton()
        {
            yield return new WaitForEndOfFrame();

            // Highlight the skip button if it exists
            if (m_skipButton != null)
            {
                m_skipButton.Highlight();
            }
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
        /// Skips the text writing sequence.
        /// </summary>
        private void SkipWriteOut()
        {
            // Stop all coroutines
            StopAllCoroutines();

            // End the line
            EndLine();

            // If the refresh layout groups flag is set, refresh the layout groups
            if (m_refreshLayoutGroups)
            {
                Utility.RefreshLayoutGroupsImmediateAndRecursive(m_dialogRoot.gameObject);
                StartCoroutine(DelayedRefreshLayoutGroups());
            }
        }

        /// <summary>
        /// Delays the refresh of the layout groups.
        /// </summary>
        /// <returns>An enumerator that delays the refresh of the layout groups.</returns>
        private IEnumerator DelayedRefreshLayoutGroups()
        {
            yield return new WaitForEndOfFrame();
            Utility.RefreshLayoutGroupsImmediateAndRecursive(m_dialogRoot.gameObject);
        }

        /// <summary>
        /// Ends the line. Finalizes the current dialog line, updates UI elements to display the full text and responses,
        /// prepares inline images, and manages the visibility and highlighting of response or continue buttons.
        /// </summary>
        private void EndLine()
        {
            // Set the writing in progress flag to false
            m_writingInProgress = false;

            // If the speaker field exists, format the speaker field text and the text field text individually
            if (m_speakerField != null)
            {
                m_speakerField.text = FormatInlineText(m_speaker);
                PrepareInlineImages(ref m_speakerField, null);
                m_textField.text = FormatInlineText(m_text);
                PrepareInlineImages(ref m_textField, null);
            }
            else
            {
                // If the speaker field does not exist, format the text field text and include the speaker text within it
                m_textField.text = FormatInlineText(m_speaker) + FormatInlineText(m_text);
                PrepareInlineImages(ref m_textField, null);
            }

            // Make all characters visible
            m_textField.maxVisibleCharacters = int.MaxValue;

            // Invoke the on display line event
            m_onDisplayLine?.Invoke();

            // If there are response options available, show the response container and prepare inline images for each response
            if (m_responseContainer != null && m_responseContainer.childCount > 0)
            {
                // Show the response container so the user can select a response option
                m_responseContainer.gameObject.SetActive(true);

                // Iterate through each response option and prepare its inline images
                foreach (Transform child in m_responseContainer)
                {
                    TMP_Text text = child.GetComponentInChildren<TMP_Text>();
                    PrepareInlineImages(ref text, s_inlineImageReplacementPostProcessingInfo);
                }

                // Highlight the first response button by default
                m_responseContainer.GetChild(0).GetComponent<Button>().Highlight();
            }
            else
            {
                // If there are no responses, activate and highlight the continue button
                m_continueButton.gameObject.SetActive(true);
                m_continueButton.Highlight();
            }

            // Start a coroutine to refresh inline images after UI updates
            StartCoroutine(DelayedRefreshInlineImages());
        }

        /// <summary>
        /// Prepares and inserts inline images into the specified TMP_Text field by searching for special markers in the text.
        /// If a list of image replacement info is provided, it creates and positions image GameObjects at the locations of the markers.
        /// </summary>
        /// <param name="textField">The TMP_Text field to process and insert inline images into.</param>
        /// <param name="parsedText">The parsed text to search for image markers (used for positioning images).</param>
        /// <param name="info">A list to store information about each inline image replacement; if null, only text formatting is performed.</param>
        private static void PrepareInlineImages(ref TMP_Text textField, List<InlineImageReplacementPostProcessingInfo> info)
        {
            // Clear the static list of inline image replacement info and populate it with info from the DialogProcessor
            s_inlineImageReplacementInfo.Clear();
            DialogProcessor.Instance.GetInlineImageInfo(s_inlineImageReplacementInfo);

            // Track whether any inline image markers were found and replaced in the text
            bool foundAny = false;

            // Iterate through each inline image replacement info to process inline images in the text
            foreach (InlineImageReplacementInfo inlineInfo in s_inlineImageReplacementInfo)
            {
                string seeking = inlineInfo.textToReplace;
                string before = "<color=#FFFFFF00>";
                string after = "</color>";
                
                // Loop through the text to find all occurrences of the marker and wrap them in invisible color tags
                for (int i = 0; i <= textField.text.Length - seeking.Length; i++)
                {
                    if (textField.text.Substring(i, seeking.Length) != seeking)
                    {
                        continue;
                    }

                    textField.text = textField.text[..i] + before + seeking + after + textField.text[(i + seeking.Length)..];
                    i += before.Length + seeking.Length + after.Length;

                    foundAny = true;
                }

                // If no image replacement info list is provided, exit early
                if (info == null)
                {
                    continue;
                }

                // Force the text field to update its mesh so character positions are accurate
                textField.ForceMeshUpdate();

                string parsedText = textField.GetParsedText();

                // Loop through the parsed text to find all occurrences of the marker and create inline image GameObjects for each
                for (int i = 0; i <= parsedText.Length - seeking.Length; i++)
                {
                    // Check if the current substring matches the inline image marker; if not, skip to the next iteration
                    if (parsedText.Substring(i, seeking.Length) != seeking)
                    {
                        continue;
                    }

                    // Create a new GameObject for the inline image and set its transform properties
                    GameObject gameObject = new("Inline Image", typeof(RectTransform), typeof(Image));
                    gameObject.transform.SetParent(textField.canvas.transform);
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localEulerAngles = Vector3.zero;
                    gameObject.transform.localScale = Vector3.one;

                    // Configure the Image component and assign the sprite
                    Image image = gameObject.GetComponent<Image>();
                    image.enabled = false;
                    image.sprite = inlineInfo.sprite;

                    // Set up the RectTransform for the image
                    RectTransform rt = image.rectTransform;
                    rt.pivot = 0.5f * Vector2.one;
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.zero;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    rt.sizeDelta = inlineInfo.scale * image.sprite.rect.size;

                    // Add the image replacement info to the list for later positioning and management
                    info.Add(new()
                    {
                        text = seeking,
                        startIndex = i,
                        endIndex = i + (seeking.Length - 1),
                        textField = textField,
                        image = image,
                    });
                }
            }

            // Warn the user if inline images are being used on a Canvas that is not set to "Screen Space Camera" mode,
            // since inline images are only supported in that render mode.
            if (foundAny && textField.canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                Debug.LogWarning($"{nameof(DialogUI)} inline images are only compatible with Canvas render mode \"Screen Space Camera.\"");
            }
        }

        /// <summary>
        /// Waits until the end of the current frame, then refreshes the positions and visibility of all inline images in the dialog text.
        /// This ensures that the inline images are correctly aligned with the updated text mesh after UI changes.
        /// </summary>
        private static IEnumerator DelayedRefreshInlineImages()
        {
            yield return new WaitForEndOfFrame();
            RefreshInlineImages(int.MaxValue);
        }

        /// <summary>
        /// Updates the position and visibility of all inline images in the dialog text based on the currently revealed character index.
        /// For each inline image marker, this method:
        /// - Forces a mesh update on the associated text field to ensure character positions are current.
        /// - Enables the image if its marker's start index is less than or equal to the currently showing character index.
        /// - Calculates the world and viewport position of the marker in the text and positions the image accordingly.
        /// - Maintains the image's size.
        /// </summary>
        /// <param name="currentlyShowingIndex">The index of the last character currently visible in the dialog text.</param>
        private static void RefreshInlineImages(int currentlyShowingIndex)
        {
            for (int i = 0; i < s_inlineImageReplacementPostProcessingInfo.Count; i++)
            {
                InlineImageReplacementPostProcessingInfo info = s_inlineImageReplacementPostProcessingInfo[i];

                // Ensure the text mesh is up to date for accurate character positions
                info.textField.ForceMeshUpdate();

                // Enable the image if its marker is within the revealed text range
                info.image.enabled = info.startIndex <= currentlyShowingIndex;

                // Calculate the center position of the inline image marker in the text
                Vector3 bl = info.textField.textInfo.characterInfo[info.startIndex].bottomLeft;
                Vector3 tr = info.textField.textInfo.characterInfo[info.endIndex].topRight;
                Vector3 localPosition = Vector3.Lerp(bl, tr, 0.5f);
                Vector3 worldPosition = info.textField.transform.TransformPoint(localPosition);
                Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
                Vector2 viewportPosition = Camera.main.ScreenToViewportPoint(screenPosition);
                //Vector2 viewportPosition = Camera.main.WorldToViewportPoint(worldPosition, Camera.MonoOrStereoscopicEye.Mono);

                // Set the image's RectTransform to match the calculated position and maintain its size
                Vector2 sizeDelta = info.image.rectTransform.sizeDelta;
                info.image.rectTransform.anchorMin = viewportPosition;
                info.image.rectTransform.anchorMax = viewportPosition;
                info.image.rectTransform.offsetMin = Vector2.zero;
                info.image.rectTransform.offsetMax = Vector2.zero;
                info.image.rectTransform.sizeDelta = sizeDelta;
            }
        }

        /// <summary>
        /// Destroys all inline images currently displayed in the dialog text and clears their tracking information.
        /// </summary>
        private static void DestroyInlineImages()
        {
            for (int i = s_inlineImageReplacementPostProcessingInfo.Count - 1; i >= 0; i--)
            {
                if (s_inlineImageReplacementPostProcessingInfo[i].image != null)
                {
                    Destroy(s_inlineImageReplacementPostProcessingInfo[i].image.gameObject);
                }

                s_inlineImageReplacementPostProcessingInfo.RemoveAt(i);
            }
        }

        /// <summary>
        /// Delays the enabling of the continue handler.
        /// </summary>
        /// <returns>An enumerator that delays the enabling of the continue handler.</returns>
        private IEnumerator HandlerDelay()
        {
            // Wait for the handler delay
            yield return new WaitForSecondsRealtime(HANDLER_DELAY);

            // Enable the continue handler
            m_canUseContinueHandler = true;
        }

        /// <summary>
        /// Handles the click of a response button.
        /// </summary>
        /// <param name="response">The response button that was clicked.</param>
        private void OnClickResponse(Button response)
        {
            // Reset inline images
            DestroyInlineImages();

            // If the stop blast after text flag is set, stop the blast audio source
            if (m_stopBlastAfterText)
            {
                BlastAudioSource.Stop();
            }

            // If the button click sound is set, play it
            if (m_buttonClick != null)
            {
                ClickAudioSource.PlayOneShot(m_buttonClick);
            }

            // Invoke the line complete callback
            m_lineCompleteCallback?.Invoke(response == null ? null : m_line.responses[response.GetComponent<RectTransform>().GetSiblingIndex()].link);
        }

        /// <summary>
        /// Checks for the completion of the blast audio source.
        /// </summary>
        /// <returns>An enumerator that checks for the completion of the blast audio source.</returns>
        private IEnumerator CheckForBlastComplete()
        {
            // While the writing is not in progress and the blast audio source is playing, check for the completion of the blast audio source
            while (m_writingInProgress && BlastAudioSource.isPlaying)
            {
                // Wait for the end of the frame
                yield return new WaitForEndOfFrame();

                // If the writing is not in progress and the blast audio source is not playing, continue the line
                if (!m_writingInProgress && !BlastAudioSource.isPlaying)
                {
                    ContinueHandler();
                    break;
                }
            }
        }
        #endregion
    }
}