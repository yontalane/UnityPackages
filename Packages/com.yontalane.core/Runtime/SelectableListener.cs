using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yontalane
{
    /// <summary>
    /// A component that listens for selectable events and plays an audio clip.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Yontalane/Selectable Listener")]
    public class SelectableListener : MonoBehaviour, ISelectHandler
    {
        #region Delegates
        /// <summary>
        /// A delegate that represents a method that handles a change in selection.
        /// </summary>
        public delegate void ChangeSelectionHandler(Button button);

        /// <summary>
        /// An event that is triggered when the selection changes.
        /// </summary>
        public ChangeSelectionHandler OnChangeSelection = null;
        #endregion

        #region Serialized Fields
        [Tooltip("Event invoked when the selection changes.")]
        [SerializeField]
        private UnityEvent m_onChangeSelection;

        [Header("Audio")]

        [Tooltip("Audio clip to play when the selection changes.")]
        [SerializeField]
        private AudioClip m_clip;

        [Tooltip("Volume at which the audio clip will be played.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float m_volume;
        #endregion

        #region Accessors
        /// <summary>
        /// Gets or sets the volume at which the audio clip will be played.
        /// </summary>
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
            m_onChangeSelection = null;
            m_clip = null;
            m_volume = 1f;
        }

        /// <summary>
        /// Called when the pointer enters the selectable.
        /// </summary>
        public void OnPointerEnter(PointerEventData _) => Activate();

        /// <summary>
        /// Called when the selectable is selected.
        /// </summary>
        public void OnSelect(BaseEventData _) => Activate();

        /// <summary>
        /// Activates the selectable.
        /// </summary>
        public void Activate()
        {
            if (m_clip != null)
            {
                // Play the audio clip at the specified volume.
                AudioSource.PlayClipAtPoint(m_clip, Camera.main.transform.position, m_volume);
            }

            // Invoke the OnChangeSelection event with the Button component, if any listeners are attached.
            OnChangeSelection?.Invoke(GetComponent<Button>());
            // Invoke the m_onChangeSelection UnityEvent, if any listeners are attached.
            m_onChangeSelection?.Invoke();
        }
    }
}
