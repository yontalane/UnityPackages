using UnityEngine;
using Yontalane.Dialog;

namespace Yontalane.Demos.Dialog
{
    /// <summary>
    /// Represents a non-player character (NPC) that can interact with the dialog system,
    /// handle inventory-related dialog functions, and provide keyword replacements such as the desired item.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Dialog/NPC")]
    public sealed class NPC : DialogAgent
    {
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

            return base.DialogFunction(call, parameter, out result);
        }

        /// <summary>
        /// Callback to be used in DialogProcess thanks to this class inheriting from DialogResponder.
        /// If the dialog contains the keyword DesiredItem then replace that word with our actual desired item.
        /// </summary>
        public override bool GetKeyword(string key, out string result)
        {
            if (key.Equals("DesiredItem"))
            {
                result = GameManager.DesiredItem;
                return true;
            }

            return base.GetKeyword(key, out result);
        }
    }
}