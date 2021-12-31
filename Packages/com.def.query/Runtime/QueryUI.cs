using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DEF.Query
{
    [DisallowMultipleComponent]
    [AddComponentMenu("DEF/Query/Query UI")]
    public sealed class QueryUI : MonoBehaviour
    {
        public delegate void QueryUIHandler(QueryUI queryUI);
        public static QueryUIHandler OnQueryUILoaded = null;

        private const string ANIMATION_PARAMETER = "Query Visible";
        private const float BUTTON_HIGHLIGHT_DELAY = 0.25f;

        [Header("Scene UI")]

        [SerializeField]
        [Tooltip("The Animator for controlling the dialog. Must have a \"Query Visible\" boolean parameter.")]
        private Animator m_animator = null;

        [SerializeField]
        [Tooltip("The field for displaying the dialog's message text.")]
        private Text m_text = null;

        [SerializeField]
        [Tooltip("The location to instantiate response buttons.")]
        private RectTransform m_responseContainer = null;

        [SerializeField]
        [Tooltip("An optional audio clip to play when clicking buttons.")]
        private AudioClip m_buttonClick = null;

        [Header("Prefabs")]

        [SerializeField]
        [Tooltip("The prefab to use for the response buttons.")]
        private Button m_responseButtonPrefab = null;

        private static AudioSource s_clickAudioSource = null;

        private Action<string> m_callback = null;
        private readonly List<Button> m_responses = new List<Button>();

        private void Start()
        {
            if (m_animator == null) m_animator = GetComponent<Animator>();
            if (m_text == null) m_text = GetComponentInChildren<Text>();
        }

        /// <summary>
        /// Close the window. QueryUI relies on the Animator to do the closing.
        /// </summary>
        public void Close() => m_animator.SetBool(ANIMATION_PARAMETER, false);

        /// <summary>
        /// Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.
        /// </summary>
        /// <param name="text">The query message.</param>
        /// <param name="responses">The possible responses.</param>
        /// <param name="callback">The function to call when a response is chosen.</param>
        public static void Initiate(string text, string[] responses, Action<string> callback)
        {
            QueryUI queryUI = FindObjectOfType<QueryUI>();

            if (queryUI == null)
            {
                Debug.LogError("QueryUI could not be found.");
                return;
            }

            queryUI.m_callback = callback;
            queryUI.m_text.text = text;

            for (int i = queryUI.m_responses.Count - 1; i >= 0; i--)
            {
                Destroy(queryUI.m_responses[i].gameObject);
            }
            queryUI.m_responses.Clear();

            for (int i = 0; i < responses.Length; i++)
            {
                Button instance = Instantiate(queryUI.m_responseButtonPrefab.gameObject).GetComponent<Button>();
                instance.GetComponentInChildren<Text>().text = responses[i];
                instance.onClick.AddListener(delegate { queryUI.OnClickResponse(instance); });
                instance.GetComponent<RectTransform>().SetParent(queryUI.m_responseContainer);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localScale = Vector3.one;
                instance.navigation = Navigation.defaultNavigation;

                if (i > 0)
                {
                    Button prev = queryUI.m_responses[i - 1];

                    Navigation currNav = instance.navigation;
                    currNav.mode = Navigation.Mode.Explicit;
                    currNav.selectOnLeft = prev;
                    currNav.selectOnUp = prev;
                    instance.navigation = currNav;

                    Navigation prevNav = prev.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    prevNav.selectOnRight = instance;
                    prevNav.selectOnDown = instance;
                    prev.navigation = prevNav;
                }

                queryUI.m_responses.Add(instance);
            }

            if (responses.Length > 0)
            {
                queryUI.m_responses[0].GetComponent<Button>().Highlight();
                queryUI.StartCoroutine(queryUI.DelayedHighlight(queryUI));
            }

            Utility.RefreshLayoutGroupsImmediateAndRecursive(queryUI.m_responseContainer.gameObject);

            queryUI.m_animator.SetBool(ANIMATION_PARAMETER, true);

            OnQueryUILoaded?.Invoke(queryUI);
        }

        /// <summary>
        /// In case the animation involves deactivating the buttons...
        /// </summary>
        private IEnumerator DelayedHighlight(QueryUI queryUI)
        {
            yield return new WaitForSeconds(BUTTON_HIGHLIGHT_DELAY);

            if (queryUI.m_responses.Count > 0)
            {
                queryUI.m_responses[0].GetComponent<Button>().Highlight();
            }
        }

        private void OnClickResponse(Button response)
        {
            if (response == null) return;

            if (m_buttonClick != null) ClickAudioSource.PlayOneShot(m_buttonClick);

            Close();

            m_callback?.Invoke(response.GetComponentInChildren<Text>().text);
        }

        private static AudioSource ClickAudioSource
        {
            get
            {
                if (s_clickAudioSource == null)
                {
                    s_clickAudioSource = new GameObject().AddComponent<AudioSource>();
                    s_clickAudioSource.name = $"{typeof(QueryUI).Name} {typeof(AudioSource).Name}";
                    s_clickAudioSource.playOnAwake = false;
                    s_clickAudioSource.loop = false;
                    s_clickAudioSource.transform.SetParent(Camera.main.transform, false);
                }

                return s_clickAudioSource;
            }
        }
    }
}