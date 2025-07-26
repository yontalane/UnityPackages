using TMPro;
using UnityEngine;

namespace Yontalane.Demos.Dialog
{
    /// <summary>
    /// Handles the UI representation of a single inventory item, including its name and count, and provides methods to modify the item count.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryItemUI : MonoBehaviour
    {
        [Header("References")]

        [SerializeField] private TMP_Text m_nameText = null;
        [SerializeField] private TMP_Text m_countText = null;

        /// <summary>
        /// The name of this inventory item.
        /// </summary>
        public string Name
        {
            get => m_nameText.text;
            set => m_nameText.text = value;
        }

        /// <summary>
        /// How many items are in this stack?
        /// </summary>
        public int Count
        {
            get => int.TryParse(m_countText.text, out int result) ? result : 0;
            set => m_countText.text = value.ToString();
        }

        /// <summary>
        /// Add one to this stack of items.
        /// </summary>
        public void AddItem() => Count++;

        /// <summary>
        /// Remove one from this stack of items.
        /// </summary>
        public void RemoveItem()
        {
            int count = Count;
            count = Mathf.Max(count - 1, 0);
            Count = count;
        }
    }
}