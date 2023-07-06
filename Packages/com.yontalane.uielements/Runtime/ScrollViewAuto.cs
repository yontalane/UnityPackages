using System.Collections.Generic;
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

        private void FocusListener(FocusEvent e)
        {
            if (e.target is VisualElement targetAsVisualElement)
            {
                ScrollTo(targetAsVisualElement);
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
            element.RegisterCallback<FocusEvent>(FocusListener);
        }

        private void UnregisterElement(VisualElement element)
        {
            if (!m_registeredChildren.Contains(element))
            {
                return;
            }
            m_registeredChildren.Remove(element);
            element.UnregisterCallback<FocusEvent>(FocusListener);
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
