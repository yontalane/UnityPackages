using TMPro;
using UnityEngine;
using Yontalane.Dialog;

namespace Yontalane.Demos.Dialog
{
    /// <summary>
    /// Manages the main game logic, including handling player input for dialog initiation and updating the player's name.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Dialog/Game Manager")]
    public sealed class GameManager : Singleton<GameManager>
    {
        [SerializeField]
        [Tooltip("Reference to the NPC DialogAgent that the player will interact with.")]
        private DialogAgent m_npc = null;

        [Tooltip("Input field for entering the player's name.")]
        [SerializeField]
        private TMP_InputField m_playerNameField = null;

        [Tooltip("Input field for the NPC's desired item.")]
        [SerializeField]
        private TMP_InputField m_desiredItemField = null;

        public static string DesiredItem => Instance.m_desiredItemField.text;

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