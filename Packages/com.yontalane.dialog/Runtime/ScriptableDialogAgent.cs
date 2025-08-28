using System.Collections.Generic;
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

        [Tooltip("The agent's name in dialog. If blank, will default to the name of this asset.")]
        [SerializeField]
        private string m_displayName = "";

        [Tooltip("The type of dialog script this object expects.")]
        [SerializeField]
        private DialogAgentInputType m_inputType = DialogAgentInputType.Json;

        [Tooltip("A complex dialog script. Used to generate DialogData.")]
        [SerializeField]
        private DialogDataContainer m_data = null;

        [Tooltip("The starting node.")]
        [SerializeField]
        private string m_textDataStart = "";

        [Tooltip("A complex dialog script. Used to generate DialogData.")]
        [SerializeField]
        private TextAsset m_textData = null;

        [Tooltip("A complex dialog script. Used to generate DialogData.")]
        [SerializeField]
        private TextAsset m_json = null;

        [Tooltip("A single, unchanging line of dialog. Used in place of a complex script for simple things like signs.")]
        [SerializeField]
        [TextArea]
        private string m_staticText = "";

        [Header("Keyword Replacement")]

        [Tooltip("Text to replace keywords in dialog.")]
        [SerializeField]
        private KeywordPair[] m_keywords = new KeywordPair[0];

        [Header("Inline Image Replacement")]

        [Tooltip("Info for swapping text with inline images.")]
        [SerializeField]
        private InlineImageReplacementInfo[] m_inlineImageReplacementInfo = new InlineImageReplacementInfo[0];

        [Header("Line Builder")]

        [Tooltip("Event for building a line.")]
        [SerializeField]
        private LineBuilder m_lineBuilder = null;

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
        /// The type of dialog script this agent expects (e.g., Json, TextData, String).
        /// </summary>
        public DialogAgentInputType InputType
        {
            get
            {
                return m_inputType;
            }
            set
            {
                m_inputType = value;
            }
        }

        /// <summary>
        /// The TextAsset containing dialog data if using TextData input type.
        /// Returns null if not using TextData.
        /// Setting this also sets the input type to TextData and clears the start node and dialog data.
        /// </summary>
        public TextAsset TextData
        {
            get
            {
                if (m_inputType != DialogAgentInputType.TextData)
                {
                    return null;
                }
                return m_textData;
            }
            set
            {
                m_inputType = DialogAgentInputType.TextData;
                m_textData = value;
                m_textDataStart = string.Empty;
                Data = null;
            }
        }

        /// <summary>
        /// The starting node for TextData dialog scripts.
        /// Returns empty string if not using TextData.
        /// </summary>
        public string TextDataStart
        {
            get
            {
                if (m_inputType != DialogAgentInputType.TextData)
                {
                    return string.Empty;
                }
                return m_textDataStart;
            }
            set
            {
                m_textDataStart = value;
            }
        }

        /// <summary>
        /// The static text for this agent if using String input type.
        /// Returns empty string if not using String input type.
        /// Setting this also sets the input type to String and clears dialog data.
        /// </summary>
        public string StaticText
        {
            get
            {
                if (m_inputType != DialogAgentInputType.String)
                {
                    return string.Empty;
                }
                return m_staticText;
            }
            set
            {
                m_inputType = DialogAgentInputType.String;
                Data = null;
                m_staticText = value;
            }
        }

        /// <summary>
        /// Sets the dialog agent to use TextData input type with the specified TextAsset and start node.
        /// Clears any existing dialog data.
        /// </summary>
        /// <param name="textAsset">The TextAsset containing the dialog script.</param>
        /// <param name="startNode">The starting node for the dialog script.</param>
        public void SetTextData(TextAsset textAsset, string startNode)
        {
            m_inputType = DialogAgentInputType.TextData;
            m_textData = textAsset;
            m_textDataStart = startNode;
            Data = null;
        }

        /// <summary>
        /// Sets the dialog agent to use String input type with the specified static text.
        /// Clears any existing dialog data.
        /// </summary>
        /// <param name="text">The static text to use for dialog.</param>
        public void SetStaticText(string text)
        {
            m_inputType = DialogAgentInputType.String;
            Data = null;
            m_staticText = text;
        }

        /// <summary>
        /// Sets the dialog agent to use TextData input type with the specified TextAsset and an empty start node.
        /// </summary>
        /// <param name="textAsset">The TextAsset containing the dialog script.</param>
        public void SetTextData(TextAsset textAsset) => SetTextData(textAsset, string.Empty);

        /// <summary>
        /// Clears the current dialog data.
        /// </summary>
        public void ClearData() => Data = null;

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
                switch (m_inputType)
                {
                    case DialogAgentInputType.Data:
                        // Initialize dialog data from the assigned DialogData asset.
                        ID = name;
                        Data = m_data.data;
                        break;

                    case DialogAgentInputType.Json:
                        // Parse dialog data from the provided JSON TextAsset.
                        ID = name;
                        Data = JsonUtility.FromJson<DialogData>(m_json.text);
                        break;

                    case DialogAgentInputType.String:
                        // Create a simple dialog node with static text and the provided speaker.
                        ID = DialogAgent.STATIC_ID;
                        Data = new DialogData
                        {
                            nodes = new NodeData[1]
                        };
                        Data.nodes[0] = new NodeData
                        {
                            lines = new LineData[1]
                        };
                        Data.nodes[0].lines[0] = new LineData
                        {
                            speaker = speaker,
                            text = m_staticText
                        };
                        break;

                    case DialogAgentInputType.TextData:
                        // Convert TextAsset dialog data using the specified start node.
                        ID = name;
                        Data = TextDataConverter.Convert(m_textData, m_textDataStart);
                        break;
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

        /// <summary>
        /// Adds this agent's inline image replacement information to the provided list.
        /// </summary>
        /// <param name="info">A list to which inline image replacement info will be added.</param>
        /// <returns>True if any inline image info was added; otherwise, false.</returns>
        public bool GetInlineImageInfo(List<InlineImageReplacementInfo> info)
        {
            if (m_inlineImageReplacementInfo == null || m_inlineImageReplacementInfo.Length == 0)
            {
                return false;
            }

            info.AddRange(m_inlineImageReplacementInfo);
            return true;
        }

        /// <summary>
        /// Invokes the line builder event to construct or modify a LineData object for a given dialog line.
        /// </summary>
        /// <param name="call">The text of the dialog line to process.</param>
        /// <param name="lineData">The original <see cref="LineData"/> that can be modified.</param>
        /// <returns>True if the line builder was invoked; otherwise, false.</returns>
        public bool GetLineDataBuilderResult(string call, LineData lineData)
        {
            // Create a new LineData object to hold the result.
            lineData = null;

            // If a line builder event is assigned, invoke it to process the line.
            if (m_lineBuilder != null)
            {
                bool result = false;
                m_lineBuilder?.Invoke(call, lineData, (callback) => { result = callback; });
                return result;
            }
            else
            {
                // If no line builder is assigned, return false.
                return false;
            }
        }
    }
}
