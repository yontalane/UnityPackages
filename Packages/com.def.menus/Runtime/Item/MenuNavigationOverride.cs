using UnityEngine;
using UnityEngine.UI;

namespace DEF.Menus.Item
{
    [DisallowMultipleComponent, RequireComponent(typeof(Selectable))]
    public sealed class MenuNavigationOverride : MenuComponent
    {
		[SerializeField] private bool m_isNavigable = false;
		public bool IsNavigable => m_isNavigable;
    }
}