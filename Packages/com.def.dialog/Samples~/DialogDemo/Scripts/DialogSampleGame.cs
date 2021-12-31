using DEF.Dialog;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DEF.Demos
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public class DialogSampleGame : MonoBehaviour
    {
        [SerializeField] private DialogAgent m_npc = null;

        private void Awake() => DialogProcessor.PlayerName = "Player";

        public void OnClickDialogAgent()
        {
            if (!DialogProcessor.IsActive)
            {
                DialogProcessor.InitiateDialog(m_npc);
            }
        }

        public void OnSubmit()
        {
            if (!DialogProcessor.IsActive)
            {
                DialogProcessor.InitiateDialog(m_npc);
            }
        }
    }
}