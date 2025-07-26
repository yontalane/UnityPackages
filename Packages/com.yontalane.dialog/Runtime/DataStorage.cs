using System.Collections.Generic;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Provides a static storage for dialog-related variables used by the DialogProcessor.
    /// </summary>
    public static class DataStorage
    {
        /// <summary>
        /// A public, static dictionary that the DialogProcessor can use to keep track of the dialog state.
        /// </summary>
        public static Dictionary<string, string> Vars { get; set; } = new Dictionary<string, string>();
    }
}