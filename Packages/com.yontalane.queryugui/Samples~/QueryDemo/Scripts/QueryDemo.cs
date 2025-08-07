using Yontalane.Query;
using Yontalane.QueryUGUI;
using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.Demos.QueryUGUI
{
    /// <summary>
    /// Demonstrates the use of the QueryUI system by initiating a query with random responses,
    /// handling the result, and updating the UI accordingly in the Query Demo scene.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Query UGUI/Query Demo")]
    [RequireComponent(typeof(QueryProcessor))]
    public sealed class QueryDemo : MonoBehaviour
    {
        [Tooltip("Text field for displaying the result of the query.")]
        [SerializeField]
        private Text m_resultText = null;

        [Tooltip("The button to initiate the query (so we can make sure it's highlighted after the query box closes).")]
        [SerializeField]
        private Button m_goButton = null;

        /// <summary>
        /// Initiates a query with a random set of response options.
        /// - Generates between 2 and 4 random response strings.
        /// - Passes the prompt and responses to the Query system.
        /// </summary>
        public void InitiateQuery()
        {
            // Generate a random number (between 2 and 4) of response options for the query.
            string[] responses = new string[Random.Range(2, 5)];

            // Populate each response with a string containing random text and a random number.
            for (int i = 0; i < responses.Length; i++)
            {
                responses[i] = $"Some Text {Random.Range(10, 99)}";
            }
            
            // Initiate the query UI with the prompt and generated responses, specifying a callback for the result.
            QueryProcessor.Initiate("Pick something.", responses, OnQueryGetResult);
        }

        /// <summary>
        /// Callback for when the user selects a response from the query.
        /// - Updates the result text field with the selected response.
        /// - Highlights the "Go" button for user feedback.
        /// </summary>
        /// <param name="result">The response selected by the user.</param>
        private void OnQueryGetResult(string result)
        {
            m_resultText.text = $"Result: \"{result}\"";
            m_goButton.Highlight();
        }
    }
}