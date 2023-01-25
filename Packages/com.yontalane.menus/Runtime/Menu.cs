using System;
using System.Collections.Generic;
using System.Linq;
using Yontalane.Menus.Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Yontalane.Menus
{
    public struct MenuActionEvent
    {
        public Selectable item;
        public string itemName;
        public string menuName;
        public Menu menu;
    }

    [AddComponentMenu("Yontalane/Menus/Menu")]
    [DisallowMultipleComponent]
    public sealed class Menu : MenuComponent
    {
        public delegate void ClickDelegate(MenuActionEvent e);
        public static ClickDelegate OnClick = null;

        [Tooltip("The Selectable items within this Menu. If left unassigned, the Menu will attempt to find them at runtime.")]
        public List<Selectable> selectables = new List<Selectable>();
        [Min(0)]
        [Tooltip("The initially selected Selectable.")]
        public int activeSelectable = 0;
        private int m_originalActiveSelectable = 0;
        [Tooltip("Use navigation?")]
        public bool useNavigation = true;
        [Tooltip("Wrap navigation? If wrapping is on, clicking next while selecting the last item moves your selection to the first item. Otherwise, it will do nothing.")]
        public bool wrapNavigation = true;
        [Tooltip("If you exit this Menu and then return to it, should the item that had been selected still be selected?")]
        public bool rememberHighlight = true;
        public bool IsActive { get; private set; } = false;

        private bool m_listenersWereAdded = false;
        private ScrollRect m_scrollRect = null;

        [Space]

        [SerializeField]
        [Tooltip("If you can add new items to this Menu at runtime, this Selectable will be used as the template.")]
        private Selectable m_addableItem = null;

        public void Initialize()
        {
            m_originalActiveSelectable = activeSelectable;

            m_scrollRect = GetComponent<ScrollRect>();

            if (m_addableItem == null) return;

            if (m_addableItem.GetComponent<AddedMenuItem>() == null)
            {
                m_addableItem.gameObject.AddComponent<AddedMenuItem>();
            }
            m_addableItem.gameObject.SetActive(false);
        }

        public void Activate(bool isActive)
        {
            IsActive = isActive;

            if (!rememberHighlight)
            {
                activeSelectable = m_originalActiveSelectable;
            }

            if (selectables.Count == 0)
            {
                if (m_addableItem != null)
                {
                    m_addableItem.gameObject.SetActive(true);
                }
                selectables = GetComponentsInChildren<Selectable>(true).ToList();
                if (m_addableItem != null)
                {
                    m_addableItem.gameObject.SetActive(false);
                }

                for (int i = selectables.Count - 1; i >= 0; i--)
                {
                    for (int j = 0; j < selectables.Count; j++)
                    {
                        if (selectables[j] != selectables[i] && selectables[i].transform.IsDescendantOf(selectables[j].transform))
                        {
                            selectables.RemoveAt(i);
                        }
                    }
                }
            }

            if (isActive)
            {
                for (int i = 0; i < selectables.Count; i++)
                {
                    if (selectables[i] is Selectable s)
                    {
                        InitializeNavigation(s);
                    }

                    if (!m_listenersWereAdded && selectables[i] is Button b)
                    {
                        InitializeListener(b);
                    }
                }
                m_listenersWereAdded = true;
            }
            else
            {
                for (int i = 0; i < selectables.Count; i++)
                {
                    if (selectables[i] is Selectable s)
                    {
                        s.interactable = false;
                    }
                    if (selectables[i] is Button b)
                    {
                        b.onClick.RemoveAllListeners();
                    }
                }
                m_listenersWereAdded = false;
            }

            if (isActive)
            {
                activeSelectable = GetNavigableSelectableIndex(activeSelectable, 1);
                HighlightActiveSelectable();
            }
        }

        private void InitializeNavigation(Selectable s)
        {
            s.RemoveNavigation();
            s.interactable = true;
            if (s.GetComponent<SelectItemAction>() == null)
            {
                s.gameObject.AddComponent<SelectItemAction>();
            }
            s.GetComponent<SelectItemAction>().Menu = this;
        }

        private void InitializeListener(Button b)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() =>
            {
                OnClick?.Invoke(new MenuActionEvent()
                {
                    item = b,
                    itemName = b.name,
                    menuName = name,
                    menu = this
                });
            });
        }

        private void HighlightActiveSelectable()
        {
            if (activeSelectable < 0 || activeSelectable >= selectables.Count) return;

            selectables[activeSelectable].Highlight();

            if (m_scrollRect == null) return;

            ScrollTo(activeSelectable);
        }

        public void HighlightNext()
        {
            int start = activeSelectable;
            activeSelectable++;
            while (activeSelectable != start)
            {
                if (activeSelectable >= selectables.Count)
                {
                    activeSelectable = wrapNavigation ? 0 : selectables.Count - 1;
                }
                if (selectables[activeSelectable].gameObject.activeSelf)
                {
                    break;
                }
                else
                {
                    activeSelectable++;
                }
            }

            activeSelectable = GetNavigableSelectableIndex(activeSelectable, 1);

            HighlightActiveSelectable();
        }

        public void HighlightPrevious()
        {
            int start = activeSelectable;
            activeSelectable--;
            while (activeSelectable != start)
            {
                if (activeSelectable < 0)
                {
                    activeSelectable = wrapNavigation ? selectables.Count - 1 : 0;
                }
                if (selectables[activeSelectable].gameObject.activeSelf)
                {
                    break;
                }
                else
                {
                    activeSelectable--;
                }
            }

            activeSelectable = GetNavigableSelectableIndex(activeSelectable, -1);

            HighlightActiveSelectable();
        }

        public bool TryGetItem(string name, out Selectable item)
        {
            foreach (Selectable selectable in selectables.Where(selectable => selectable.name == name))
            {
                item = selectable;
                return true;
            }
            item = null;
            return false;
        }

        public Selectable GetItem(string name) => TryGetItem(name, out Selectable selectable) ? selectable : null;

        private int GetNavigableSelectableIndex(int index, int directionToCheck = 1)
        {
            if (selectables.Count == 0 || index < 0)
            {
                return -1;
            }

            MenuNavigationOverride o;

            if (selectables.Count > index && selectables[index] != null)
            {
                o = selectables[index].GetComponent<MenuNavigationOverride>();
                if (selectables[index].interactable && ((o != null && o.IsNavigable) || (o == null && useNavigation)))
                {
                    return index;
                }
            }

            directionToCheck = directionToCheck >= 0 ? 1 : -1;
            int i = index + directionToCheck;
            if (i >= selectables.Count)
            {
                i = 0;
            }
            else if (i < 0)
            {
                i = selectables.Count - 1;
            }

            while (i != index)
            {
                if (selectables[i] == null)
                {
                    return -1;
                }

                o = selectables[i].GetComponent<MenuNavigationOverride>();
                if (selectables[i].interactable && ((o != null && o.IsNavigable) || (o == null && useNavigation)))
                {
                    return i;
                }

                i += directionToCheck;
                if (i >= selectables.Count)
                {
                    i = 0;
                }
                else if (i < 0)
                {
                    i = selectables.Count - 1;
                }
            }

            return -1;
        }

        private void ScrollTo(int index)
        {
            Bounds b1 = RectTransformUtility.CalculateRelativeRectTransformBounds(m_scrollRect.content, selectables[index].transform);
            Bounds b2 = RectTransformUtility.CalculateRelativeRectTransformBounds(m_scrollRect.content);
            float positionOfSelectable = Mathf.Abs(b1.center.y) + b1.extents.y * (index < selectables.Count * 0.5f ? -1 : 1);
            float heightOfContentFrame = Mathf.Abs(b2.center.y) + b2.extents.y;
            float res = positionOfSelectable / heightOfContentFrame;
            res = 1f - res;
            m_scrollRect.verticalNormalizedPosition = res;
        }

        #region Adding

        public void Add(string name, string label = null, Selectable targetLocation = null, bool andScrollTo = true)
        {
            if (m_addableItem != null)
            {
                Selectable instance = Instantiate(m_addableItem.gameObject).GetComponent<Selectable>();
                instance.name = name;
                TMP_Text text = instance.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = label ?? name;
                }
                Add(instance, targetLocation, andScrollTo);
            }
        }

        public void Add(Action<Selectable> selectableCreator, Selectable targetLocation = null, bool andScrollTo = true)
        {
            if (m_addableItem == null) return;

            Selectable instance = Instantiate(m_addableItem.gameObject).GetComponent<Selectable>();
            selectableCreator(instance);
            Add(instance, targetLocation, andScrollTo);
        }

        public void Add(Selectable selectable, Selectable targetLocation = null, bool andScrollTo = true)
        {
            if (selectable.GetComponent<AddedMenuItem>() == null)
            {
                selectable.gameObject.AddComponent<AddedMenuItem>();
            }

            selectable.gameObject.SetActive(true);
            InitializeNavigation(selectable);
            if (selectable is Button b)
            {
                InitializeListener(b);
            }

            Transform parent = m_addableItem != null ? m_addableItem.transform.parent : selectables.Count > 0 ? selectables[0].transform.parent : transform;

            int targetIndex = -1;
            if (targetLocation != null)
            {
                targetIndex = selectables.IndexOf(targetLocation);
            }
            if (targetIndex >= 0)
            {
                targetIndex++;
            }
            else
            {
                targetIndex = 0;
                for (int i = selectables.Count - 1; i >= 0; i--)
                {
                    if (selectables[i].GetComponent<AddedMenuItem>() == null) continue;

                    targetIndex = i + 1;
                    break;
                }
                if (targetIndex == 0 && selectables.Count > 0)
                {
                    targetIndex = selectables.Count;
                }
            }

            if (targetIndex == selectables.Count)
            {
                selectables.Add(selectable);
            }
            else
            {
                selectables.Insert(targetIndex, selectable);
            }

            selectable.transform.SetParent(parent, false);
            selectable.transform.localPosition = Vector3.zero;
            selectable.transform.localEulerAngles = Vector3.zero;
            selectable.transform.localScale = Vector3.one;
            selectable.transform.SetSiblingIndex(targetLocation != null ? targetLocation.transform.GetSiblingIndex() + 1 : targetIndex);

            if (andScrollTo)
            {
                ScrollTo(targetIndex);
            }
        }

        public void Remove(Selectable selectable) => Remove(selectable.name);

        public void Remove(string name)
        {
            bool removeAll = string.IsNullOrEmpty(name);

            for (int i = selectables.Count - 1; i >= 0; i--)
            {
                if (selectables[i].GetComponent<AddedMenuItem>() != null && ((removeAll && selectables[i].gameObject.activeSelf) || selectables[i].name == name))
                {
                    Destroy(selectables[i].gameObject);
                    selectables.RemoveAt(i);
                    if (!removeAll)
                    {
                        break;
                    }
                }
            }

            activeSelectable = Mathf.Min(activeSelectable, selectables.Count - 1);
            HighlightActiveSelectable();
        }

        public void Clear() => Remove("");

        #endregion
    }

    [DisallowMultipleComponent, RequireComponent(typeof(Selectable))]
    internal sealed class SelectItemAction : MonoBehaviour, ISelectHandler
    {
        public delegate void SelectItemHandler(Menu menu, Selectable item);
        public static SelectItemHandler OnSelectItem = null;
        public Menu Menu { get; set; } = null;
        public void OnPointerEnter(PointerEventData _) => OnSelectItem?.Invoke(Menu, GetComponent<Selectable>());
        public void OnSelect(BaseEventData _) => OnSelectItem?.Invoke(Menu, GetComponent<Selectable>());
    }

    [DisallowMultipleComponent]
    internal sealed class AddedMenuItem : MonoBehaviour
    {

    }
}