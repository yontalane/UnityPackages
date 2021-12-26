using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DEF.Menus.Item
{
    [DisallowMultipleComponent]
    public sealed class CycleSelector : InteractableMenuItem
    {
        [SerializeField] private Text m_text = null;
        [SerializeField, Min(0)] private int m_index = 0;
        public int Index
        {
            get => m_index;
            private set => m_index = value;
        }

        [SerializeField] private string[] m_items = new string[0];
        public string[] Items
        {
            get => m_items;
            set => m_items = value;
        }

        [SerializeField] private Button m_previousButton = null;
        [SerializeField] private Button m_nextButton = null;

        private void Start()
        {
            if (m_text == null)
            {
                m_text = GetComponent<Text>();
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

        protected override void OnInputEvent(MenuInputEvent e)
        {
            if (e.move.x < 0)
            {
                if (m_previousButton != null)
                {
                    ExecuteEvents.Execute(m_previousButton.gameObject, new BaseEventData(MenuUtility.EventSystem), ExecuteEvents.submitHandler);
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
                    ExecuteEvents.Execute(m_nextButton.gameObject, new BaseEventData(MenuUtility.EventSystem), ExecuteEvents.submitHandler);
                }
                else
                {
                    SelectNext();
                }
            }
        }

        public void SelectPrevious()
        {
            Index--;
            if (Index < 0)
            {
                Index = Items.Length - 1;
            }
            RefreshText();
        }

        public void SelectNext()
        {
            Index++;
            if (Index >= Items.Length)
            {
                Index = 0;
            }
            RefreshText();
        }

        public string Value => Items.Length > Index ? Items[Index] : "";

        private void RefreshText() => m_text.text = Value;
    }
}