using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yontalane.Menus.Item
{
    [AddComponentMenu("Yontalane/Menus/Items/Cycle Selector")]
    [DisallowMultipleComponent]
    public sealed class CycleSelector : InteractableMenuItem
    {
        #region Serialized Fields
        [Header("Info")]

        [SerializeField]
        [Tooltip("The options within the CycleSelector.")]
        private string[] m_items = new string[0];

        [SerializeField]
        [Min(0)]
        [Tooltip("The initially selected option.")]
        private int m_index = 0;

        [Header("References")]

        [SerializeField]
        [Tooltip("If left null, defaults to a Text component attached to this GameObject.")]
        private TMP_Text m_text = null;

        [SerializeField]
        [Tooltip("If you assign a button to this field, CycleSelector will set up the OnClick event so you don't have to. Without a button assigned here, you can still navigate using a controller.")]
        private Button m_previousButton = null;

        [SerializeField]
        [Tooltip("If you assign a button to this field, CycleSelector will set up the OnClick event so you don't have to. Without a button assigned here, you can still navigate using a controller.")]
        private Button m_nextButton = null;

        [Header("Events")]

        public UnityEvent<string> OnChange = null;
        #endregion

        #region Accessors
        /// <summary>
        /// The list of options.
        /// </summary>
        public string[] Items
        {
            get => m_items;
            set
            {
                m_items = value;
                if (m_items != null && m_items.Length > 0)
                {
                    m_index = Mathf.Clamp(m_index, 0, value.Length);
                }
                else
                {
                    m_index = 0;
                }
                RefreshText();
            }
        }

        /// <summary>
        /// The currently selected option.
        /// </summary>
        public int Index
        {
            get => m_index;
            set
            {
                m_index = value;
                RefreshText();
                OnChange?.Invoke(Value);
            }
        }

        /// <summary>
        /// The currently selected option.
        /// </summary>
        public string Value
        {
            get => Items.Length > Index ? Items[Index] : string.Empty;
            set
            {
                
                for (int i = 0; i < Items.Length; i++)
                {
                    if (Items[i] == value)
                    {
                        Index = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Are the previous/next cycle buttons interactable?
        /// </summary>
        public bool CycleInteractable
        {
            get
            {
                return m_previousButton.interactable;
            }
            set
            {
                m_previousButton.interactable = value;
                m_nextButton.interactable = value;
            }
        }
        #endregion

        private void Start()
        {
            if (m_text == null)
            {
                m_text = GetComponent<TMP_Text>();
            }

            RefreshText();

            if (m_previousButton != null)
            {
                m_previousButton.RemoveNavigation();
                m_previousButton.onClick.AddListener(() => SelectPrevious());
            }
            if (m_nextButton != null)
            {
                m_nextButton.RemoveNavigation();
                m_nextButton.onClick.AddListener(() => SelectNext());
            }
        }

        /// <summary>
        /// Set the value of the selector without notifying listeners.
        /// </summary>
        public void SetValueWithoutNotify(string value)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] == value)
                {
                    m_index = i;
                    RefreshText();
                    break;
                }
            }
        }

        protected override void OnInputEvent(MenuInputEvent e)
        {
            if (e.move.x < 0)
            {
                if (m_previousButton != null)
                {
                    ExecuteEvents.Execute(m_previousButton.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                }
                else
                {
                    SelectPrevious();
                }
            }
            else if (e.move.x > 0)
            {
                if (m_nextButton != null)
                {
                    ExecuteEvents.Execute(m_nextButton.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                }
                else
                {
                    SelectNext();
                }
            }
        }

        /// <summary>
        /// Select the previous option.
        /// </summary>
        public void SelectPrevious()
        {
            m_index--;
            if (m_index < 0)
            {
                m_index = Items.Length - 1;
            }
            Index = m_index;
        }

        /// <summary>
        /// Select the next option.
        /// </summary>
        public void SelectNext()
        {
            m_index++;
            if (m_index >= Items.Length)
            {
                m_index = 0;
            }
            Index = m_index;
        }

        private void RefreshText() => m_text.text = Value;
    }
}