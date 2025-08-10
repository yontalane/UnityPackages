using UnityEngine;
using UnityEngine.UIElements;
using Yontalane.Query;

namespace Yontalane.Demos.QueryUGUI
{
    // The QueryDemo class is a Unity MonoBehaviour that demonstrates how to use the Query system with UI Toolkit.
    // It manages UI elements, handles button clicks to initiate queries, and updates the UI with results.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(QueryProcessor))]
    [RequireComponent(typeof(DialogUIElements.QueryUI))]
    [AddComponentMenu("Yontalane/Demos/Dialog UI Toolkit/Query Demo")]
    public sealed class QueryDemo : MonoBehaviour
    {
        #region Private Fields
        private DialogUIElements.QueryUI m_queryUI;
        private Button m_button;
        private Label m_resultLabel;
        #endregion

        #region Inspector Fields

        [Header("UI")]

        [Tooltip("The button to initiate the query (so we can make sure it's highlighted after the query box closes).")]
        [SerializeField]
        private string m_buttonName = null;

        [Tooltip("Text field for displaying the result of the query.")]
        [SerializeField]
        private string m_resultLabelName = null;

        [Header("Audio")]

        [Tooltip("Audio to play when clicking the go button.")]
        [SerializeField]
        private AudioClip m_buttonClickA = null;

        [Tooltip("Audio to play when clicking a response button.")]
        [SerializeField]
        private AudioClip m_buttonClickB = null;

        [Tooltip("Audio to play when changing selection.")]
        [SerializeField]
        private AudioClip m_selectionClick = null;

        #endregion

        #region Unity Methods

        private void Start()
        {
            // Get the QueryUI component attached to this GameObject.
            m_queryUI = GetComponent<DialogUIElements.QueryUI>();

            // Retrieve the UIDocument from the QueryUI component.
            UIDocument document = m_queryUI.Document;

            // Get the root visual element for the button using the specified name.
            m_button = document.rootVisualElement.Q<Button>(m_buttonName);

            // Get the root visual element for the result label using the specified name.
            m_resultLabel = document.rootVisualElement.Q<Label>(m_resultLabelName);

            // Register a callback to initiate the query when the button is clicked.
            m_button.clicked += () => InitiateQuery();

            // Set focus to the button at startup.
            m_button.Focus();
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Initiates a query using Yontalane's QueryProcessor.
        /// </summary>
        public void InitiateQuery()
        {
            // Disable the start button's picking mode so it can't be interacted with during the query.
            m_button.pickingMode = PickingMode.Ignore;

            // Generate a random number of response options.
            string[] responses = new string[Random.Range(2, 5)];

            // Populate the responses array with random text values.
            for (int i = 0; i < responses.Length; i++)
            {
                responses[i] = $"Some Text {Random.Range(10, 100)}";
            }
            
            // Initiate the query UI with the generated responses and callback methods.
            QueryProcessor.Initiate("Pick something.", responses, OnQueryGetResult, OnQueryChangeResult);

            // Play a sound to indicate the query has been initiated.
            PlaySound(m_buttonClickA);
        }

        /// <summary>
        /// Callback method invoked when a query result is selected.
        /// Plays a sound, updates the result label with the chosen response,
        /// and restores the button's picking mode and focus.
        /// </summary>
        private void OnQueryGetResult(QueryEventData eventData)
        {
            // Play a sound to indicate the result was selected.
            PlaySound(m_buttonClickB);
            
            // Update the result label with the chosen response.
            m_resultLabel.text = $"Result: \"{eventData.chosenResponse}\"";

            // Restore the button's picking mode and focus.
            m_button.pickingMode = PickingMode.Position;
            m_button.Focus();
        }

        /// <summary>
        /// Callback method invoked when the query selection changes.
        /// Plays a sound to indicate the selection change.
        /// </summary>
        private void OnQueryChangeResult(QueryEventData eventData) => PlaySound(m_selectionClick);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Plays the specified audio clip at the main camera's position.
        /// </summary>
        private static void PlaySound(AudioClip clip) => AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);

        #endregion
    }
}