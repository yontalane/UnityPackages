using System.Collections.Generic;
using UnityEngine;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Container component for holding a list of DataStorageVar instances used by dialog systems.
    /// Prevents multiple components on the same GameObject and can be added via the Unity editor menu.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog/Data Storage Container")]
    internal class DataStorageContainer : MonoBehaviour
    {
        /// <summary>
        /// The list of variables managed by this storage container.
        /// </summary>
        [Tooltip("List of variables to be stored and managed by this dialog data container.")]
        [SerializeField]
        private List<DataStorageVar> m_vars = new();

        /// <summary>
        /// Gets the list of variables managed by this container.
        /// </summary>
        internal List<DataStorageVar> Vars => m_vars;
    }
}
