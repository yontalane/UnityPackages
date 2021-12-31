using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DEF.Dialog
{
    [DisallowMultipleComponent]
    [AddComponentMenu("DEF/Dialog/Dialog UI")]
    public class DialogUI : MonoBehaviour
    {
        [Serializable] public class GetAudioClipAction : UnityEvent<DialogAgent, LineData, Action<AudioClip>> { }
        [Serializable] public class GetSpriteAction : UnityEvent<DialogAgent, LineData, Action<Sprite>> { }

        private const string ANIMATION_PARAMETER = "Dialog Visible";
        private const float HANDLER_DELAY = 0.15f;

        [Header("Cosmetics")]
        [SerializeField] private Color m_playerColor = Color.white;
        [SerializeField] private Color m_speakerColor = Color.white;
        [SerializeField] private bool m_useTypeCharacterInterval = true;
        [SerializeField, Min(0f)] private float m_typeCharacterInterval = 0.05f;
        [SerializeField] private bool m_autoContinue = false;

        [Header("Scene UI")]
        [SerializeField] private Animator m_animator = null;
        [SerializeField] private Text m_textField = null;
        [SerializeField] private Button m_skipButton = null;
        [SerializeField] private Button m_continueButton = null;
        [SerializeField] private RectTransform m_responseContainer = null;
        [SerializeField] private RectTransform m_portraitContainer = null;
        [SerializeField] private Image m_portrait = null;
        [SerializeField] private AudioClip m_buttonClick = null;
        [SerializeField] private AudioClip m_buttonSelect = null;
        [SerializeField] private AudioClip m_typingDefault = null;
        [SerializeField] private bool m_stopBlastAfterText = true;

        [Header("Prefabs")]
        [SerializeField] private Button m_responseButtonPrefab = null;

        [Header("Overrides")]
        [SerializeField] private GetSpriteAction m_getPortrait = new GetSpriteAction();
        [SerializeField] private GetAudioClipAction m_getSound = new GetAudioClipAction();
        [SerializeField] private GetAudioClipAction m_getVoice = new GetAudioClipAction();

        [Header("Callbacks")]
        [SerializeField] private UnityEvent m_onType = new UnityEvent();

        private string m_speaker = "";
        private LineData m_line = null;
        private string m_text = null;
        private bool m_writingInProgress = false;
        private Action<string> m_lineCompleteCallback = null;
        private static AudioSource m_clickAudioSource = null;
        private static AudioSource m_blastAudioSource = null;
        private bool m_canUseContinueHandler = true;

        private void Start()
        {
            m_skipButton.onClick.AddListener(delegate { SkipWriteOut(); });
            m_continueButton.onClick.AddListener(delegate { OnClickResponse(null); });
            m_continueButton.gameObject.SetActive(false);
            if (m_responseContainer != null)
                m_responseContainer.gameObject.SetActive(false);
        }

        public void Close()
        {
            if (m_animator != null)
                m_animator.SetBool(ANIMATION_PARAMETER, false);
        }

        public void Initiate(LineData line, Action<string> lineCompleteCallback, Func<string, string> replaceInlineText)
        {
            m_continueButton.gameObject.SetActive(false);
            m_canUseContinueHandler = true;

            m_speaker = string.IsNullOrEmpty(line.speaker) ? "" : "<color=#" + ColorUtility.ToHtmlStringRGBA(GetSpeakerColor(line.speaker)) + ">" + replaceInlineText(line.speaker) + ": </color>";
            m_text = replaceInlineText(line.text);

            m_line = line;
            m_lineCompleteCallback = lineCompleteCallback;
            m_textField.text = "";

            if (m_portraitContainer != null && m_portrait != null)
            {
                Sprite portrait = null;
                m_getPortrait?.Invoke(DialogProcessor.Instance.DialogAgent, line, (sprite) => portrait = sprite);
                if (portrait == null) portrait = Resources.Load<Sprite>(line.portrait);
                if (portrait != null)
                {
                    m_portrait.sprite = portrait;
                    AspectRatioFitter fitter = m_portrait.GetComponent<AspectRatioFitter>();
                    if (fitter != null)
                        fitter.aspectRatio = portrait.rect.width / portrait.rect.height;
                    m_portraitContainer.gameObject.SetActive(true);
                }
                else
                    m_portraitContainer.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(line.sound))
            {
                AudioClip blast = null;
                m_getSound?.Invoke(DialogProcessor.Instance.DialogAgent, line, (clip) => blast = clip);
                if (blast == null) blast = Resources.Load<AudioClip>(line.sound);
                if (blast != null)
                {
                    BlastAudioSource.PlayOneShot(blast);
                    if (m_autoContinue)
                        StartCoroutine(CheckForBlastComplete());
                }
            }

            if (!string.IsNullOrEmpty(line.voice))
            {
                AudioClip blast = null;
                m_getVoice?.Invoke(DialogProcessor.Instance.DialogAgent, line, (clip) => blast = clip);
                if (blast == null) blast = Resources.Load<AudioClip>(line.voice);
                if (blast != null)
                {
                    BlastAudioSource.PlayOneShot(blast);
                    if (m_autoContinue)
                        StartCoroutine(CheckForBlastComplete());
                }
            }

            if (m_responseContainer != null)
            {
                for (int i = m_responseContainer.childCount - 1; i >= 0; i--)
                    Destroy(m_responseContainer.GetChild(i).gameObject);
                m_responseContainer.gameObject.SetActive(false);
                for (int i = 0; i < line.responses.Length; i++)
                {
                    Button instance = Instantiate(m_responseButtonPrefab.gameObject).GetComponent<Button>();
                    instance.onClick.AddListener(delegate
                    {
                        OnClickResponse(instance);
                    });
                    instance.GetComponentInChildren<Text>().text = FormatInlineText(replaceInlineText(line.responses[i].text));
                    instance.GetComponent<RectTransform>().SetParent(m_responseContainer);
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localScale = Vector3.one;
                    instance.navigation = Navigation.defaultNavigation;

                    if (i > 0)
                    {
                        Button prev = m_responseContainer.GetChild(i - 1).GetComponent<Button>();

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
                }
            }

            if (m_animator != null)
                m_animator.SetBool(ANIMATION_PARAMETER, true);
            m_writingInProgress = true;
            if (m_useTypeCharacterInterval)
                StartCoroutine(WriteOut());
            else
                EndLine();
        }

        private Color GetSpeakerColor(string speaker)
        {
            DialogProcessor.SpeakerType speakerType = DialogProcessor.GetSpeakerType(speaker);
            if (speakerType == DialogProcessor.SpeakerType.Player)
                return m_playerColor;
            else if (speakerType == DialogProcessor.SpeakerType.Self)
                return m_speakerColor;
            return Color.white;
        }

        private IEnumerator WriteOut()
        {
            AudioClip typing = null;
            if (!string.IsNullOrEmpty(m_line.typing))
                typing = Resources.Load<AudioClip>(m_line.typing);
            if (typing == null)
                typing = m_typingDefault;

            m_skipButton.Highlight();
            List<string> hangingTags = new List<string>();

            m_textField.text = "";
            string fullText = FormatInlineText(m_speaker);
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
                        m_textField.supportRichText = true;
                        if (interior.Equals("/color"))
                        {
                            fullText += "</color>";
                            hangingTags.Remove("</color>");
                        }
                        else if (interior.Contains("color"))
                        {
                            string[] colorParameter = interior.Split('=');
                            if (colorParameter.Length == 2 && colorParameter[0].Equals("color"))
                                fullText += ("<color=" + colorParameter[1] + ">");
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
                    for (int j = 0; j < hangingTags.Count; j++)
                        m_textField.text += hangingTags[j];
                    m_onType?.Invoke();
                    if (typing != null)
                        ClickAudioSource.PlayOneShot(typing);
                    yield return new WaitForSecondsRealtime(m_typeCharacterInterval);
                }
            }
            EndLine();
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
        }

        private void EndLine()
        {
            m_writingInProgress = false;
            m_textField.text = FormatInlineText(m_speaker) + FormatInlineText(m_text);
            if (m_responseContainer != null && m_responseContainer.childCount > 0)
            {
                m_responseContainer.gameObject.SetActive(true);
                if (m_responseContainer.childCount > 0)
                    m_responseContainer.GetChild(0).GetComponent<Button>().Highlight();
            }
            else
            {
                m_continueButton.gameObject.SetActive(true);
                m_continueButton.Highlight();
            }
        }

        public void SkipHandler()
        {
            if (m_skipButton != null && m_skipButton.gameObject.activeInHierarchy && m_continueButton != null && !m_continueButton.gameObject.activeSelf)
            {
                SkipWriteOut();
                m_canUseContinueHandler = false;
                if (gameObject.activeSelf)
                    StartCoroutine(HandlerDelay());
            }
        }

        private IEnumerator HandlerDelay()
        {
            yield return new WaitForSecondsRealtime(HANDLER_DELAY);
            m_canUseContinueHandler = true;
        }

        public void ContinueHandler()
        {
            if (m_canUseContinueHandler && m_continueButton != null && m_continueButton.gameObject.activeInHierarchy)
            {
                m_continueButton.gameObject.SetActive(false);
                OnClickResponse(null);
            }
        }

        private void OnClickResponse(Button response)
        {
            if (m_stopBlastAfterText)
                BlastAudioSource.Stop();

            if (m_buttonClick != null)
                ClickAudioSource.PlayOneShot(m_buttonClick);

            m_lineCompleteCallback.Invoke(response == null ? null : m_line.responses[response.GetComponent<RectTransform>().GetSiblingIndex()].link);
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
                if (m_clickAudioSource == null)
                {
                    m_clickAudioSource = (new GameObject()).AddComponent<AudioSource>();
                    m_clickAudioSource.playOnAwake = false;
                    m_clickAudioSource.loop = false;
                    m_clickAudioSource.transform.SetParent(Camera.main.transform, false);
                }
                return m_clickAudioSource;
            }
        }

        private static AudioSource BlastAudioSource
        {
            get
            {
                if (m_blastAudioSource == null)
                {
                    m_blastAudioSource = (new GameObject()).AddComponent<AudioSource>();
                    m_blastAudioSource.playOnAwake = false;
                    m_blastAudioSource.loop = false;
                    m_blastAudioSource.transform.SetParent(Camera.main.transform, false);
                }
                return m_blastAudioSource;
            }
        }
    }
}