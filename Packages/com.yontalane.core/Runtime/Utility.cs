using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yontalane
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

        /// <summary>
        /// Sometimes Vertical or Horizontal LayoutGroups don't adjust to match the size of their content. This function forces them to.
        /// </summary>
        /// <param name="root">An object containing all the LayoutGroups you want to refresh.</param>
        public static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
        {
            LayoutGroup[] componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);

            foreach (LayoutGroup layoutGroup in componentsInChildren)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            }

            LayoutGroup parent = root.GetComponent<LayoutGroup>();
            if (parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
            }
        }
    }
}
