using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.UIElements
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/UI Elements/Simple Menu Manager")]
    public class SimpleMenuManager : MenuManager
    {
        public class MenuEvent : UnityEvent<string, string> { }
        public class SimpleMenuEvent : UnityEvent<string> { }

        public MenuEvent OnMenuItem;
        public SimpleMenuEvent OnButton;

        private void Reset()
        {
            OnMenuItem = null;
            OnButton = null;
        }

        protected override void OnClick(string menu, string item)
        {
            OnMenuItem?.Invoke(menu, item);
            OnButton?.Invoke(item);
        }
    }
}
