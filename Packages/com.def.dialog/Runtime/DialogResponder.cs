using UnityEngine;

namespace DEF.Dialog
{
    public abstract class DialogResponder : MonoBehaviour
    {
        /// <summary>
        /// Execute an function called from a dialog script.
        /// </summary>
        /// <param name="call">The name of the function.</param>
        /// <param name="parameter">A parameter for the function.</param>
        /// <param name="result">The return value of the function.</param>
        /// <returns>True if we have a function to invoke with the given name; false if we do not.</returns>
        public virtual bool DialogFunction(string call, string parameter, out string result)
        {
            result = null;
            return false;
        }

        /// <summary>
        /// Replace a keyword in a dialog script with custom text.
        /// </summary>
        /// <param name="key">The keyword to replace.</param>
        /// <param name="result">The text to replace it with.</param>
        /// <returns>True if we have text to replace the keyword with; false if we don't.</returns>
        public virtual bool GetKeyword(string key, out string result)
        {
            result = null;
            return false;
        }
    }
}