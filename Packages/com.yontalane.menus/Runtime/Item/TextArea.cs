using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.Menus.Item
{
    [AddComponentMenu("Yontalane/Menus/Items/Text Area")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public sealed class TextArea : InteractableMenuItem
    {
        private ScrollRect m_scrollRect = null;

        [Min(0f)]
        [SerializeField]
        [Tooltip("Scroll speed.")]
        private float m_speed = 2.5f;

        [SerializeField]
        [Tooltip("Invert Y axis.")]
        private bool m_reverse = false;

        private void Start() => m_scrollRect = GetComponent<ScrollRect>();

        protected override void OnInputEvent(MenuInputEvent e)
        {
            if (e.scroll.magnitude > 0f)
            {
                float v = m_scrollRect.verticalNormalizedPosition;
                v = Mathf.Clamp(v + (e.scroll.y * m_speed * Time.deltaTime * (m_reverse ? -1 : 1)), 0f, 1f);
                m_scrollRect.verticalNormalizedPosition = v;
            }
        }
    }
}