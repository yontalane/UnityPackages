using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yontalane.Query;

namespace Yontalane.QueryUGUI
{
    /// <summary>
    /// Handles the display and interaction logic for query dialogs, including showing prompts,
    /// managing response buttons, and invoking events when a query is loaded or a response is chosen.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Query UGUI/Query UI")]
    public sealed class QueryUI : Singleton<QueryUI>, IQueryUI
    {
        #region Delegates
        /// <summary>
        /// Delegate for handling events related to the QueryUI, such as when a query UI is loaded.
        /// </summary>
        /// <param name="queryUI">The QueryUI instance involved in the event.</param>
        public delegate void QueryUIHandler(QueryUI queryUI);

        /// <summary>
        /// Event triggered when a QueryUI is loaded and ready.
        /// </summary>
        public static QueryUIHandler OnQueryUILoaded = null;
        #endregion

        #region Constants
        private const string ANIMATION_PARAMETER = "Query Visible";
        private const float BUTTON_HIGHLIGHT_DELAY = 0.25f;
        #endregion

        #region Serialized Fields
        [Header("Config")]

        [SerializeField]
        [Tooltip("How to show the dialog.")]
        private ShowType m_showType = ShowType.Animator;

        [Header("Scene UI")]

        [SerializeField]
        [Tooltip("The root object to show and hide. (Assuming showType is \"SetActive.\")")]
        private GameObject m_rootObject = null;

        [SerializeField]
        [Tooltip("The Animator for controlling the dialog. Must have a \"Query Visible\" boolean parameter. (Assuming showType is \\\"Animator.\\\")\")")]
        private Animator m_animator = null;

        [SerializeField]
        [Tooltip("The field for displaying the dialog's message text.")]
        private TMP_Text m_text = null;

        [SerializeField]
        [Tooltip("An optional field for displaying the dialog's description text.")]
        private TMP_Text m_description = null;

        [SerializeField]
        [Tooltip("The location to instantiate response buttons.")]
        private RectTransform m_responseContainer = null;

        [SerializeField]
        [Tooltip("An optional audio clip to play when clicking buttons.")]
        private AudioClip m_buttonClick = null;

        [Header("Prefabs")]

        [SerializeField]
        [Tooltip("The prefab to use for the primary response button.")]
        private Button m_primaryResponseButtonPrefab = null;

        [SerializeField]
        [Tooltip("The prefab to use for the response buttons.")]
        private Button m_responseButtonPrefab = null;
        #endregion

        #region Private Variables
        private static AudioSource s_clickAudioSource = null;

        private Action<QueryEventData> m_callback = null;
        private Action<QueryEventData> m_selectCallback = null;
        private readonly List<Button> m_responses = new();
        private string[] m_responsesText;

        private bool m_isOn = false;
        #endregion

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            if (m_animator == null) m_animator = GetComponent<Animator>();
            if (m_text == null) m_text = GetComponentInChildren<TMP_Text>();

            if (m_showType == ShowType.SetActive && m_rootObject != null)
            {
                m_rootObject.SetActive(false);
            }
        }

