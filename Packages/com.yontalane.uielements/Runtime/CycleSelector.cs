using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.UIElements
{
    /// <summary>
    /// A custom UIElements control that displays a static label, a value label, and left/right arrow buttons
    /// for cycling through a list of string choices. Mirrors the public API of <see cref="DropdownField"/>.
    /// </summary>
    [UxmlElement]
    public partial class CycleSelector : VisualElement, INotifyValueChanged<string>
    {
        #region Events

        public delegate void CycleSelectorValueSelectedHandler(string value);

        /// <summary>
        /// Invoked only when a genuine user interaction -- a pointer click on an arrow button, or
        /// directional navigation handled internally via <see cref="LeftRightNav"/> -- changes the
        /// selected value. Unlike <see cref="RegisterValueChangedCallback"/> (which also fires for
        /// programmatic changes via <see cref="value"/>, <see cref="index"/>, SetValueWithoutNotify, or
        /// the choices-mutation methods below), this is safe to use for a selection sound effect without
        /// it misfiring while a menu is being populated from saved state before the user has touched it.
        /// </summary>
        public CycleSelectorValueSelectedHandler OnValueSelected;

        #endregion

        private const string STYLESHEET_RESOURCE = "YontalaneCycleSelector";
        private const string ICON_RESOURCE = "RightArrow";
        private const string FOCUSED_STYLE_CLASS = "focused";

        #region Private Fields

        private readonly Label m_labelElement;
        private readonly IconButton m_previousButton;
        private readonly Label m_valueLabel;
        private readonly IconButton m_nextButton;

        private List<string> m_choices = new();
        private int m_index = -1;
        private bool m_loopable = true;
        private bool m_leftRightNav = true;

        #endregion

        #region Uxml Attributes

        /// <summary>
        /// The optional static text label displayed to the left of the control.
        /// </summary>
        [Tooltip("The optional static text label displayed to the left of the control.")]
        [UxmlAttribute]
        public string label
        {
            get => m_labelElement.text;
            set
            {
                m_labelElement.text = value;
                m_labelElement.style.display = !string.IsNullOrEmpty(value) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// The list of choices to cycle through.
        /// </summary>
        [Tooltip("The list of choices to cycle through.")]
        [UxmlAttribute]
        public List<string> choices
        {
            get => m_choices;
            set
            {
                m_choices = value ?? new List<string>();
                RefreshAfterChoicesChanged();
            }
        }

        /// <summary>
        /// Whether cycling loops from the last choice back to the first (and vice versa).
        /// </summary>
        [Tooltip("Whether cycling loops from the last choice back to the first (and vice versa).")]
        [UxmlAttribute]
        public bool Loopable
        {
            get => m_loopable;
            set
            {
                m_loopable = value;
                RefreshInteractable();
            }
        }

        /// <summary>
        /// Whether the CycleSelector itself listens for left/right navigation input. When true, the arrow
        /// buttons are excluded from the tab order. When false, the arrow buttons are individually focusable
        /// and it's up to the developer to set up their own navigation.
        /// </summary>
        [Tooltip("Whether the CycleSelector itself listens for left/right navigation input. When true, the arrow buttons are excluded from the tab order. When false, the arrow buttons are individually focusable and it's up to the developer to set up their own navigation.")]
        [UxmlAttribute]
        public bool LeftRightNav
        {
            get => m_leftRightNav;
            set
            {
                m_leftRightNav = value;
                m_previousButton.focusable = !value;
                m_nextButton.focusable = !value;
            }
        }

        /// <summary>
        /// The index of the currently selected choice, or -1 if there are no choices.
        /// </summary>
        [Tooltip("The index of the currently selected choice, or -1 if there are no choices.")]
        [UxmlAttribute]
        public int index
        {
            get => m_index;
            set
            {
                int clamped = m_choices.Count > 0 ? Mathf.Clamp(value, 0, m_choices.Count - 1) : -1;
                if (clamped == m_index)
                {
                    return;
                }

                // TEMPORARY [NavDiag4] diagnostic -- full stack trace on every real invocation of this
                // setter, to identify what's calling it automatically on menu open with no user input.
                // Remove once the caller is confirmed.
                Debug.Log($"[NavDiag4] CycleSelector({name}) index setter invoked: {m_index} -> {clamped}, " +
                    $"ChoiceCount={m_choices.Count}, panel={(panel != null)}\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}");

                string previousValue = this.value;
                SetIndexWithoutNotify(clamped);
                string newValue = this.value;

                // No panel means this is the index UXML attribute applying during initial construction,
                // before any listener could exist -- nothing to notify.
                if (panel == null)
                {
                    return;
                }

                // Deferred to escape the caller's own dispatch context. Both real triggers for this
                // setter -- OnNavigationMove and the arrow buttons' clicked callbacks -- run from inside
                // an already-active event dispatch (a NavigationMoveEvent or a pointer event), so calling
                // SendEvent here directly would itself be a reentrant dispatch. Unity's dispatcher can
                // queue a reentrant SendEvent instead of delivering it immediately, but disposing the
                // pooled ChangeEvent right after (via the using block) doesn't wait for that -- so a
                // later, unrelated ChangeEvent<string>.GetPooled call elsewhere (this pool is shared
                // across every string-valued control) can grab and overwrite that same recycled slot
                // before the original queued delivery happens, corrupting it by the time a real listener
                // sees it. Confirmed via a project reproducing delivery with an empty newValue, a stale
                // unrelated previousValue, and a target that wasn't even this element. Scheduling this
                // makes the SendEvent call below a top-level, non-reentrant dispatch, which Unity
                // delivers immediately and safely -- matching how this same package already defers other
                // actions (DelayedFocusElement, DropdownPopupWidthFix) to escape same-frame timing hazards.
                schedule.Execute(() =>
                {
                    using ChangeEvent<string> evt = ChangeEvent<string>.GetPooled(previousValue, newValue);
                    evt.target = this;
                    SendEvent(evt);
                });
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The currently selected choice.
        /// </summary>
        public string value
        {
            get => m_index >= 0 && m_index < m_choices.Count ? m_choices[m_index] : string.Empty;
            set
            {
                int newIndex = m_choices.IndexOf(value);
                if (newIndex < 0)
                {
                    return;
                }
                index = newIndex;
            }
        }

        /// <summary>
        /// The text currently displayed by the control. Mirrors <see cref="value"/>.
        /// </summary>
        public string text => value;

        #endregion

        #region Constructor

        public CycleSelector()
        {
            AddToClassList("yontalane-cycle-selector");
            focusable = true;

            m_labelElement = new()
            {
                name = "yontalane-cycle-selector-label",
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            Add(m_labelElement);

            VisualElement control = new()
            {
                name = "yontalane-cycle-selector-control",
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            Add(control);

            m_previousButton = new()
            {
                name = "yontalane-cycle-selector-previous-button",
                focusable = !m_leftRightNav,
            };
            m_previousButton.AddToClassList("yontalane-cycle-selector-previous-button");
            m_previousButton.Icon = Resources.Load<Sprite>(ICON_RESOURCE);
            m_previousButton.clicked += SelectPrevious;
            control.Add(m_previousButton);

            m_valueLabel = new()
            {
                name = "yontalane-cycle-selector-value-label",
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            control.Add(m_valueLabel);

            m_nextButton = new()
            {
                name = "yontalane-cycle-selector-next-button",
                focusable = !m_leftRightNav,
            };
            m_nextButton.AddToClassList("yontalane-cycle-selector-next-button");
            m_nextButton.Icon = Resources.Load<Sprite>(ICON_RESOURCE);
            m_nextButton.clicked += SelectNext;
            control.Add(m_nextButton);

            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<NavigationMoveEvent>(OnNavigationMove);

            styleSheets.Add(Resources.Load<StyleSheet>(STYLESHEET_RESOURCE));

            label = string.Empty;
            RefreshLabel();
            RefreshInteractable();
        }

        #endregion

        #region Choices Management

        /// <summary>
        /// The number of choices currently in <see cref="choices"/>.
        /// </summary>
        public int ChoiceCount => m_choices.Count;

        /// <summary>
        /// Sets the list of choices to cycle through.
        /// </summary>
        /// <param name="newChoices">The new choices.</param>
        public void SetChoices(IReadOnlyList<string> newChoices)
        {
            choices = newChoices != null ? new List<string>(newChoices) : new List<string>();
        }

        /// <summary>
        /// Adds a single choice to the end of <see cref="choices"/>.
        /// </summary>
        /// <param name="s">The choice to add.</param>
        public void AddChoice(string s)
        {
            m_choices.Add(s);
            RefreshAfterChoicesChanged();
        }

        /// <summary>
        /// Adds a range of choices to the end of <see cref="choices"/>.
        /// </summary>
        /// <param name="s">The choices to add.</param>
        public void AddChoiceRange(IReadOnlyList<string> s)
        {
            if (s == null)
            {
                return;
            }
            m_choices.AddRange(s);
            RefreshAfterChoicesChanged();
        }

        /// <summary>
        /// Removes all choices from <see cref="choices"/>.
        /// </summary>
        public void ClearChoices()
        {
            m_choices.Clear();
            RefreshAfterChoicesChanged();
        }

        /// <summary>
        /// Inserts a choice into <see cref="choices"/> at the given index.
        /// </summary>
        /// <param name="i">The index to insert at.</param>
        /// <param name="s">The choice to insert.</param>
        public void InsertChoice(int i, string s)
        {
            m_choices.Insert(i, s);
            RefreshAfterChoicesChanged();
        }

        /// <summary>
        /// Removes the first occurrence of a choice from <see cref="choices"/>, if present.
        /// </summary>
        /// <param name="s">The choice to remove.</param>
        public void RemoveChoice(string s)
        {
            m_choices.Remove(s);
            RefreshAfterChoicesChanged();
        }

        /// <summary>
        /// Removes the choice at the given index from <see cref="choices"/>.
        /// </summary>
        /// <param name="i">The index to remove.</param>
        public void RemoveChoiceAt(int i)
        {
            m_choices.RemoveAt(i);
            RefreshAfterChoicesChanged();
        }

        /// <summary>
        /// Returns the index of a choice within <see cref="choices"/>, or -1 if it isn't present.
        /// </summary>
        /// <param name="s">The choice to look up.</param>
        public int IndexOfChoice(string s) => m_choices.IndexOf(s);

        /// <summary>
        /// Re-clamps the selected index against the current choice count and refreshes the displayed
        /// label and button enabled-state. Called after any change to the contents of <see cref="choices"/>,
        /// whether via that property's setter or one of the incremental mutation methods above.
        /// </summary>
        private void RefreshAfterChoicesChanged()
        {
            m_index = m_choices.Count > 0 ? Mathf.Clamp(m_index, 0, m_choices.Count - 1) : -1;
            RefreshLabel();
            RefreshInteractable();
        }

        #endregion

        #region Value Management

        /// <summary>
        /// Sets the currently selected choice without invoking the value-changed callback.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public void SetValueWithoutNotify(string newValue)
        {
            int newIndex = m_choices.IndexOf(newValue);
            if (newIndex < 0)
            {
                return;
            }
            SetIndexWithoutNotify(newIndex);
        }

        private void SetIndexWithoutNotify(int newIndex)
        {
            m_index = newIndex;
            RefreshLabel();
            RefreshInteractable();
        }

        /// <summary>
        /// Selects the previous choice, looping to the last choice if <see cref="Loopable"/> is true.
        /// </summary>
        public void SelectPrevious()
        {
            if (!CanSelectPrevious)
            {
                return;
            }

            int newIndex = m_index - 1;
            if (newIndex < 0)
            {
                newIndex = m_choices.Count - 1;
            }
            index = newIndex;
            OnValueSelected?.Invoke(value);
        }

        /// <summary>
        /// Selects the next choice, looping to the first choice if <see cref="Loopable"/> is true.
        /// </summary>
        public void SelectNext()
        {
            if (!CanSelectNext)
            {
                return;
            }

            int newIndex = m_index + 1;
            if (newIndex >= m_choices.Count)
            {
                newIndex = 0;
            }
            index = newIndex;
            OnValueSelected?.Invoke(value);
        }

        private bool CanSelectPrevious => m_choices.Count > 1 && (m_loopable || m_index > 0);

        private bool CanSelectNext => m_choices.Count > 1 && (m_loopable || m_index < m_choices.Count - 1);

        private void RefreshLabel() => m_valueLabel.text = value;

        private void RefreshInteractable()
        {
            m_previousButton.SetEnabled(CanSelectPrevious);
            m_nextButton.SetEnabled(CanSelectNext);
        }

        #endregion

        #region Focus and Navigation

        private void OnFocusIn(FocusInEvent _) => m_valueLabel.AddToClassList(FOCUSED_STYLE_CLASS);

        private void OnFocusOut(FocusOutEvent _) => m_valueLabel.RemoveFromClassList(FOCUSED_STYLE_CLASS);

        private void OnNavigationMove(NavigationMoveEvent e)
        {
            if (!m_leftRightNav)
            {
                return;
            }

            switch (e.direction)
            {
                case NavigationMoveEvent.Direction.Left:
                    SelectPrevious();
                    break;
                case NavigationMoveEvent.Direction.Right:
                    SelectNext();
                    break;
                default:
                    return;
            }

            e.StopPropagation();
            focusController.IgnoreEvent(e);
        }

        #endregion
    }
}
