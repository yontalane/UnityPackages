using UnityEngine;
using DEF.Dialog;

namespace DEF.Test
{
    [RequireComponent(typeof(DialogAgent))]
    public class TestNPC : DialogResponder
    {
        [SerializeField] private string m_desiredItem = "";

        public void Initialize(string dialogJSON)
        {
            GetComponent<DialogAgent>().Initialize(name, dialogJSON);
            // GetComponent<DialogAgent>().InitializeStatic("<<self>>", "I always say the same thing.");
        }

        public override bool DialogFunction(string call, string parameter, out string result)
        {
            result = null;
            switch (call)
            {
                case "GiveToPlayer":
                    TestInventoryManager.Add(parameter);
                    return true;
                case "TakeFromPlayer":
                    TestInventoryManager.Remove(parameter);
                    return true;
            }
            return false;
        }

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