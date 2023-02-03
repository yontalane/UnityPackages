using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.Menus.Item
{
    #region Structs
    internal struct BetterButtonEvent
    {
        public BetterButton betterButton;
        public GameObject gameObject;
        public Button button;
        public string name;
        public string label;
    }
    #endregion

    [RequireComponent(typeof(Button))]
    internal class BetterButton : MonoBehaviour
    {
        #region Delegates
        internal delegate void ClickHandler(BetterButtonEvent e);
        internal ClickHandler OnClick = null;
        #endregion

        #region Private Variables
        private Button m_button;
        private TMP_Text m_text;
        #endregion

        /// <summary>
        /// Return the component on the GameObject. If there is no component, add a new one and return that.
        /// </summary>
        internal static BetterButton GetOrAdd(Button button)
        {
            if (button.gameObject.TryGetComponent(out BetterButton existingBetterButton))
            {
                return existingBetterButton;
            }
            else
            {
                return button.gameObject.AddComponent<BetterButton>();
            }
        }

        private void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
            m_button = GetComponent<Button>();
            m_text = GetComponentInChildren<TMP_Text>();
        }

        private void OnEnable() => m_button.onClick.AddListener(ClickListener);

        private void OnDisable() => m_button.onClick.RemoveListener(ClickListener);

        private void ClickListener() => OnClick?.Invoke(new BetterButtonEvent()
        {
            gameObject = gameObject,
            betterButton = this,
            button = m_button,
            name = name,
            label = m_text != null ? m_text.text : string.Empty
        });
    }
}
