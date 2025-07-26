using UnityEngine.Events;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Defines the interface for a dialog agent in the Yontalane dialog system.
    /// A dialog agent is responsible for providing dialog data, handling dialog state,
    /// and responding to dialog events.
    /// In a game where the player talks to multiple NPCs, each with their own
    /// dialog tree, each NPC might have its own dialog agent.
    /// </summary>
    public interface IDialogAgent : IDialogResponder
    {
        /// <summary>
        /// The internal name of the dialog agent.
        /// </summary>
        public string name { get; }

        /// <summary>
        /// Indicates whether the dialog agent is enabled and can participate in dialog.
        /// </summary>
        public bool enabled { get; set; }

        /// <summary>
        /// A unique identifier for the dialog agent.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The dialog data associated with this agent.
        /// </summary>
        public DialogData Data { get; }

        /// <summary>
        /// Initiates a dialog session with this agent.
        /// </summary>
        /// <param name="onExitDialog">Callback invoked when the dialog session ends.</param>
        public void InitiateDialog(UnityAction onExitDialog);

        /// <summary>
        /// The display name of the dialog agent, shown to the player.
        /// </summary>
        public string DisplayName { get; }
    }
}
