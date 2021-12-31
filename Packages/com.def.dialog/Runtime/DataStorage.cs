using System.Collections.Generic;

namespace DEF.Dialog
{
    public static class DataStorage
    {
        /// <summary>
        /// A public, static dictionary that the DialogProcessor can use to keep track of the dialog state.
        /// </summary>
        public static Dictionary<string, string> Vars { get; set; } = new Dictionary<string, string>();
    }
}