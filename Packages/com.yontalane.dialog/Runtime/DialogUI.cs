using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Yontalane.Dialog
{
    #region Data Structures
    /// <summary>
    /// Represents an event containing information about a dialog portrait, including the agent, line data, image, and speaker.
    /// </summary>
    public struct PortraitEvent
    {
        [Tooltip("The dialog agent associated with this portrait event.")]
        public IDialogAgent agent;

        [Tooltip("The line data associated with this portrait event.")]
        public LineData data;

        [Tooltip("The UI Image component displaying the portrait.")]
        public Image image;

        [Tooltip("The name of the speaker for this portrait event.")]
        public string speaker;
    }
    #endregion

    /// <summary>
    /// Main UI controller for displaying and managing dialog interactions, including speaker display, text typing, responses, and associated UI elements.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog/Dialog UI")]
    public sealed class DialogUI : Singleton<DialogUI>
    {
        #region Data Structures
        /// <summary>
        /// Represents a set of color values for a specific speaker in the dialog.
        /// </summary>
        [Serializable]
        private struct ColorSet
        {
            [Tooltip("The name of the speaker for this color set.")]
            public string speaker;

            [Tooltip("The color value to use for this speaker.")]
            public Color color;
        }
        #endregion

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
        private SetSpriteAction m_setPortrait = new SetSpriteAction();

        [Tooltip("A callback that can be used to get a custom portrait instead of the one requested by the dialog data. SetPortrait overrides GetPortrait, which in turn overrides the dialog data.")]
        [SerializeField]
        private GetSpriteAction m_getPortrait = new GetSpriteAction();

        [Tooltip("A callback that can be used to get a custom sound instead of the one requested by the dialog data.")]
        [SerializeField]
        private GetAudioClipAction m_getSound = new GetAudioClipAction();

        [Tooltip("A callback that can be used to get a custom voice instead of the one requested by the dialog data.")]
        [SerializeField]
        private GetAudioClipAction m_getVoice = new GetAudioClipAction();

        [Header("Callbacks")]

        [Tooltip("An action that gets invoked when the dialog starts being typed out.")]
        [SerializeField]
        private UnityEvent m_onStartTyping = new UnityEvent();

        [Tooltip("An action that gets invoked while the dialog is being typed out.")]
        [SerializeField]
        private UnityEvent m_onType = new UnityEvent();

        [Tooltip("An action that gets invoked when the dialog is fully typed out.")]
        [SerializeField]
        private UnityEvent m_onDisplayLine = new UnityEvent();

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
        /// Closes the dialog UI.
        /// </summary>
        public void Close()
        {
            if (m_animator != null)
            {
                m_animator.SetBool(ANIMATION_PARAMETER, false);
            }
        }

        /// <summary>
        /// Initializes the dialog UI with the given line data.
        /// </summary>
        /// <param name="line">The line data to initialize the dialog UI with.</param>
        /// <param name="lineCompleteCallback">The callback to invoke when the line is complete.</param>
        public void Initiate(LineData line, Action<string> lineCompleteCallback, Func<string, string> replaceInlineText)
        {
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

            if (m_responseContainer != null)
            {
                for (int i = m_responseContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(m_responseContainer.GetChild(i).gameObject);
                }

                m_responseContainer.gameObject.SetActive(false);
                s_tempResponseButtons.Clear();

                for (int i = 0; i < line.responses.Length; i++)
                {
                    Button instance = Instantiate(m_responseButtonPrefab.gameObject).GetComponent<Button>();
                    instance.onClick.AddListener(delegate
                    {
                        OnClickResponse(instance);
                    });
                    instance.GetComponentInChildren<TMP_Text>().text = FormatInlineText(replaceInlineText(line.responses[i].text));
                    instance.GetComponent<RectTransform>().SetParent(m_responseContainer);
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localEulerAngles = Vector3.zero;
                    instance.transform.localScale = Vector3.one;
                    instance.navigation = Navigation.defaultNavigation;

                    s_tempResponseButtons.Add(instance);

                    if (i == 0) continue;

                    Button prev = s_tempResponseButtons[^2];

                    Navigation currNav = instance.navigation;
                    currNav.mode = Navigation.Mode.Explicit;
                    currNav.selectOnLeft = prev;
                    currNav.selectOnUp = prev;
                    instance.navigation = currNav;

                    Navigation prevNav = prev.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    prevNav.selectOnRight = instance;
                    prevNav.selectOnDown = instance;
                    prev.navigation = prevNav;
                }

                s_tempResponseButtons.Clear();
            }

            if (m_animator != null)
            {
                m_animator.SetBool(ANIMATION_PARAMETER, true);
            }

            m_writingInProgress = true;
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
            DialogProcessor.SpeakerType speakerType = DialogProcessor.GetSpeakerType(speaker);

            // Return the color of the speaker's name in the dialog header text based on the speaker type
            switch (speakerType)
            {
                case DialogProcessor.SpeakerType.Player:
                    return m_playerColor;
                case DialogProcessor.SpeakerType.Self:
                    return m_speakerColor;
            }

            // Return the color of the speaker's name in the dialog header text based on the speaker colors
            foreach(ColorSet colorSet in m_speakerColors)
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

            // Length in seconds of the typing sound
            float typingSoundLength = typing != null ? typing.length : 0f;

            // The time when the last character was typed
            float lastTypedTime = Time.time - 10f;

            // Start a coroutine to delay the highlight of the skip button
            StartCoroutine(DelayHighlightSkipButton());

            // Create a list to store the hanging tags
            List<string> hangingTags = new();

            // Check if the speaker field exists
            bool speakerFieldExists = m_speakerField != null;

            // If the speaker field exists, set the speaker field text to the speaker
            if (speakerFieldExists)
            {
                m_speakerField.text = m_speaker;
            }

            // Set the text field text to empty
            m_textField.text = string.Empty;

            // Format the inline text
            string fullText = FormatInlineText(speakerFieldExists ? string.Empty : m_speaker);

            // If the line has a typing loop sound, play it
            if (typingLoop != null)
            {
                ClickLoopAudioSource.clip = typingLoop;
                ClickLoopAudioSource.Play();
            }

            // Write out the text character by character
            for (int i = 0; i < m_text.Length; i++)
            {
                // Check if the next character is a special character
                bool showNextCharacter = true;
                string nextCharacter = m_text.Substring(i, 1);

                // If the next character is a special character, handle it
                if (nextCharacter.Equals("%") && i < m_text.Length - 4)
                {
                    // Find the index of the closing special character
                    int j = i + 2;
                    int indexOfClose = m_text[j..].IndexOf("%%");

                    // If the closing special character is found, handle it
                    if (indexOfClose >= 0)
                    {
                        indexOfClose += j;
                        i = indexOfClose + 1;

                        // Get the interior of the special character
                        string interior = m_text[j..i];

                        // Set the text field rich text to true
                        m_textField.richText = true;

                        // Handle interior tags
                        if (interior.Equals("/color"))
                        {
                            fullText += "</color>";
                            hangingTags.Remove("</color>");
                        }
                        else if (interior.Contains("color"))
                        {
                            string[] colorParameter = interior.Split('=');
                            if (colorParameter.Length == 2 && colorParameter[0].Equals("color"))
                            {
                                fullText += "<color=" + colorParameter[1] + ">";
                            }
                            hangingTags.Insert(0, "</color>");
                        }
                        else if (interior.Equals("b"))
                        {
                            fullText += "<b>";
                            hangingTags.Insert(0, "</b>");
                        }
                        else if (interior.Equals("/b"))
                        {
                            fullText += "</b>";
                            hangingTags.Remove("</b>");
                        }
                        else if (interior.Equals("i"))
                        {
                            fullText += "<i>";
                            hangingTags.Insert(0, "</i>");
                        }
                        else if (interior.Equals("/i"))
                        {
                            fullText += "</i>";
                            hangingTags.Remove("</i>");
                        }
                        showNextCharacter = false;
                    }
                }

                // If the next character is not a special character, handle it
                if (showNextCharacter)
                {
                    fullText += nextCharacter;
                    m_textField.text = fullText;

                    // Add the hanging tags to the text field
                    foreach (string hangingTag in hangingTags)
                    {
                        m_textField.text += hangingTag;
                    }

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

                    // Wait for the next character
                    yield return new WaitForSecondsRealtime(m_typeCharacterInterval);
                }
            }

            ClickLoopAudioSource.Stop();

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
        private string FormatInlineText(string text)
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
        /// Ends the line.
        /// </summary>
        private void EndLine()
        {
            // Set the writing in progress flag to false
            m_writingInProgress = false;

            // If the speaker field exists, format the speaker field text and the text field text individually
            if (m_speakerField != null)
            {
                m_speakerField.text = FormatInlineText(m_speaker);
                m_textField.text = FormatInlineText(m_text);
            }
            else
            {
                // If the speaker field does not exist, format the text field text and include the speaker text within it
                m_textField.text = FormatInlineText(m_speaker) + FormatInlineText(m_text);
            }

            // Invoke the on display line event
            m_onDisplayLine?.Invoke();

            // If the response container exists and has children, activate the response container and highlight the first child
            if (m_responseContainer != null && m_responseContainer.childCount > 0)
            {
                m_responseContainer.gameObject.SetActive(true);
                m_responseContainer.GetChild(0).GetComponent<Button>().Highlight();
            }
            else
            {
                // If there is no response, activate the continue button and highlight it
                m_continueButton.gameObject.SetActive(true);
                m_continueButton.Highlight();
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