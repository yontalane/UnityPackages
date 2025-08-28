using System.Collections.Generic;
using UnityEngine;
using Yontalane.Dialog;

namespace Yontalane.Demos.Dialog
{
    /// <summary>
    /// Manages icon replacement information for inline images in dialog text.
    /// Implements IDialogResponder to provide icon-related keyword and image info to the dialog system.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Dialog/Icon Manager")]
    public sealed class IconManager : Singleton<IconManager>, IDialogResponder
    {
        [Tooltip("Array of inline image replacement information for dialog icons.")]
        [SerializeField]
        private InlineImageReplacementInfo[] m_replacementInfo = new InlineImageReplacementInfo[0];

        /// <summary>
        /// Handles custom dialog function calls. This implementation does not handle any functions.
        /// </summary>
        /// <param name="call">The name of the function to execute.</param>
        /// <param name="parameter">A parameter for the function.</param>
        /// <param name="result">The return value of the function, if any.</param>
        /// <returns>Always returns false, as no functions are handled.</returns>
        public bool DialogFunction(string call, string parameter, out string result)
        {
            result = default;
            return false;
        }

        /// <summary>
        /// Retrieves the value associated with the specified icon-related keyword.
        /// This implementation does not handle any keywords.
        /// </summary>
        /// <param name="key">The keyword to look up.</param>
        /// <param name="result">The value associated with the keyword, if any.</param>
        /// <returns>Always returns false, as no keywords are handled.</returns>
        public bool GetKeyword(string key, out string result)
        {
            result = default;
            return false;
        }

        /// <summary>
        /// Adds inline image replacement information for icons to the provided list.
        /// </summary>
        /// <param name="info">A list to which inline image replacement info will be added.</param>
        /// <returns>True if any inline image info was added; otherwise, false.</returns>
        public bool GetInlineImageInfo(List<InlineImageReplacementInfo> info)
        {
            info.AddRange(m_replacementInfo);
            return m_replacementInfo.Length > 0;
        }

        /// <summary>
        /// Attempts to construct or modify a LineData object for a given dialog line specific to the inventory system.
        /// </summary>
        /// <param name="call">The text of the dialog line to process.</param>
        /// <param name="lineData">The resulting LineData object after processing, if any.</param>
        /// <returns>True if the line builder was invoked and lineData was set; otherwise, false.</returns>
        public bool GetLineDataBuilderResult(string call, out LineData lineData)
        {
            lineData = null;
            return false;
        }
    }
}