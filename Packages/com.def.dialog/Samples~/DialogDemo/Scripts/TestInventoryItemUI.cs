using UnityEngine;
using UnityEngine.UI;

namespace DEF.Test
{
    public class TestInventoryItemUI : MonoBehaviour
    {
        [SerializeField] private Text m_nameText = null;
        [SerializeField] private Text m_countText = null;

        public string Name
        {
            get { return m_nameText.text; }
            set { m_nameText.text = value; }
        }

        public int Count
        {
            get
            {
                if (int.TryParse(m_countText.text, out int result))
                    return result;
                return 0;
            }
            set { m_countText.text = value.ToString(); }
        }

        public void AddItem()
        {
            Count++;
        }

        public void RemoveItem()
        {
            int count = Count;
            count = Mathf.Max(count - 1, 0);
            Count = count;
        }
    }
}