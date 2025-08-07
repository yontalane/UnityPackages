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
}
