using UnityEngine.Events;

namespace Yontalane.Dialog
{
    public interface IDialogAgent : IDialogResponder
    {
        public string name { get; }
        public bool enabled { get; set; }
        public string ID { get; }
        public DialogData Data { get; }
        public void InitiateDialog(UnityAction onExitDialog);
        public string DisplayName { get; }
    }
}
