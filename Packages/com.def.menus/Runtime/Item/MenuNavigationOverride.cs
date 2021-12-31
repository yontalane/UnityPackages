using UnityEngine;
using UnityEngine.UI;

namespace DEF.Menus.Item
{
    [AddComponentMenu("DEF/Menus/Items/Menu Navigation Override")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public sealed class MenuNavigationOverride : MenuComponent
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