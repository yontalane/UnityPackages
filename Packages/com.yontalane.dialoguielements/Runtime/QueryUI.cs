using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Yontalane.Query;
using Yontalane.UIElements;

namespace Yontalane.DialogUIElements
{
    /// <summary>
    /// Singleton component that implements IQueryUI to display queries using Unity's UI Toolkit.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog UI Toolkit/Query UI")]
    public sealed class QueryUI : Singleton<QueryUI>, IQueryUI
    {
        #region Private Fields
        private Action<QueryEventData> m_callback;
        private Action<QueryEventData> m_selectCallback;
        private string m_prompt;
        private string m_description;
        private string m_id;
        private static int s_currentID = 0;
        #endregion

        #region Inspector Fields

        [Tooltip("Reference to the UIDocument that contains the root VisualElement for displaying queries.")]
        [SerializeField]
        private UIDocument m_document;

        [Tooltip("The name of the VisualElement in the UI hierarchy that will serve as the root for displaying queries.")]
        [SerializeField]
        private string m_queryRoot;

        #endregion

        #region Public Properties

        /// <summary>
        /// Reference to the UIDocument that contains the root VisualElement for displaying queries.
        /// </summary>
        public UIDocument Document => m_document != null ? m_document : FindAnyObjectByType<UIDocument>();

        /// <summary>
        /// The current active Query UI element instance.
        /// </summary>
        public UIElements.Query Query { get; private set; }

        /// <summary>
        /// Indicates whether a query UI is currently active and visible.
        /// </summary>
        public bool IsActive { get; private set; } = false;

        #endregion


        #region Unity Methods

        private void Reset()
        {
            m_document = FindAnyObjectByType<UIDocument>();
        }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Initializes and displays a query UI with the specified prompt, description, and response options.
        /// Sets up callbacks for result selection and selection changes.
        /// </summary>
        /// <param name="text">The main prompt or question to display.</param>
        /// <param name="description">Additional description or context for the query.</param>
        /// <param name="responses">An array of response options for the user to choose from.</param>
        /// <param name="_">Unused parameter (reserved for initial selection index).</param>
        /// <param name="callback">Callback invoked when a response is selected.</param>
        /// <param name="selectCallback">Callback invoked when the selection changes.</param>
        public void Initialize(string text, string description, string[] responses, int _, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null)
        {
            // Store the callback and query details for later use
            m_callback = callback;
            m_selectCallback = selectCallback;
            m_prompt = text;
            m_description = description;
            m_id = (++s_currentID).ToString();

            // Get the UIDocument to use for displaying the query UI
            UIDocument document = Document;

            // If no UIDocument is found, log an error and exit
            if (document == null)
            {
                Debug.LogError($"Could not find {nameof(UIDocument)}.");
                return;
            }

            // Get the root VisualElement from the UIDocument
            VisualElement root = document.rootVisualElement;
            
            // Attempt to find the VisualElement specified by m_queryRoot; if not found, use the root element as fallback.
            VisualElement queryRoot = root.Q<VisualElement>(m_queryRoot) ?? root;

            // Create a new Query UI element and add it to the query root
            Query = new(text, description, responses, ConvertCallback);
            queryRoot.Add(Query);

            // Set the IsActive flag to true to indicate that the query UI is now active.
            IsActive = true;

            // Subscribe the OnChangeSelectedButton method to the Query's OnChangeSelectedButton event.
            Query.OnChangeSelectedButton += OnChangeSelectedButton;

            // Start a coroutine to delay focusing the query UI until the end of the frame.
            StartCoroutine(DelayedFocus());
        }

        /// <summary>
        /// Initializes and displays a query UI with the specified prompt, description, and response options.
        /// Sets up callbacks for result selection and selection changes.
        /// </summary>
        /// <param name="text">The main prompt or question to display.</param>
        /// <param name="description">Additional description or context for the query.</param>
        /// <param name="responses">An array of response options for the user to choose from.</param>
        /// <param name="callback">Callback invoked when a response is selected.</param>
        /// <param name="selectCallback">Callback invoked when the selection changes.</param>
        public void Initialize(string text, string description, string[] responses, Action<QueryEventData> callback = null, Action<QueryEventData> selectCallback = null)
        {
            Initialize(text, description, responses, -1, callback, selectCallback);
        }

