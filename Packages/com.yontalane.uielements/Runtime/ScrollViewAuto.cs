using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.ComponentModel;
using System;
using System.Reflection;
using System.Xml.Linq;

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

        private VisualElement m_container;

        #region Constructors
        public ScrollViewAuto() : base()
        {
            m_container = this.Q<VisualElement>("unity-content-container");
            IEnumerable<VisualElement> children = m_container.Children();
            foreach(VisualElement child in children)
            {
                child.RegisterCallback<FocusEvent>(FocusListener);
            }
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

        public new int childCount
        {
            get
            {
                return m_container.childCount;
            }
        }

        #region Add & Remove
        public new void Clear()
        {
            for (int i = childCount - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
        }

        public new void Add(VisualElement element)
        {
            element.RegisterCallback<FocusEvent>(FocusListener);
            m_container.Add(element);
        }

        public new void Insert(int index, VisualElement element)
        {
            element.RegisterCallback<FocusEvent>(FocusListener);
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
            element.UnregisterCallback<FocusEvent>(FocusListener);
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
                    child.UnregisterCallback<FocusEvent>(FocusListener);
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
                    child.RegisterCallback<FocusEvent>(FocusListener);
                }
                i++;
            }
        }
        #endregion
    }
}
