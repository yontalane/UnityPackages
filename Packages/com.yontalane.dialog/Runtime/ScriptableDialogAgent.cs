using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Dialog
{
    /// <summary>
    /// A ScriptableObject implementation of IDialogAgent for use in the Yontalane dialog system.
    /// Allows dialog agent data to be stored as assets, supporting dialog data, keyword replacement, and dialog session management.
    /// </summary>
    [CreateAssetMenu(fileName = "Dialog Agent", menuName = "Yontalane/Dialog/Dialog Agent", order = 1)]
    public class ScriptableDialogAgent : ScriptableObject, IDialogAgent
    {
        /// <summary>
        /// A unique identifier for this dialog agent instance.
        /// </summary>
        public string ID { get; private set; } = "";

        /// <summary>
        /// The dialog data associated with this agent.
        /// </summary>
        public DialogData Data { get; private set; } = null;

        [Header("Dialog Script Data")]

        [Tooltip("A complex dialog script. Used to generate DialogData.")]
        [SerializeField]
        private TextAsset m_json = null;

        [Tooltip("A single, unchanging line of dialog. Used in place of a complex script for simple things like signs.")]
        [SerializeField]
        [TextArea]
        private string m_staticText = "";

        [Tooltip("The agent's name in dialog. If blank, will default to the name of this asset.")]
        [SerializeField]
        private string m_displayName = "";

        [Header("Keyword Replacement")]

        [Tooltip("Text to replace keywords in dialog.")]
        [SerializeField]
        private KeywordPair[] m_keywords = new KeywordPair[0];

        /// <summary>
        /// Stores the callback action to be invoked when the dialog session ends.
        /// </summary>
        private UnityAction m_onExitDialog;

        private bool m_enabled = true;
        /// <summary>
        /// Gets or sets whether this dialog agent is enabled.
        /// </summary>
        public bool enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }

        /// <summary>
        /// The display name of the dialog agent, shown to the player.
        /// If not set, defaults to the asset's name.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                return !string.IsNullOrEmpty(m_displayName) ? m_displayName : name;
            }
            set
            {
                m_displayName = value;
            }
        }

        /// <summary>
        /// Initiates a dialog session with the specified speaker and an optional callback to invoke when the dialog ends.
        /// If dialog data is not already set, it will be initialized from the JSON asset if available,
        /// otherwise a simple dialog node will be created using the static text and provided speaker.
        /// </summary>
        /// <param name="speaker">The name of the speaker to use in the dialog.</param>
        /// <param name="onExitDialog">Callback to invoke when the dialog session ends.</param>
        public void InitiateDialog(string speaker, UnityAction onExitDialog)
        {
            // Check if dialog data is missing or empty, and initialize it from JSON or static text as appropriate.
            if (Data == null || (string.IsNullOrEmpty(Data.start) && string.IsNullOrEmpty(Data.windowType) && string.IsNullOrEmpty(Data.data) && Data.nodes.Length == 0))
            {
                // If a JSON asset is assigned, parse dialog data from it.
                if (m_json != null)
                {
                    ID = name;
                    Data = JsonUtility.FromJson<DialogData>(m_json.text);
                }
                // Otherwise, create a simple dialog node with static text and the provided speaker.
                else
                {
                    ID = DialogAgent.STATIC_ID;
                    Data = new()
                    {
                        nodes = new NodeData[1],
                    };
                    Data.nodes[0] = new()
                    {
                        lines = new LineData[1],
                    };
                    Data.nodes[0].lines[0] = new()
                    {
                        speaker = speaker,
                        text = m_staticText,
                    };
                }
            }

            // Store the exit callback and start the dialog session.
            m_onExitDialog = onExitDialog;
            DialogProcessor.InitiateDialog(this, OnExitDialog);
        }

        /// <summary>
        /// Initiates a dialog session with the specified speaker and no exit callback.
        /// </summary>
        /// <param name="speaker">The name of the speaker to use in the dialog.</param>
        public void InitiateDialog(string speaker) => InitiateDialog(speaker, null);

        /// <summary>
        /// Initiates a dialog session with the specified exit callback and no speaker.
        /// </summary>
        /// <param name="onExitDialog">The callback to invoke when the dialog ends.</param>
        public void InitiateDialog(UnityAction onExitDialog) => InitiateDialog(string.Empty, onExitDialog);

        /// <summary>
        /// Initiates a dialog session with no speaker and no exit callback.
        /// </summary>
        public void InitiateDialog() => InitiateDialog(string.Empty, null);

        /// <summary>
        /// Invokes the exit callback when the dialog session ends.
        /// </summary>
        private void OnExitDialog() => m_onExitDialog?.Invoke();

        /// <summary>
        /// Attempts to find a replacement value for the specified keyword using the <c>m_keywords</c> array.
        /// </summary>
        /// <param name="key">The keyword to search for.</param>
        /// <param name="result">The replacement value if found; otherwise, null.</param>
        /// <returns>True if a replacement was found; otherwise, false.</returns>
        public virtual bool GetKeyword(string key, out string result)
        {
            foreach (KeywordPair keywordPair in m_keywords.Where(keywordPair => keywordPair.key.Equals(key)))
            {
                result = keywordPair.value;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Handles custom function calls from dialog scripts.
        /// This base implementation does not handle any functions and always returns false.
        /// Override this method in a derived class to provide custom logic for dialog script functions.
        /// </summary>
        /// <param name="call">The name of the function to execute.</param>
        /// <param name="parameter">A parameter for the function.</param>
        /// <param name="result">The return value of the function, if any.</param>
        /// <returns>True if the function was handled; otherwise, false.</returns>
        public virtual bool DialogFunction(string call, string parameter, out string result)
        {
            result = null;
            return false;
        }
    }
}
