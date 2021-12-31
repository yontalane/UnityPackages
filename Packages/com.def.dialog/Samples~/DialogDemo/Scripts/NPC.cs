using DEF.Dialog;
using UnityEngine;

namespace DEF.Demos.Dialog
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DialogAgent))]
    public sealed class NPC : DialogResponder
    {
        [SerializeField] private string m_desiredItem = "";

        /// <summary>
        /// Callback to be used in DialogProcess thanks to this class inheriting from DialogResponder.
        /// Checks if we're invoking inventory-related keywords and updates the inventory accordingly.
        /// </summary>
        public override bool DialogFunction(string call, string parameter, out string result)
        {
            result = null;
            switch (call)
            {
                case "GiveToPlayer":
                    InventoryManager.Add(parameter);
                    return true;
                case "TakeFromPlayer":
                    InventoryManager.Remove(parameter);
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Callback to be used in DialogProcess thanks to this class inheriting from DialogResponder.
        /// If the dialog contains the keyword DesiredItem then replace that word with our actual desired item.
        /// </summary>
        public override bool GetKeyword(string key, out string result)
        {
            if (key.Equals("DesiredItem"))
            {
                result = m_desiredItem;
                return true;
            }
            result = null;
            return false;
        }
    }
}