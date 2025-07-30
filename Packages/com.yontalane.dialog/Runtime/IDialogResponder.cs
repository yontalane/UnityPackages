using System.Collections.Generic;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Defines the interface for responding to dialog events and custom function calls in the Yontalane dialog system.
    /// Implement this interface to provide custom logic for dialog script functions and keyword replacements.
    /// </summary>
    public interface IDialogResponder
    {
        /// <summary>
        /// Execute an function called from a dialog script.
        /// </summary>
        /// <param name="call">The name of the function.</param>
        /// <param name="parameter">A parameter for the function.</param>
        /// <param name="result">The return value of the function.</param>
        /// <returns>True if we have a function to invoke with the given name; false if we do not.</returns>
        public bool DialogFunction(string call, string parameter, out string result);

        /// <summary>
        /// Replace a keyword in a dialog script with custom text.
        /// </summary>
        /// <param name="key">The keyword to replace.</param>
        /// <param name="result">The text to replace it with.</param>
        /// <returns>True if we have text to replace the keyword with; false if we don't.</returns>
        public bool GetKeyword(string key, out string result);

        /// <summary>
        /// Aggregates inline image replacement information for dialog text.
        /// Implement this method to add or modify inline image replacement info for keywords in dialog lines.
        /// </summary>
        /// <param name="info">A list to which inline image replacement info should be added or updated.</param>
        /// <returns>True if any inline image info was added or modified; otherwise, false.</returns>
        public bool GetInlineImageInfo(List<InlineImageReplacementInfo> info);
    }
}