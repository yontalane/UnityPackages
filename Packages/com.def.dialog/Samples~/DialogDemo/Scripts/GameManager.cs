using DEF.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace DEF.Demos.Dialog
{
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private DialogAgent m_npc = null;
        [SerializeField] private InputField m_playerNameField = null;

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
                DialogProcessor.InitiateDialog(m_npc);
            }
        }
    }
}