using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DEF
{
    public static class Utility
    {
        /// <summary>
        /// Visually select the target Selectable.
        /// </summary>
        public static void Highlight(this Selectable selectable)
        {
            selectable.Select();
            selectable.OnSelect(null);

            if (EventSystem.current == null) return;

            EventSystem.current.SetSelectedGameObject(selectable.gameObject, new BaseEventData(EventSystem.current));
        }
    }
}