        public void Initialize(string text, string description, string[] responses, int initialSelection, Action<QueryEventData> callback, Action<QueryEventData> selectCallback)
        {
            m_callback = callback;
            m_selectCallback = selectCallback;
            m_text.text = text;

            // Set the description text if the description field exists.
            if (m_description != null)
            {
                m_description.text = description;
            }

            // Remove all existing response buttons and their listeners.
            for (int i = m_responses.Count - 1; i >= 0; i--)
            {
                if (m_responses[i].TryGetComponent(out SelectableListener selectableListener))
                {
                    selectableListener.OnChangeSelection -= OnSelectResponse;
                }
                Destroy(m_responses[i].gameObject);
            }
            m_responses.Clear();

            // Store the new set of response texts.
            m_responsesText = responses;

            // Create new response buttons for each response option.
            for (int i = 0; i < responses.Length; i++)
            {
                // Use the primary response button prefab for the first response if available, otherwise use the default.
                Button prefab = i == 0 && m_primaryResponseButtonPrefab != null ? m_primaryResponseButtonPrefab : m_responseButtonPrefab;
                Button instance = Instantiate(prefab.gameObject).GetComponent<Button>();
                instance.GetComponentInChildren<TMP_Text>().text = responses[i];
                instance.name = i.ToString();
                instance.onClick.AddListener(delegate { OnClickResponse(instance); });
                instance.GetComponent<RectTransform>().SetParent(m_responseContainer);
                instance.gameObject.AddComponent<SelectableListener>().OnChangeSelection += OnSelectResponse;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localEulerAngles = Vector3.zero;
                instance.transform.localScale = Vector3.one;
                Navigation nav = Navigation.defaultNavigation;
                nav.mode = Navigation.Mode.Explicit;
                instance.navigation = nav;

                // Set up explicit navigation between buttons for keyboard/controller support.
                if (i > 0)
                {
                    Button prev = m_responses[i - 1];

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

                m_responses.Add(instance);
            }

            // Highlight the initial selection and start a coroutine to ensure highlight is set.
            if (responses.Length > 0)
            {
                initialSelection = initialSelection < 0 ? 0 : initialSelection >= responses.Length ? responses.Length - 1 : initialSelection;
                m_responses[initialSelection].GetComponent<Button>().Highlight();
                StartCoroutine(DelayedHighlight(this, initialSelection));
            }

            // Show the query UI using the appropriate method (Animator or SetActive).
            if (m_showType == ShowType.Animator && m_animator != null)
            {
                m_animator.SetBool(ANIMATION_PARAMETER, true);
            }
            else if (m_showType == ShowType.SetActive && m_rootObject != null)
            {
                m_rootObject.SetActive(true);
            }


            // Notify listeners that the QueryUI has loaded.
            OnQueryUILoaded?.Invoke(this);

            // Refresh the layout groups to ensure UI updates immediately.
            if (m_rootObject != null)
            {
                Utility.RefreshLayoutGroupsImmediateAndRecursive(m_rootObject);
            }
            else
            {
                Utility.RefreshLayoutGroupsImmediateAndRecursive(m_responseContainer.gameObject);
            }

            // Mark the QueryUI as active.
            m_isOn = true;
        }

        /// <summary>
        /// Close the window. QueryUI relies on the Animator to do the closing.
        /// </summary>
        public void Close()
        {
            if (m_showType == ShowType.Animator && m_animator != null)
            {
                m_animator.SetBool(ANIMATION_PARAMETER, false);
            }
            else if (m_showType == ShowType.SetActive && m_rootObject != null)
            {
                m_rootObject.SetActive(false);
            }

            m_isOn = false;
        }

        /// <summary>
        /// In case the animation involves deactivating the buttons...
        /// </summary>
        private IEnumerator DelayedHighlight(QueryUI queryUI, int index)
        {
            yield return new WaitForSeconds(BUTTON_HIGHLIGHT_DELAY);

            if (queryUI.m_responses.Count > index)
            {
                queryUI.m_responses[index].GetComponent<Button>().Highlight();
            }
        }

        private void OnSelectResponse(Button response)
        {
            m_selectCallback?.Invoke(new QueryEventData()
            {
                prompt = m_text.text,
                description = m_description != null ? m_description.text : string.Empty,
                responses = m_responsesText,
                chosenResponse = m_responsesText[int.Parse(response.name)],
            });
        }

        public void OnClickResponse(string response)
        {
            if (!m_isOn)
            {
                return;
            }

            if (response == null) return;

            if (m_buttonClick != null) ClickAudioSource.PlayOneShot(m_buttonClick);

            Close();

            m_callback?.Invoke(new QueryEventData()
            {
                prompt = m_text.text,
                description = m_description != null ? m_description.text : string.Empty,
                responses = m_responsesText,
                chosenResponse = response,
            });
        }

        public void OnClickResponse(Button response)
        {
            if (response == null)
            {
                return;
            }

            OnClickResponse(response.GetComponentInChildren<TMP_Text>().text);
        }

        private static AudioSource ClickAudioSource
        {
            get
            {
                if (s_clickAudioSource == null)
                {
                    s_clickAudioSource = new GameObject().AddComponent<AudioSource>();
                    s_clickAudioSource.name = $"{typeof(QueryUI).Name} {typeof(AudioSource).Name}";
                    s_clickAudioSource.playOnAwake = false;
                    s_clickAudioSource.loop = false;
                    s_clickAudioSource.transform.SetParent(Camera.main.transform, false);
                }

                return s_clickAudioSource;
            }
        }
    }
}