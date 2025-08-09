using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    /// <summary>
    /// A custom ScrollView that automatically registers its child elements for navigation and interaction.
    /// </summary>
    [UxmlElement]
    public partial class ScrollViewAuto : ScrollView
    {
        private readonly VisualElement m_container;
        private readonly List<VisualElement> m_registeredChildren;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollViewAuto"/> class,
        /// setting up automatic registration of child elements for navigation and interaction.
        /// </summary>
        public ScrollViewAuto()
        {
            // Initialize the container by querying for the "unity-content-container" VisualElement.
            m_container = this.Q<VisualElement>("unity-content-container");
            // Initialize the list to keep track of registered child elements.
            m_registeredChildren = new();

            // Register a callback to handle the AttachToPanelEvent, which occurs when the element is added to the UI panel.
            RegisterCallback<AttachToPanelEvent>((e) =>
            {
                // Only perform registration if the application is running (play mode).
                if (Application.isPlaying)
                {
                    // Get all direct children of this ScrollView.
                    IEnumerable<VisualElement> children = Children();
                    // Register each child element for navigation and interaction.
                    foreach (VisualElement child in children)
                    {
                        RegisterElement(child);
                    }
                }
            });
        }

        /// <summary>
        /// Handles navigation move events (e.g., up/down arrow keys) to move focus between child elements within the ScrollView.
        /// </summary>
        /// <param name="e">The navigation move event data.</param>
        private void NavigationMoveListener(NavigationMoveEvent e)
        {
            // Check if the event target is a VisualElement; if not, exit early.
            if (e.target is not VisualElement element)
            {
                return;
            }

            // Get all children of the container, the index of the current element, and the total child count.
            IEnumerable<VisualElement> children = m_container.Children();
            int index = m_container.IndexOf(element);
            int count = ChildCount;
            VisualElement target = null;

            // Handle navigation in the Up direction: wrap to last if at the top, otherwise move to previous.
            if (e.direction == NavigationMoveEvent.Direction.Up)
            {
                if (index == 0)
                {
                    target = children.Last();
                }
                else
                {
                    target = children.ElementAt(index - 1);
                }
            }
            // Handle navigation in the Down direction: wrap to first if at the bottom, otherwise move to next.
            else if (e.direction == NavigationMoveEvent.Direction.Down)
            {
                if (index == count - 1)
                {
                    target = children.First();
                }
                else
                {
                    target = children.ElementAt(index + 1);
                }
            }

            // If no valid target was found, exit early.
            if (target == null)
            {
                return;
            }

            // Move focus to the target element and scroll it into view.
            target.Focus();
            ScrollToChild(target);
        }

        /// <summary>
        /// Scrolls the view to bring the specified child <see cref="VisualElement"/> into view, centering it vertically if possible.
        /// </summary>
        /// <param name="child">The child element to scroll to.</param>
        private void ScrollToChild(VisualElement child)
        {
            if (m_container.worldBound.height <= worldBound.height)
            {
                scrollOffset = new()
                {
                    x = scrollOffset.x,
                    y = 0f
                };
            }
            else if (child != null)
            {
                scrollOffset = new()
                {
                    x = scrollOffset.x,
                    y = child.layout.center.y - contentViewport.layout.size.y * 0.5f
                };
            }
        }

        /// <summary>
        /// Returns an enumerable collection of the child <see cref="VisualElement"/> objects contained within the scroll view.
        /// </summary>
        public new IEnumerable<VisualElement> Children()
        {
            return m_container.Children();
        }

        /// <summary>
        /// Gets the number of child elements contained in the scroll view.
        /// </summary>
        public int ChildCount => m_container.childCount;

        private void RegisterElement(VisualElement element)
        {
            if (m_registeredChildren.Contains(element))
            {
                return;
            }
            m_registeredChildren.Add(element);
            element.RegisterCallback<NavigationMoveEvent>(NavigationMoveListener);
        }

        private void UnregisterElement(VisualElement element)
        {
            if (!m_registeredChildren.Contains(element))
            {
                return;
            }
            m_registeredChildren.Remove(element);
            element.UnregisterCallback<NavigationMoveEvent>(NavigationMoveListener);
        }

        #region Add & Remove
        /// <summary>
        /// Removes all child elements from the scroll view.
        /// </summary>
        public new void Clear()
        {
            for (int i = ChildCount - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
        }

        /// <summary>
        /// Adds a <see cref="VisualElement"/> to the scroll view and registers it for navigation events.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public new void Add(VisualElement element)
        {
            RegisterElement(element);
            m_container.Add(element);
        }

        /// <summary>
        /// Inserts a <see cref="VisualElement"/> at the specified index and registers it for navigation events.
        /// </summary>
        /// <param name="index">The index at which to insert the element.</param>
        /// <param name="element">The element to insert.</param>
        public new void Insert(int index, VisualElement element)
        {
            RegisterElement(element);
            m_container.Insert(index, element);
        }

        /// <summary>
        /// Adds a range of <see cref="VisualElement"/> objects to the scroll view.
        /// </summary>
        /// <param name="elements">The elements to add.</param>
        public void AddRange(IReadOnlyList<VisualElement> elements)
        {
            foreach (VisualElement element in elements)
            {
                Add(element);
            }
        }

        /// <summary>
        /// Removes a <see cref="VisualElement"/> from the scroll view and unregisters it from navigation events.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public new void Remove(VisualElement element)
        {
            UnregisterElement(element);
            m_container.Remove(element);
        }

        /// <summary>
        /// Removes a <see cref="VisualElement"/> from the scroll view by its name.
        /// </summary>
        /// <param name="elementName">The name of the element to remove.</param>
        public void Remove(string elementName)
        {
            VisualElement element = m_container.Q<VisualElement>(elementName);
            if (element != null)
            {
                Remove(element);
            }
        }

        /// <summary>
        /// Removes the child element at the specified index from the scroll view and unregisters it from navigation events.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        public new void RemoveAt(int index)
        {
            IEnumerable<VisualElement> children = Children();
            int i = 0;
            foreach (VisualElement child in children)
            {
                if (i == index)
                {
                    UnregisterElement(child);
                    m_container.RemoveAt(index);
                    return;
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Clones a <see cref="VisualTreeAsset"/> into the scroll view's container and registers the added elements for navigation events.
        /// </summary>
        /// <param name="treeAsset">The visual tree asset to clone.</param>
        /// <param name="firstElementIndex">The index of the first added element.</param>
        /// <param name="elementAddedCount">The number of elements added.</param>
        public void CloneTreeAsset(VisualTreeAsset treeAsset, out int firstElementIndex, out int elementAddedCount)
        {
            treeAsset.CloneTree(m_container, out firstElementIndex, out elementAddedCount);
            IEnumerable<VisualElement> children = Children();
            int i = 0;
            foreach (VisualElement child in children)
            {
                if (i >= firstElementIndex && i < firstElementIndex + elementAddedCount)
                {
                    RegisterElement(child);
                }
                i++;
            }
        }
        #endregion
    }
}