        /// <summary>
        /// Initializes and displays a query UI with the specified prompt, description, and response options.
        /// Sets up callbacks for result selection and selection changes.
        /// </summary>
        /// <param name="text">The main prompt or question to display.</param>
        /// <param name="description">Additional description or context for the query.</param>
        /// <param name="response">The label of the UI's single button.</param>
        /// <param name="callback">Callback invoked when a response is selected.</param>
        /// <param name="selectCallback">Callback invoked when the selection changes.</param>
        public void Initialize(string text, string description, string response, Action<QueryEventData> callback = null, Action<QueryEventData> selectCallback = null)
        {
            Initialize(text, description, new string[] { response }, -1, callback, selectCallback);
        }

        /// <summary>
        /// Initializes and displays a query UI with the specified prompt, description, and response options.
        /// Sets up callbacks for result selection and selection changes.
        /// </summary>
        /// <param name="text">The main prompt or question to display.</param>
        /// <param name="responses">An array of response options for the user to choose from.</param>
        /// <param name="callback">Callback invoked when a response is selected.</param>
        /// <param name="selectCallback">Callback invoked when the selection changes.</param>
        public void Initialize(string text, string[] responses, Action<QueryEventData> callback = null, Action<QueryEventData> selectCallback = null)
        {
            Initialize(text, string.Empty, responses, -1, callback, selectCallback);
        }

        /// <summary>
        /// Initializes and displays a query UI with the specified prompt, description, and response options.
        /// Sets up callbacks for result selection and selection changes.
        /// </summary>
        /// <param name="text">The main prompt or question to display.</param>
        /// <param name="response">The label of the UI's single button.</param>
        /// <param name="callback">Callback invoked when a response is selected.</param>
        /// <param name="selectCallback">Callback invoked when the selection changes.</param>
        public void Initialize(string text, string response, Action<QueryEventData> callback = null, Action<QueryEventData> selectCallback = null)
        {
            Initialize(text, string.Empty, new string[] { response }, -1, callback, selectCallback);
        }

        #endregion

        #region Coroutine Methods

        /// <summary>
        /// Coroutine that waits until the end of the frame before focusing the Query UI element.
        /// This ensures that the UI is fully initialized and ready to receive focus.
        /// </summary>
        private IEnumerator DelayedFocus()
        {
            yield return new WaitForEndOfFrame();
            Query.Focus();
        }

        #endregion

        #region Callback Methods

        /// <summary>
        /// Converts a UIElements.QueryEvent into a QueryEventData and invokes the callback.
        /// </summary>
        /// <param name="queryEvent">The event containing the user's query response.</param>
        private void ConvertCallback(QueryEvent queryEvent)
        {
            // Invoke the callback with the query event data, passing relevant information.
            m_callback?.Invoke(new()
            {
                chosenResponse = queryEvent.chosenResponseText,
                description = m_description,
                prompt = m_prompt,
                responses = queryEvent.allResponses,
                queryId = m_id,
            });

            // Unsubscribe from the Query's OnChangeSelectedButton event to prevent memory leaks.
            Query.OnChangeSelectedButton -= OnChangeSelectedButton;

            // Mark the query UI as inactive.
            IsActive = false;
        }

        /// <summary>
        /// Called when the selected button in the query UI changes.
        /// Invokes the selection callback with the current selection details if the query UI is active.
        /// </summary>
        /// <param name="button">The newly selected SelectableButton.</param>
        private void OnChangeSelectedButton(SelectableButton button)
        {
            // Check if the query UI is currently active; if not, exit early.
            if (!IsActive)
            {
                return;
            }

            // Invoke the selection callback with the current selection details.
            m_selectCallback?.Invoke(new()
            {
                chosenResponse = button.text,
                description = m_description,
                prompt = m_prompt,
                responses = Query.Responses,
                queryId = m_id,
            });
        }

        #endregion
    }
}
