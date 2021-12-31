using UnityEngine;

namespace DEF.Dialog
{
    public abstract class DialogResponder : MonoBehaviour
    {
        public virtual bool DialogFunction(string call, string parameter, out string result)
        {
            result = null;
            return false;
        }

        public virtual bool GetKeyword(string key, out string result)
        {
            result = null;
            return false;
        }
    }
}