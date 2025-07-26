using TMPro;
using UnityEngine;
using Yontalane.Dialog;

namespace Yontalane.Demos.Dialog
{
    /// <summary>
    /// Manages the main game logic, including handling player input for dialog initiation and updating the player's name.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reference to the NPC DialogAgent that the player will interact with.")]
        private DialogAgent m_npc = null;

        [Tooltip("Input field for entering the player's name.")]
        [SerializeField]
        private TMP_InputField m_playerNameField = null;

        /// <summary>
        /// Dialog is initiated by clicking on the talk button.
        /// </summary>
        public void OnClick() => InitiateDialog();

        /// <summary>
        /// Update the player's name and initiate dialog.
        /// </summary>
        private void InitiateDialog()
        {
            if (!DialogProcessor.IsActive)
            {
                DialogProcessor.PlayerName = m_playerNameField.text;
                m_npc.InitiateDialog();
            }
        }
    }
}