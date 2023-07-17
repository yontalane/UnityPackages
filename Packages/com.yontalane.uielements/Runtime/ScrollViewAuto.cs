using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    public class ScrollViewAuto : ScrollView
    {
        #region UXML Methods
        public new class UxmlFactory : UxmlFactory<ScrollViewAuto, UxmlTraits> { }

        public new class UxmlTraits : ScrollView.UxmlTraits
        {
            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(visualElement, attributes, context);
            }
        }
        #endregion

        private readonly VisualElement m_container;
        private readonly List<VisualElement> m_registeredChildren;

        #region Constructors
        public ScrollViewAuto() : base()
        {
            m_container = this.Q<VisualElement>("unity-content-container");
            m_registeredChildren = new();

            RegisterCallback<AttachToPanelEvent>((e) =>
            {
                if (Application.isPlaying)
                {
                    IEnumerable<VisualElement> children = Children();
                    foreach (VisualElement child in children)
                    {
                        RegisterElement(child);
                    }
                }
            });
        }
        #endregion

        private void NavigationMoveListener(NavigationMoveEvent e)
        {
            if (e.target is not VisualElement element)
            {
                return;
            }

            IEnumerable<VisualElement> children = m_container.Children();
            int index = m_container.IndexOf(element);
            int count = ChildCount;
            VisualElement target = null;

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

            if (target == null)
            {
                return;
            }

            target.Focus();
            ScrollToChild(target);
        }

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

        public new IEnumerable<VisualElement> Children()
        {
            return m_container.Children();
        }

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
        public new void Clear()
        {
            for (int i = ChildCount - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
        }

        public new void Add(VisualElement element)
        {
            RegisterElement(element);
            m_container.Add(element);
        }

        public new void Insert(int index, VisualElement element)
        {
            RegisterElement(element);
            m_container.Insert(index, element);
        }

        public void AddRange(IReadOnlyList<VisualElement> elements)
        {
            foreach (VisualElement element in elements)
            {
                Add(element);
            }
        }

        public new void Remove(VisualElement element)
        {
            UnregisterElement(element);
            m_container.Remove(element);
        }

        public void Remove(string elementName)
        {
            VisualElement element = m_container.Q<VisualElement>(elementName);
            if (element != null)
            {
                Remove(element);
            }
        }

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
