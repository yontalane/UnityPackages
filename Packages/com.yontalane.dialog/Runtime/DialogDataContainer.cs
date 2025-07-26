using UnityEngine;

namespace Yontalane.Dialog
{
    /// <summary>
    /// A ScriptableObject container for holding dialog data in the Yontalane dialog system.
    /// </summary>
    [CreateAssetMenu(fileName = "Dialog Data", menuName = "Yontalane/Dialog/Dialog Data", order = 1)]
    public class DialogDataContainer : ScriptableDialogAgent
    {
        /// <summary>
        /// The dialog data stored in this container.
        /// </summary>
        [Tooltip("The dialog data stored in this container.")]
        public DialogData data;
    }
}
