using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Yontalane.Dialog
{
    public struct PortraitEvent
    {
        public IDialogAgent agent;
        public LineData data;
        public Image image;
        public string speaker;
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog/Dialog UI")]
    public sealed class DialogUI : MonoBehaviour
    {
        [Serializable]
        private struct ColorSet
        {
            public string speaker;
            public Color color;
        }

        [Serializable] public class GetAudioClipAction : UnityEvent<IDialogAgent, LineData, Action<AudioClip>> { }
        [Serializable] public class SetSpriteAction : UnityEvent<PortraitEvent> { }
        [Serializable] public class GetSpriteAction : UnityEvent<IDialogAgent, LineData, Action<Sprite>> { }

        private const string ANIMATION_PARAMETER = "Dialog Visible";
        private const float HANDLER_DELAY = 0.15f;

        private static readonly List<Button> s_tempResponseButtons = new();
        private static AudioSource s_clickAudioSource = null;
        private static AudioSource s_blastAudioSource = null;

        [Header("Cosmetics")]

        [SerializeField]
        [Tooltip("Color of player name in dialog header text.")]
        private Color m_playerColor = Color.white;
        [SerializeField]
        [Tooltip("Color of speaker name in dialog header text.")]
        private Color m_speakerColor = Color.white;
        [SerializeField]
        [Tooltip("Color of other speaker names in dialog header text.")]
        private ColorSet[] m_speakerColors = new ColorSet[0];
        [SerializeField]
        [Tooltip("Color of fallback speaker name in dialog header text.")]
        private Color m_fallbackSpeakerColor = Color.white;
        [SerializeField]
        [Tooltip("Whether or not the speaker name is bold.")]
        private bool m_speakerBold = false;
        [SerializeField]
        [Tooltip("Whether or not the speaker name is in caps.")]
        private bool m_speakerCaps = false;
        [SerializeField]
        [Tooltip("The string to appear between the speaker name and dialog text.")]
        private string m_speakerSeparator = ": ";
        [SerializeField]
        [Tooltip("Line breaks between the speaker name and the dialog text.")]
        [Range(0, 2)]
        private int m_speakerLineBreaks = 0;
        [SerializeField]
        [Tooltip("Whether to write out text character by character.")]
        private bool m_useTypeCharacterInterval = true;
        [SerializeField]
        [Min(0f)]
        [Tooltip("How long to wait between characters when writing out text.")]
        private float m_typeCharacterInterval = 0.05f;
        [SerializeField]
        [Tooltip("Whether to continue onto the next line immediately after finishing the current. Otherwise, wait for player input.")]
        private bool m_autoContinue = false;

        [Header("Scene UI")]

        [SerializeField]
        [Tooltip("The root container of the dialog UI. Used to recursively refresh child layout groups.")]
        private RectTransform m_dialogRoot = null;
        [SerializeField]
        [Tooltip("Whether to force layout groups to refresh while typing.")]
        private bool m_refreshLayoutGroups = true;
        [SerializeField]
        [Tooltip("The Animator that controls the dialog UI.")]
        private Animator m_animator = null;
        [SerializeField]
        [Tooltip("The field for displaying dialog text.")]
        private TMP_Text m_textField = null;
        [SerializeField]
        [Tooltip("The optional field for displaying speaker's name. If left blank, the speaker's name will be displayed in the main text field.")]
        private TMP_Text m_speakerField = null;
        [SerializeField]
        [Tooltip("Button for skipping text writing sequence. May be the same as the continue button.")]
        private Button m_skipButton = null;
        [SerializeField]
        [Tooltip("Button for proceeding to the next line of dialog. May be the same as the skip button.")]
        private Button m_continueButton = null;
        [Tooltip("Button for rewinding to the previous line of dialog.")]
        private Button m_rewindButton = null;
        [SerializeField]
        [Tooltip("The object within which dialog response button prefabs will be instantiated.")]
        private RectTransform m_responseContainer = null;
        [SerializeField]
        [Tooltip("The object that contains the portrait. Having a reference to both the container and the portrait helps with layout.")]
        private RectTransform m_portraitContainer = null;
        [SerializeField]
        [Tooltip("The Image that will display the speaker's portrait (if there is one).")]
        private Image m_portrait = null;
        [SerializeField]
        [Tooltip("Sound effect for clicking a response button.")]
        private AudioClip m_buttonClick = null;
        [SerializeField]
        [Tooltip("Sound effect for clicking the rewind button.")]
        private AudioClip m_rewindClick = null;
        [SerializeField]
        [Tooltip("Sound effect for typing during the text writing sequence.")]
        private AudioClip m_typingDefault = null;
        [SerializeField]
        [Tooltip("If a line of text has an accompanying sound blast, whether or not to cut the blast short if the text finishes before the sound does.")]
        private bool m_stopBlastAfterText = true;

        [Header("Prefabs")]

        [SerializeField]
        [Tooltip("The prefab to use for responses.")]
        private Button m_responseButtonPrefab = null;

        [Header("Overrides")]

        [SerializeField]
        [Tooltip("A callback that can be used to set a custom portrait instead of using the one requested by the dialog data. SetPortrait overrides GetPortrait, which in turn overrides the dialog data.")]
        private SetSpriteAction m_setPortrait = new SetSpriteAction();
        [Tooltip("A callback that can be used to get a custom portrait instead of the one requested by the dialog data. SetPortrait overrides GetPortrait, which in turn overrides the dialog data.")]
        private GetSpriteAction m_getPortrait = new GetSpriteAction();
        [SerializeField]
        [Tooltip("A callback that can be used to get a custom sound instead of the one requested by the dialog data.")]
        private GetAudioClipAction m_getSound = new GetAudioClipAction();
        [SerializeField]
        [Tooltip("A callback that can be used to get a custom voice instead of the one requested by the dialog data.")]
        private GetAudioClipAction m_getVoice = new GetAudioClipAction();

        [Header("Callbacks")]

        [SerializeField]
        [Tooltip("An action that gets invoked when the dialog starts being typed out.")]
        private UnityEvent m_onStartTyping = new UnityEvent();

        [SerializeField]
        [Tooltip("An action that gets invoked while the dialog is being typed out.")]
        private UnityEvent m_onType = new UnityEvent();

        [SerializeField]
        [Tooltip("An action that gets invoked when the dialog is fully typed out.")]
        private UnityEvent m_onDisplayLine = new UnityEvent();

        private string m_speaker = "";
        private LineData m_line = null;
        private string m_text = null;
        private bool m_writingInProgress = false;
        private Action<string> m_lineCompleteCallback = null;
        private Action m_rewindCallback = null;
        private bool m_canUseContinueHandler = true;
        private bool m_canUseRewindHandler = true;

        public static DialogUI Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            m_skipButton.onClick.AddListener(delegate { SkipWriteOut(); });
            m_continueButton.onClick.AddListener(delegate { OnClickResponse(null); });
            m_continueButton.gameObject.SetActive(false);

            if (m_rewindButton != null)
            {
                m_rewindButton.onClick.AddListener(delegate { OnClickRewind(); });
                m_rewindButton.gameObject.SetActive(false);
            }

            if (m_responseContainer != null)
            {
                m_responseContainer.gameObject.SetActive(false);
            }
        }

        public void Close()
        {
            if (m_animator != null)
            {
                m_animator.SetBool(ANIMATION_PARAMETER, false);
            }
        }

        public void Initiate(LineData line, Action<string> lineCompleteCallback, Action rewindCallback, Func<string, string> replaceInlineText)
        {
            m_continueButton.gameObject.SetActive(false);

            if (m_rewindButton != null)
            {
                m_rewindButton.gameObject.SetActive(false);
            }

            m_canUseContinueHandler = true;
            m_canUseRewindHandler = true;
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
            m_rewindCallback = rewindCallback;
            m_textField.text = "";

            if (m_speakerField != null)
            {
                m_speakerField.text = string.Empty;
            }

            if (m_portraitContainer != null && m_portrait != null)
            {
                if (m_setPortrait != null)
                {
                    m_setPortrait.Invoke(new()
                    {
                        agent = DialogProcessor.Instance.DialogAgent,
                        data = line,
                        image = m_portrait,
                        speaker = speaker,
                    });
                    m_portraitContainer.gameObject.SetActive(m_portrait.sprite != null);
                }
                else
                {
                    Sprite portrait = null;
                    m_getPortrait?.Invoke(DialogProcessor.Instance.DialogAgent, line, (sprite) => portrait = sprite);
                    if (portrait == null)
                    {
                        portrait = Resources.Load<Sprite>(line.portrait);
                    }

                    if (portrait != null)
                    {
                        m_portrait.sprite = portrait;
                        AspectRatioFitter fitter = m_portrait.GetComponent<AspectRatioFitter>();
                        if (fitter != null)
                        {
                            fitter.aspectRatio = portrait.rect.width / portrait.rect.height;
                        }

                        m_portraitContainer.gameObject.SetActive(true);
                    }
                    else
                    {
                        m_portraitContainer.gameObject.SetActive(false);
                    }
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

        private Color GetSpeakerColor(string speaker)
        {
            DialogProcessor.SpeakerType speakerType = DialogProcessor.GetSpeakerType(speaker);
            switch (speakerType)
            {
                case DialogProcessor.SpeakerType.Player:
                    return m_playerColor;
                case DialogProcessor.SpeakerType.Self:
                    return m_speakerColor;
            }
            foreach(ColorSet colorSet in m_speakerColors)
            {
                if (speaker.Contains(colorSet.speaker))
                {
                    return colorSet.color;
                }
            }
            return m_fallbackSpeakerColor;
        }

        private IEnumerator WriteOut()
        {
            AudioClip typing = null;

            if (!string.IsNullOrEmpty(m_line.typing))
            {
                typing = Resources.Load<AudioClip>(m_line.typing);
            }

            if (typing == null)
            {
                typing = m_typingDefault;
            }

            StartCoroutine(DelayHighlightSkipButton());
            List<string> hangingTags = new List<string>();

            bool speakerFieldExists = m_speakerField != null;
            if (speakerFieldExists)
            {
                m_speakerField.text = m_speaker;
            }

            m_textField.text = "";
            string fullText = FormatInlineText(speakerFieldExists ? string.Empty : m_speaker);
            for (int i = 0; i < m_text.Length; i++)
            {
                bool showNextCharacter = true;
                string nextCharacter = m_text.Substring(i, 1);
                if (nextCharacter.Equals("%") && i < m_text.Length - 4)
                {
                    int j = i + 2;
                    int indexOfClose = m_text.Substring(j).IndexOf("%%");
                    if (indexOfClose >= 0)
                    {
                        indexOfClose += j;
                        i = indexOfClose + 1;
                        string interior = m_text.Substring(j, i - j - 1);
                        m_textField.richText = true;
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
                                fullText += ("<color=" + colorParameter[1] + ">");
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
                if (showNextCharacter)
                {
                    fullText += nextCharacter;
                    m_textField.text = fullText;
                    foreach (string hangingTag in hangingTags)
                    {
                        m_textField.text += hangingTag;
                    }
                    m_onType?.Invoke();
                    if (typing != null)
                    {
                        ClickAudioSource.PlayOneShot(typing);
                    }
                    if (m_refreshLayoutGroups)
                    {
                        Utility.RefreshLayoutGroupsImmediateAndRecursive(m_dialogRoot.gameObject);
                    }
                    yield return new WaitForSecondsRealtime(m_typeCharacterInterval);
                }
            }
            EndLine();
        }

        private IEnumerator DelayHighlightSkipButton()
        {
            yield return new WaitForEndOfFrame();
            if (m_skipButton != null)
            {
                m_skipButton.Highlight();
            }
        }

        private string FormatInlineText(string text)
        {
            bool isOpen = true;

            while (text.Contains("%%"))
            {
                int i = text.IndexOf("%%");
                text = text.Substring(0, i) + (isOpen ? "<" : ">") + text.Substring(i + 2);
                isOpen = !isOpen;
            }

            return text;
        }

        private void SkipWriteOut()
        {
            StopAllCoroutines();
            EndLine();

            if (m_refreshLayoutGroups)
            {
                Utility.RefreshLayoutGroupsImmediateAndRecursive(m_dialogRoot.gameObject);
                StartCoroutine(DelayedRefreshLayoutGroups());
            }
        }

        private IEnumerator DelayedRefreshLayoutGroups()
        {
            yield return new WaitForEndOfFrame();
            Utility.RefreshLayoutGroupsImmediateAndRecursive(m_dialogRoot.gameObject);
        }

        private void EndLine()
        {
            m_writingInProgress = false;

            if (m_speakerField != null)
            {
                m_speakerField.text = FormatInlineText(m_speaker);
                m_textField.text = FormatInlineText(m_text);
            }
            else
            {
                m_textField.text = FormatInlineText(m_speaker) + FormatInlineText(m_text);
            }

            m_onDisplayLine?.Invoke();

            if (m_responseContainer != null && m_responseContainer.childCount > 0)
            {
                m_responseContainer.gameObject.SetActive(true);
                if (m_responseContainer.childCount > 0)
                {
                    m_responseContainer.GetChild(0).GetComponent<Button>().Highlight();
                }
            }
            else
            {
                m_continueButton.gameObject.SetActive(true);
                m_continueButton.Highlight();

                if (m_rewindButton != null)
                {
                    m_rewindButton.gameObject.SetActive(true);
                }
            }
        }

        internal void SkipHandler()
        {
            if (m_skipButton != null
                && m_skipButton.gameObject.activeInHierarchy
                && m_continueButton != null
                && !m_continueButton.gameObject.activeSelf)
            {
                SkipWriteOut();
                m_canUseContinueHandler = false;
                m_canUseRewindHandler = false;
                if (gameObject.activeSelf)
                {
                    StartCoroutine(HandlerDelay());
                }
            }
        }

        private IEnumerator HandlerDelay()
        {
            yield return new WaitForSecondsRealtime(HANDLER_DELAY);
            m_canUseContinueHandler = true;
            m_canUseRewindHandler = true;
        }

        internal void ContinueHandler()
        {
            if (m_canUseContinueHandler
                && m_continueButton != null
                && m_continueButton.gameObject.activeInHierarchy)
            {
                m_continueButton.gameObject.SetActive(false);
                OnClickResponse(null);
            }
        }

        internal void RewindHandler()
        {
            if (m_canUseRewindHandler
                && m_rewindButton != null
                && m_rewindButton.gameObject.activeInHierarchy)
            {
                m_rewindButton.gameObject.SetActive(false);
                OnClickRewind();
            }
        }

        private void OnClickResponse(Button response)
        {
            if (m_stopBlastAfterText)
            {
                BlastAudioSource.Stop();
            }

            if (m_buttonClick != null)
            {
                ClickAudioSource.PlayOneShot(m_buttonClick);
            }

            m_lineCompleteCallback?.Invoke(response == null ? null : m_line.responses[response.GetComponent<RectTransform>().GetSiblingIndex()].link);
        }

        private void OnClickRewind()
        {
            if (m_stopBlastAfterText)
            {
                BlastAudioSource.Stop();
            }

            if (m_rewindClick != null)
            {
                ClickAudioSource.PlayOneShot(m_rewindClick);
            }

            m_rewindCallback?.Invoke();
        }

        private IEnumerator CheckForBlastComplete()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (!m_writingInProgress && !BlastAudioSource.isPlaying)
                {
                    ContinueHandler();
                    break;
                }
            }
        }

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
    }
}