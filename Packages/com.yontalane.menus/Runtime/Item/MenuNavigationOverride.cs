using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.Menus.Item
{
    [AddComponentMenu("Yontalane/Menus/Items/Menu Navigation Override")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(MenuComponent))]
    public sealed class MenuNavigationOverride : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Is this MenuComponent part of the UI navigation map?")]
        private bool m_isNavigable = false;

        /// <summary>
        /// Is this MenuComponent part of the UI navigation map?
        /// </summary>
		public bool IsNavigable => m_isNavigable;
    }
}