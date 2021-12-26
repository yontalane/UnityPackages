using UnityEngine;
using UnityEngine.UI;

namespace DEF.Menus.Item
{
    [DisallowMultipleComponent, RequireComponent(typeof(Selectable))]
    public abstract class InteractableMenuItem : MenuComponent
    {
        private void OnEnable()
        {
            if (MenuInput == null) return;

            MenuInput.OnInputEvent += OnInputEventInternal;
        }

        private void OnDisable()
        {
            if (MenuInput == null) return;

            MenuInput.OnInputEvent -= OnInputEventInternal;
        }

        public bool IsSelected => MenuUtility.EventSystem.currentSelectedGameObject == gameObject;

        private void OnInputEventInternal(MenuInputEvent e)
        {
            if (IsSelected)
            {
                OnInputEvent(e);
            }
        }

        protected abstract void OnInputEvent(MenuInputEvent e);
    }
}