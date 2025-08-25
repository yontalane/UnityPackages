using UnityEngine;
using UnityEngine.UIElements;
using Yontalane.Dialog;
using Yontalane.DialogUIElements;

namespace Yontalane.Demos.DialogUIElements
{
    /// <summary>
    /// Manages the main game logic for the Dialog UI Toolkit demo, including dialog initiation,
    /// UI element references, and player interaction with NPCs.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Dialog UI Toolkit/Game Manager")]
    public sealed class GameManager : Singleton<GameManager>
    {
        [Tooltip("The UI Document containing the root VisualElement for the dialog UI.")]
        [SerializeField]
        private UIDocument m_document = null;

        [Tooltip("The NPC DialogAgent that the player can interact with.")]
        [SerializeField]
        private DialogAgent m_npc = null;

        [Tooltip("The name of the Talk button VisualElement in the UI Document.")]
        [SerializeField]
        private string m_talkButton = "TalkButton";

        [Tooltip("The name of the TextField VisualElement for the player's name in the UI Document.")]
        [SerializeField]
        private string m_playerNameField = "PlayerName";

        [Tooltip("The name of the TextField VisualElement for the desired item in the UI Document.")]
        [SerializeField]
        private string m_desiredItemField = "DesiredItem";

        [Space]

        [Tooltip("The sound to play when clicking the \"Talk\" button.")]
        [SerializeField]
        private AudioClip m_buttonClick = null;

        /// <summary>
        /// Gets the DialogPane VisualElement from the UI Document.
        /// </summary>
        public static DialogPane DialogPane => Instance.m_document.rootVisualElement.Q<DialogPane>();

        /// <summary>
        /// Gets the Talk button VisualElement from the UI Document.
        /// </summary>
        public static Button TalkButton => Instance.m_document.rootVisualElement.Q<Button>(Instance.m_talkButton);

        /// <summary>
        /// Gets the current text value of the player's name TextField from the UI Document.
        /// </summary>
        public static string PlayerName => Instance.m_document.rootVisualElement.Q<TextField>(Instance.m_playerNameField).text;

        /// <summary>
        /// Gets the current text value of the desired item TextField from the UI Document.
        /// </summary>
        public static string DesiredItem => Instance.m_document.rootVisualElement.Q<TextField>(Instance.m_desiredItemField).text;

        private void Start()
        {
            // Hide the dialog pane at the start of the game.
            DialogPane.style.display = DisplayStyle.None;

            // Register a callback to initiate dialog when the Talk button is clicked.
            TalkButton.clicked += InitiateDialog;

            // Set focus to the Talk button for immediate player interaction.
            TalkButton.Focus();
        }

        /// <summary>
        /// Initiates a dialog with the NPC if no dialog is currently active.
        /// </summary>
        private void InitiateDialog()
        {
            // Check if a dialog is already active; if so, exit early.
            if (DialogProcessor.IsActive)
            {
                return;
            }

            if (m_buttonClick != null)
            {
                AudioSource.PlayClipAtPoint(m_buttonClick, Camera.main.transform.position);
            }

            // Hide the Talk button while dialog is in progress.
            TalkButton.style.display = DisplayStyle.None;

            // Set the player's name in the dialog processor and initiate dialog with the NPC.
            DialogProcessor.PlayerName = PlayerName;
            m_npc.InitiateDialog();
        }

        /// <summary>
        /// Called when the dialog sequence is complete. Hides the dialog pane and restores the Talk button.
        /// </summary>
        public void OnDialogComplete()
        {
            // Hide the dialog pane after the dialog is complete.
            DialogPane.style.display = DisplayStyle.None;

            // Show the Talk button so the player can initiate another dialog.
            TalkButton.style.display = DisplayStyle.Flex;
            
            // Set focus to the Talk button for immediate player interaction.
            TalkButton.Focus();
        }
    }
}