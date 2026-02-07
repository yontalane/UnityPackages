using System.Collections.Generic;
using UnityEngine;

namespace Yontalane.Dialog
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog/Data Storage Container")]
    internal class DataStorageContainer : MonoBehaviour
    {
        [SerializeField]
        private List<DataStorageVar> m_vars = new();

        internal List<DataStorageVar> Vars => m_vars;
    }
}
