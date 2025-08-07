using System;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Main UI controller for displaying and managing dialog interactions, including speaker display, text typing, responses, and associated UI elements.
    /// </summary>
    public interface IDialogUI
    {
        /// <summary>
        /// Initializes the dialog UI with the given line data.
        /// </summary>
        /// <param name="line">The line data to initialize the dialog UI with.</param>
        /// <param name="lineCompleteCallback">The callback to invoke when the line is complete.</param>
        public void Initiate(LineData line, Action<string> lineCompleteCallback, Func<string, string> replaceInlineText);

        /// <summary>
        /// Closes the dialog UI.
        /// </summary>
        public void Close();
    }
}
