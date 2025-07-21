using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yontalane
{
    public static class Utility
    {
        /// <summary>
        /// Gets the total number of sorting layers defined in the project.
        /// </summary>
        public static int LayerCount => SortingLayer.layers.Length;

        /// <summary>
        /// Highlights the specified Selectable by selecting it and updating the EventSystem's selected GameObject.
        /// </summary>
        public static void Highlight(this Selectable selectable)
        {
            selectable.Select();
            selectable.OnSelect(null);

            if (EventSystem.current == null)
            {
                return;
            }

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

            if (root.TryGetComponent(out LayoutGroup parent))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
            }
        }
    }
}
