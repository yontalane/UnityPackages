using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.Query
{
    #region Structs
    public struct QueryEventData
    {
        public string prompt;
        public string description;
        public string[] responses;
        public string chosenResponse;
        public string queryId;
    }
    #endregion

    /// <summary>
    /// Handles the display and interaction logic for query dialogs, including showing prompts,
    /// managing response buttons, and invoking events when a query is loaded or a response is chosen.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Query/Query UI")]
    public sealed class QueryUI : MonoBehaviour
    {
        #region Structs
        [Serializable]
        /// <summary>
        /// Specifies how the query dialog should be shown or hidden.
        /// </summary>
        private enum ShowType
        {
            None = 0,
            Animator = 1,
            SetActive = 2,
        }
        #endregion

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

        #region Accessors
        public static QueryUI Instance { get; private set; }
        public string Id { get; private set; } = string.Empty;
        #endregion

        #region Private Variables
        private static AudioSource s_clickAudioSource = null;

        private Action<QueryEventData> m_callback = null;
        private Action<QueryEventData> m_selectCallback = null;
        private readonly List<Button> m_responses = new List<Button>();
        private string[] m_responsesText;
        private static Action<string> s_stringCallback;

        private bool m_isOn = false;
        #endregion

        private void Awake()
        {
            Instance = this;
            s_stringCallback = null;
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
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="initialSelection">The index of the initially selected response.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void Initiate(string id, string text, string[] responses, int initialSelection, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null)
        {
            Initiate(id, text, string.Empty, responses, initialSelection, callback, selectCallback);
        }

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="description">The query description text.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="initialSelection">The index of the initially selected response.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void Initiate(string id, string text, string description, string[] responses, int initialSelection, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null)
        {
            // Get the singleton instance of QueryUI.
            QueryUI queryUI = Instance;

            // If the QueryUI instance is not found, log an error and exit.
            if (queryUI == null)
            {
                Debug.LogError("QueryUI could not be found.");
                return;
            }

            // Set the query ID and assign the provided callbacks and main text.
            queryUI.Id = id;
            queryUI.m_callback = callback;
            queryUI.m_selectCallback = selectCallback;
            queryUI.m_text.text = text;

            // Set the description text if the description field exists.
            if (queryUI.m_description != null)
            {
                queryUI.m_description.text = description;
            }

            // Remove all existing response buttons and their listeners.
            for (int i = queryUI.m_responses.Count - 1; i >= 0; i--)
            {
                if (queryUI.m_responses[i].TryGetComponent(out SelectableListener selectableListener))
                {
                    selectableListener.OnChangeSelection -= queryUI.OnSelectResponse;
                }
                Destroy(queryUI.m_responses[i].gameObject);
            }
            queryUI.m_responses.Clear();

            // Store the new set of response texts.
            queryUI.m_responsesText = responses;

            // Create new response buttons for each response option.
            for (int i = 0; i < responses.Length; i++)
            {
                // Use the primary response button prefab for the first response if available, otherwise use the default.
                Button prefab = i == 0 && queryUI.m_primaryResponseButtonPrefab != null ? queryUI.m_primaryResponseButtonPrefab : queryUI.m_responseButtonPrefab;
                Button instance = Instantiate(prefab.gameObject).GetComponent<Button>();
                instance.GetComponentInChildren<TMP_Text>().text = responses[i];
                instance.name = i.ToString();
                instance.onClick.AddListener(delegate { queryUI.OnClickResponse(instance); });
                instance.GetComponent<RectTransform>().SetParent(queryUI.m_responseContainer);
                instance.gameObject.AddComponent<SelectableListener>().OnChangeSelection += queryUI.OnSelectResponse;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localEulerAngles = Vector3.zero;
                instance.transform.localScale = Vector3.one;
                Navigation nav = Navigation.defaultNavigation;
                nav.mode = Navigation.Mode.Explicit;
                instance.navigation = nav;

                // Set up explicit navigation between buttons for keyboard/controller support.
                if (i > 0)
                {
                    Button prev = queryUI.m_responses[i - 1];

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

                queryUI.m_responses.Add(instance);
            }

            // Highlight the initial selection and start a coroutine to ensure highlight is set.
            if (responses.Length > 0)
            {
                initialSelection = initialSelection < 0 ? 0 : initialSelection >= responses.Length ? responses.Length - 1 : initialSelection;
                queryUI.m_responses[initialSelection].GetComponent<Button>().Highlight();
                queryUI.StartCoroutine(queryUI.DelayedHighlight(queryUI, initialSelection));
            }

            // Show the query UI using the appropriate method (Animator or SetActive).
            if (queryUI.m_showType == ShowType.Animator && queryUI.m_animator != null)
            {
                queryUI.m_animator.SetBool(ANIMATION_PARAMETER, true);
            }
            else if (queryUI.m_showType == ShowType.SetActive && queryUI.m_rootObject != null)
            {
                queryUI.m_rootObject.SetActive(true);
            }

            // Notify listeners that the QueryUI has loaded.
            OnQueryUILoaded?.Invoke(queryUI);

            // Refresh the layout groups to ensure UI updates immediately.
            if (queryUI.m_rootObject != null)
            {
                Utility.RefreshLayoutGroupsImmediateAndRecursive(queryUI.m_rootObject);
            }
            else
            {
                Utility.RefreshLayoutGroupsImmediateAndRecursive(queryUI.m_responseContainer.gameObject);
            }

            // Mark the QueryUI as active.
            queryUI.m_isOn = true;
        }

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void Initiate(string id, string text, string[] responses, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null) => Initiate(id, text, responses, 0, callback, selectCallback);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="description">The query description text.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void InitiateWithDescription(string id, string text, string description, string[] responses, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null) => Initiate(id, text, description, responses, 0, callback, selectCallback);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="initialSelection">The index of the initially selected response.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void Initiate(string text, string[] responses, int initialSelection, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null) => Initiate(Instance != null ? Instance.Id : string.Empty, text, responses, initialSelection, callback, selectCallback);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void Initiate(string text, string[] responses, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null) => Initiate(Instance != null ? Instance.Id : string.Empty, text, responses, callback, selectCallback);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void Initiate(string id, string text, string[] responses, Action<string> callback)
        {
            s_stringCallback = callback;
            Initiate(id, text, responses, CallbackConverter);
        }

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="description">The query description text.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void InitiateWithDescription(string id, string text, string description, string[] responses, Action<string> callback)
        {
            s_stringCallback = callback;
            InitiateWithDescription(id, text, description, responses, CallbackConverter);
        }

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void Initiate(string text, string[] responses, Action<string> callback) => Initiate(Instance != null ? Instance.Id : string.Empty, text, responses, callback);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="description">The query description text.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void InitiateWithDescription(string text, string description, string[] responses, Action<string> callback) => InitiateWithDescription(Instance != null ? Instance.Id : string.Empty, text, description, responses, callback);

        /// <summary>
        /// Display an alert with a confirm and cancel button.
        /// </summary>
        /// <param name="text">The alert text.</param>
        /// <param name="confirmText">The confirm button label.</param>
        /// <param name="cancelText">The cancel button label.</param>
        /// <param name="callback">The function to call when the alert is closed.</param>
        public static void Alert(string id, string text, string confirmText, string cancelText, Action<string> callback)
        {
            if (!string.IsNullOrEmpty(cancelText))
            {
                Initiate(id, text, new string[] { confirmText, cancelText }, callback);
            }
            else
            {
                Initiate(id, text, new string[] { confirmText }, callback);
            }
        }

        /// <summary>
        /// Display an alert with a confirm and cancel button.
        /// </summary>
        /// <param name="text">The alert text.</param>
        /// <param name="confirmText">The confirm button label.</param>
        /// <param name="cancelText">The cancel button label.</param>
        /// <param name="callback">The function to call when the alert is closed.</param>
        public static void Alert(string text, string confirmText, string cancelText, Action<string> callback) => Alert(string.Empty, text, confirmText, cancelText, callback);

        /// <summary>
        /// Display an alert with a confirm and cancel button.
        /// </summary>
        /// <param name="text">The alert text.</param>
        /// <param name="confirmText">The confirm button label.</param>
        /// <param name="cancelText">The cancel button label.</param>
        public static void Alert(string text, string confirmText, string cancelText) => Alert(text, confirmText, cancelText, null);

        /// <summary>
        /// Display an alert.
        /// </summary>
        /// <param name="text">The alert text.</param>
        /// <param name="callback">The function to call when the alert is closed.</param>
        /// <param name="confirmText">The confirm button label.</param>
        public static void Alert(string text, Action<string> callback, string confirmText = "OK") => Alert(text, confirmText, string.Empty, callback);

        /// <summary>
        /// Display an alert.
        /// </summary>
        /// <param name="text">The alert text.</param>
        /// <param name="confirmText">The confirm button label.</param>
        public static void Alert(string text, string confirmText = "OK") => Alert(text, confirmText);

        private static void CallbackConverter(QueryEventData eventData) => s_stringCallback?.Invoke(eventData.chosenResponse);

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
                queryId = Id,
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
                queryId = Id,
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