using System;

namespace Yontalane.Query
{
    /// <summary>
    /// Data connected to query events.
    /// </summary>
    public struct QueryEventData
    {
        /// <summary>
        /// The prompt text.
        /// </summary>
        public string prompt;

        /// <summary>
        /// The description text.
        /// </summary>
        public string description;

        /// <summary>
        /// The response texts.
        /// </summary>
        public string[] responses;

        /// <summary>
        /// The chosen response's text.
        /// </summary>
        public string chosenResponse;

        /// <summary>
        /// The query's unique ID.
        /// </summary>
        public string queryId;
    }

    /// <summary>
    /// Specifies how the query dialog should be shown or hidden.
    /// </summary>
    [Serializable]
    public enum ShowType
    {
        None = 0,
        Animator = 1,
        SetActive = 2,
    }

    /// <summary>
    /// Delegate for handling events related to the QueryUI, such as when a query UI is loaded.
    /// </summary>
    /// <param name="queryUI">The QueryUI instance involved in the event.</param>
    public delegate void QueryUIHandler(IQueryUI queryUI);
}
