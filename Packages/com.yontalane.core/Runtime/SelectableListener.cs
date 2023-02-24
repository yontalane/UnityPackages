using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yontalane
{
    [RequireComponent(typeof(Button))]
    public class SelectableListener : MonoBehaviour, ISelectHandler
    {
        #region Serialized Fields
        [SerializeField]
        private UnityEvent m_onSelect;

        [Header("Audio")]

        [SerializeField]
        private AudioClip m_clip;

        [SerializeField]
        [RangeAttribute(0f, 1f)]
        private float m_volume;
        #endregion

        #region Accessors
        public float Volume
        {
            get
            {
                return m_volume;
            }
            set
            {
                m_volume = value;
            }
        }
        #endregion

        private void Reset()
        {
            m_onSelect = null;
            m_clip = null;
            m_volume = 1f;
        }

        public void OnPointerEnter(PointerEventData eventData) => Activate();

        public void OnSelect(BaseEventData eventData) => Activate();

        public void Activate()
        {
            if (m_clip != null)
            {
                AudioSource.PlayClipAtPoint(m_clip, Camera.main.transform.position, m_volume);
            }
            m_onSelect?.Invoke();
        }
    }
}
