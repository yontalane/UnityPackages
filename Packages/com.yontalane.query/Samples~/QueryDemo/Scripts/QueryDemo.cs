using Yontalane.Query;
using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.Demos.Query
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(QueryUI))]
    public sealed class QueryDemo : MonoBehaviour
    {
        public QueryUI QueryUI { get; private set; } = null;

        [SerializeField]
        [Tooltip("Text field for displaying the result of the query.")]
        private Text m_resultText = null;

        [SerializeField]
        [Tooltip("The button to initiate the query (so we can make sure it's highlighted after the query box closes).")]
        private Button m_goButton = null;

        private void Start() => QueryUI = GetComponent<QueryUI>();

        public void InitiateQuery()
        {
            string[] responses = new string[Random.Range(2, 5)];
            for (int i = 0; i < responses.Length; i++)
            {
                responses[i] = $"Some Text {Random.Range(10, 99)}";
            }
            QueryUI.Initiate("Pick something.", responses, OnQueryGetResult);
        }

        private void OnQueryGetResult(string result)
        {
            m_resultText.text = $"Result: \"{result}\"";
            m_goButton.Highlight();
        }
    }
}