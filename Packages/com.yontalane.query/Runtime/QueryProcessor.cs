using System;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Query
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Query/Query Processor")]
    public class QueryProcessor : Singleton<QueryProcessor>
    {
        [Serializable]
        public class QueryEventHandler : UnityEvent<QueryEventData> { }
        
        private Action<QueryEventData> m_callback;
        private Action<QueryEventData> m_selectCallback;
        private Action<string> m_stringCallback;

        [Header(("UI"))]
        
        [Tooltip("A game object containing a Query UI.")]
        [SerializeField]
        private GameObject m_queryUI;

        [Header("Callbacks")]

        [Tooltip("An action that is invoked when a query is opened.")]
        [SerializeField]
        private QueryEventHandler m_onQueryStart;
        
        [Tooltip("An action that is invoked when the selected button changes in a query.")]
        [SerializeField]
        private QueryEventHandler m_onQueryChangeSelection;
        
        [Tooltip("An action that is invoked when a query is closed.")]
        [SerializeField]
        private QueryEventHandler m_onQueryEnd;
        
        /// <summary>
        /// The active query's unique ID.
        /// </summary>
        public string Id { get; private set; } = string.Empty;

        public void Callback(QueryEventData data)
        {
            QueryEventData output = new()
            {
                queryId = Id,
                prompt = data.prompt,
                description = data.description,
                responses = data.responses,
                chosenResponse = data.chosenResponse,
            };
            
            m_callback?.Invoke(output);
            m_onQueryEnd?.Invoke(output);
        }

        public void SelectCallback(QueryEventData data)
        {
            QueryEventData output = new()
            {
                queryId = Id,
                prompt = data.prompt,
                description = data.description,
                responses = data.responses,
                chosenResponse = data.chosenResponse,
            };
            
            m_selectCallback?.Invoke(output);
            m_onQueryChangeSelection?.Invoke(output);
        }

        private static void CallbackConverter(QueryEventData eventData) => Instance.m_stringCallback?.Invoke(eventData.chosenResponse);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="id">A unique identifier for this query instance.</param>
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
        /// <param name="id">A unique identifier for this query instance.</param>
        /// <param name="text">The query message.</param>
        /// <param name="description">The query description text.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="initialSelection">The index of the initially selected response.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void Initiate(string id, string text, string description, string[] responses, int initialSelection, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null)
        {
            // Get the singleton instance of QueryUI.
            QueryProcessor processor = Instance;
            IQueryUI ui = Instance.m_queryUI != null && Instance.m_queryUI.TryGetComponent(out IQueryUI queryUI) ? queryUI : Instance.TryGetComponent(out IQueryUI attachedUI) ? attachedUI : null;

            // If the QueryUI instance is not found, log an error and exit.
            if (processor == null || ui == null)
            {
                Debug.LogError($"{nameof(QueryProcessor)} not properly set up.");
                return;
            }

            // Set the query ID and assign the provided callbacks and main text.
            processor.Id = id;
            Instance.m_callback = callback;
            Instance.m_selectCallback = selectCallback;
            ui.Initialize(text, description, responses, initialSelection, Instance.Callback, Instance.SelectCallback);
            
            Instance.m_onQueryStart?.Invoke(new()
            {
                queryId = id,
                prompt = text,
                description = description,
                responses = responses,
                chosenResponse = string.Empty,
            });
        }

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="id">A unique identifier for this query instance.</param>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void Initiate(string id, string text, string[] responses, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null) => Initiate(id, text, responses, 0, callback, selectCallback);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="id">A unique identifier for this query instance.</param>
        /// <param name="text">The query message.</param>
        /// <param name="description">The query description text.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="initialSelection">The initially selected response.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        /// <param name="selectCallback">The function to call when a response is selected but not yet chosen.</param>
        public static void InitiateWithDescription(string id, string text, string description, string[] responses, int initialSelection, Action<QueryEventData> callback, Action<QueryEventData> selectCallback = null) => Initiate(id, text, description, responses, initialSelection, callback, selectCallback);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="id">A unique identifier for this query instance.</param>
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
        /// <param name="id">A unique identifier for this query instance.</param>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void Initiate(string id, string text, string[] responses, Action<string> callback)
        {
            Instance.m_stringCallback = callback;
            Initiate(id, text, responses, CallbackConverter);
        }

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="id">A unique identifier for this query instance.</param>
        /// <param name="text">The query message.</param>
        /// <param name="description">The query description text.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void InitiateWithDescription(string id, string text, string description, string[] responses, Action<string> callback)
        {
            Instance.m_stringCallback = callback;
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
        /// <param name="initialSelection">The initially selected response.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void InitiateWithDescription(string text, string description, string[] responses, int initialSelection, Action<string> callback)
        {
            Instance.m_stringCallback = callback;
            InitiateWithDescription(Instance != null ? Instance.Id : string.Empty, text, description, responses, initialSelection, CallbackConverter);
        }

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="description">The query description text.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void InitiateWithDescription(string text, string description, string[] responses, Action<string> callback)
        {
            Instance.m_stringCallback = callback;
            InitiateWithDescription(Instance != null ? Instance.Id : string.Empty, text, description, responses, 0, CallbackConverter);
        }

        /// <summary>
        /// Display an alert with a confirm and cancel button.
        /// </summary>
        /// <param name="id">A unique identifier for this query instance.</param>
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
        public static void Alert(string text, string confirmText = "OK") => Alert(text, confirmText, string.Empty);
    }
}
