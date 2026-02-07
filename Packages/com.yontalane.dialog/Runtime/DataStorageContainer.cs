using System.Collections.Generic;
using UnityEngine;

namespace Yontalane.Dialog
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog/Data Storage Container")]
    internal class DataStorageContainer : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private List<DataStorage.DataStorageVar> m_vars = new();

        internal List<DataStorage.DataStorageVar> Vars => m_vars;
    }
}
