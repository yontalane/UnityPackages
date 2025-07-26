using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Handles the processing and flow of dialog interactions, managing dialog lines, speakers, and responses.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog/Dialog Processor")]
    public sealed class DialogProcessor : MonoBehaviour, IDialogResponder
    {
        #region Data Structures
        /// <summary>
        /// Enum representing the type of speaker in a dialog.
        /// </summary>
        public enum SpeakerType
        {
            /// <summary>
            /// The player is speaking.
            /// </summary>
            Player,
            /// <summary>
            /// The owner of the dialog agent is speaking.
            /// </summary>
            Self,
            /// <summary>
            /// Some other character is speaking.
            /// </summary>
            Other
        }

        [Serializable]
        /// <summary>
        /// UnityEvent callback for when a new line of dialog begins.
        /// Parameters:
        ///   LineData: The data for the current dialog line.
        ///   Action&lt;string&gt;: A callback to continue the dialog, accepting a string parameter.
        ///   Func&lt;string, string&gt;: A function to process or modify a string value.
        /// </summary>
        class LineCallback : UnityEvent<LineData, Action<string>, Func<string, string>> { }
        #endregion

        #region Delegates
        /// <summary>
        /// Delegate invoked when the dialog begins.
        /// </summary>
        public delegate void InitiateDialogDelegate();
        /// <summary>
        /// Delegate invoked when the dialog begins.
        /// </summary>
        public InitiateDialogDelegate OnInitiateDialog = null;

        /// <summary>
        /// Delegate invoked when the dialog ends.
        /// </summary>
        public delegate void ExitDialogDelegate();
        /// <summary>
        /// Delegate invoked when the dialog ends.
        /// </summary>
        public ExitDialogDelegate OnExitDialog = null;
        #endregion

        #region Private Fields
        private static DialogProcessor s_instance = null;
        private bool m_isActive = false;
        private NodeData m_nodeData = null;
        private int m_lineIndex = 0;
        private readonly List<string> m_nodeHistory = new();
        private UnityAction m_exitDialogCode = null;
        private readonly List<IDialogResponder> m_responders = new();
        #endregion

        #region Serialized Fields
        [Header("Callbacks")]

        [Tooltip("Callback when dialog begins.")]
        [SerializeField]
        private UnityEvent m_initiateDialog = new();

        [Tooltip("Callback when dialog ends.")]
        [SerializeField]
        private UnityEvent m_exitDialog = new();

        [Tooltip("Callback when a new line of dialog begins.")]
        [SerializeField]
        private LineCallback m_initiateLine = new();

        [Header("Responders")]

        [Tooltip("Every DialogResponder that this DialogProcessor should check for keywords and function calls. Note that this field will accept any GameObject, but only objects that contain IDialogResponder will be used.")]
        [SerializeField]
        private GameObject[] m_dialogResponders = new GameObject[0];
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the singleton instance of the DialogProcessor.
        /// </summary>
        public static DialogProcessor Instance
        {
            get
            {
                if (s_instance == null) s_instance = FindAnyObjectByType<DialogProcessor>();
                return s_instance;
            }
        }

        /// <summary>
        /// Gets the current dialog agent associated with this dialog processor.
        /// </summary>
        public IDialogAgent DialogAgent { get; private set; } = null;
        
        /// <summary>
        /// Gets or sets the player's name for use in dialog.
        /// </summary>
        public static string PlayerName { get; set; } = "";

        /// <summary>
        /// Gets a value indicating whether the dialog processor is currently active.
        /// </summary>
        public static bool IsActive => Instance != null && Instance.m_isActive;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            Component[] components;

            // Add all responders to the m_responders list
            foreach(GameObject gameObject in m_dialogResponders)
            {
                components = gameObject.GetComponents<Component>();
                foreach(Component component in components)
                {
                    if (component is IDialogResponder dialogResponder)
                    {
                        m_responders.Add(dialogResponder);
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Exits the dialog and triggers the appropriate callbacks.
        /// </summary>
        public void KillDialog() => ExitDialog();

        /// <summary>
        /// Checks with the DialogProcessor and DialogAgent to see if the given word is a replaceable keyword. If so, returns the result.
        /// </summary>
        /// <param name="key">The word to check.</param>
        /// <param name="result">The result of the keyword.</param>
        /// <returns>True if the word is a replaceable keyword; otherwise, false.</returns>
        public bool GetKeyword(string key, out string result)
        {
            if (key.Equals("player") && !string.IsNullOrEmpty(PlayerName))
            {
                result = PlayerName;
                return true;
            }
            else if (key.Equals("self"))
            {
                result = DialogAgent.DisplayName;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Checks if the given function name is a valid function that can be called. If so, calls the function and returns the result.
        /// </summary>
        /// <param name="call">The name of the function to call.</param>
        /// <param name="parameter">The parameter to pass to the function.</param>
        /// <param name="result">The result of the function call.</param>
        /// <returns>True if the function is valid and was called; otherwise, false.</returns>
        public bool DialogFunction(string call, string parameter, out string result)
        {
            switch (call)
            {
                case "DisableDialog":
                    DialogAgent.enabled = false;
                    result = null;
                    return true;
            }

            result = null;
            return false;
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Initiates a dialog with the given DialogAgent and onExitDialog callback.
        /// </summary>
        /// <param name="dialogAgent">The DialogAgent to initiate the dialog with.</param>
        /// <param name="onExitDialog">The callback to invoke when the dialog exits.</param>
        internal static void InitiateDialog(IDialogAgent dialogAgent, UnityAction onExitDialog)
        {
            if (dialogAgent == null)
            {
                return;
            }

            if (Instance == null)
            {
                Debug.LogError($"{nameof(DialogProcessor)} could not be found.");
                return;
            }

            NodeData nodeData = null;
            Instance.m_nodeHistory.Clear();

            if (TryGetNode(dialogAgent.Data, dialogAgent.Data.start, out NodeData startNodeData))
            {
                nodeData = startNodeData;
            }
            else if (dialogAgent.Data.nodes.Length > 0)
            {
                nodeData = dialogAgent.Data.nodes[0];
            }

            if (nodeData != null)
            {
                AddDialogCount(dialogAgent.ID);
                Instance.DialogAgent = dialogAgent;
                Instance.m_nodeData = nodeData;
                Instance.m_lineIndex = 0;
                Instance.m_initiateDialog?.Invoke();
                Instance.OnInitiateDialog?.Invoke();
                Instance.m_isActive = true;
                Instance.RunLine();
            }

            Instance.m_exitDialogCode = onExitDialog;
        }

        /// <summary>
        /// Initiates a dialog with the given DialogAgent and no onExitDialog callback.
        /// </summary>
        /// <param name="dialogAgent">The DialogAgent to initiate the dialog with.</param>
        internal static void InitiateDialog(IDialogAgent dialogAgent) => InitiateDialog(dialogAgent, null);

        /// <summary>
        /// Determines the speaker type based on the given speaker string.
        /// </summary>
        /// <param name="speaker">The speaker string to check.</param>
        /// <returns>The speaker type based on the speaker string.</returns>
        internal static SpeakerType GetSpeakerType(string speaker)
        {
            if (speaker.Contains("player"))
            {
                return SpeakerType.Player;
            }
            else if (speaker.Contains("self"))
            {
                return SpeakerType.Self;
            }

            return SpeakerType.Other;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Replaces the inline text in the given string with the appropriate keyword or function result.
        /// </summary>
        /// <param name="text">The string containing inline text to be replaced.</param>
        /// <returns>The modified string with inline text replaced by keyword or function results.</returns>
        private string ReplaceInlineText(string text)
        {
            while (true)
            {
                int leftIndex = text.IndexOf("<<");
                int rightIndex = text.IndexOf(">>");

                if (leftIndex >= 0 && rightIndex > leftIndex)
                {
                    string beforeLeft = text[..leftIndex];
                    string interior = text.Substring(leftIndex + 2, rightIndex - leftIndex - 2);
                    string afterRight = rightIndex < text.Length - 2 ? text[(rightIndex + 2)..] : "";
                    if (GetKeyword(interior, out string dialogProcessorResult))
                    {
                        interior = dialogProcessorResult;
                    }
                    else if (DialogAgent.GetKeyword(interior, out string dialogAgentResult))
                    {
                        interior = dialogAgentResult;
                    }
                    else
                    {
                        for (int i = 0; i < m_responders.Count; i++)
                        {
                            if (m_responders[i].GetKeyword(interior, out string keywordTargetResult))
                            {
                                interior = keywordTargetResult;
                                break;
                            }
                        }
                    }
                    text = beforeLeft + interior + afterRight;
                }
                else
                {
                    break;
                }
            }

            return text;
        }

        /// <summary>
        /// Calls a function on the DialogProcessor, DialogAgent, and all responders.
        /// </summary>
        /// <param name="functionName">The name of the function to call.</param>
        private void CallFunction(string functionName)
        {
            if (string.IsNullOrEmpty(functionName)) return;

            string parameter = "";
            int indexOf = functionName.IndexOf("::");
            if (indexOf >= 0)
            {
                parameter = functionName[(indexOf + 2)..];
                functionName = functionName[..indexOf];
                if (!string.IsNullOrEmpty(parameter))
                {
                    parameter = ReplaceInlineText(parameter);
                }
            }

            DialogFunction(functionName, parameter, out _);
            DialogAgent.DialogFunction(functionName, parameter, out _);
            foreach (IDialogResponder dialogResponder in m_responders)
            {
                if (dialogResponder != DialogAgent)
                {
                    dialogResponder.DialogFunction(functionName, parameter, out _);
                }
            }
        }

        /// <summary>
        /// Advances the line to the next line if the current line is an if statement.
        /// </summary>
        /// <param name="link">The link to the next line.</param>
        private void AdvanceLine(string link)
        {
            if (m_nodeData == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(link) && TryGetNode(DialogAgent.Data, link, out NodeData linkedNodeDataFromResponse))
            {
                m_nodeData = linkedNodeDataFromResponse;
                m_lineIndex = 0;
            }
            else if (!string.IsNullOrEmpty(m_nodeData.lines[m_lineIndex].link) && TryGetNode(DialogAgent.Data, m_nodeData.lines[m_lineIndex].link, out NodeData linkedNodeDataFromEmbed))
            {
                m_nodeData = linkedNodeDataFromEmbed;
                m_lineIndex = 0;
            }
            else
            {
                m_lineIndex++;
            }

            RunLine();
        }

        /// <summary>
        /// Checks if the current line is an if statement that evaluates a variable.
        /// </summary>
        /// <param name="lineData">The line data containing the if statement and variable.</param>
        /// <param name="result">The result of the variable evaluation.</param>
        /// <returns>True if the line is an if statement that evaluates a variable; otherwise, false.</returns>
        private bool CheckVar(LineData lineData, out bool result)
        {
            result = false;

            if (!string.IsNullOrEmpty(lineData.ifVar))
            {
                string[] parts = lineData.ifVar.Split('=');
                if (parts.Length == 2 && DataStorage.TryGetValue(parts[0], out string value))
                {
                    result = parts[1].Equals(value);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the current line is an if statement that evaluates a function.
        /// </summary>
        /// <param name="lineData">The line data containing the if statement and function.</param>
        /// <param name="result">The result of the function evaluation.</param>
        /// <returns>True if the line is an if statement that evaluates a function; otherwise, false.</returns>
        private bool CheckFunction(LineData lineData, out bool result)
        {
            // Check if the ifFunction field is null or empty; if so, this line does not evaluate a function.
            if (string.IsNullOrEmpty(lineData.ifFunction))
            {
                result = false;
                return false;
            }

            int colonsIndex = lineData.ifFunction.IndexOf("::");
            int equalsIndex = lineData.ifFunction.IndexOf("=");

            string functionName = null;
            string parameter = null;
            string desiredResult = null;

            // Parse the ifFunction string to extract functionName, parameter, and desiredResult.
            if (colonsIndex >= 0 && equalsIndex > colonsIndex && equalsIndex < lineData.ifFunction.Length - 1)
            {
                functionName = lineData.ifFunction[..colonsIndex];
                parameter = lineData.ifFunction.Substring(colonsIndex + 2, equalsIndex - colonsIndex - 2);
                desiredResult = lineData.ifFunction[(equalsIndex + 1)..];
            }
            else if (colonsIndex >= 0 && equalsIndex == -1)
            {
                functionName = lineData.ifFunction[..colonsIndex];
                parameter = lineData.ifFunction[(colonsIndex + 2)..];
            }
            else if (colonsIndex == -1 && equalsIndex >= 0 && equalsIndex < lineData.ifFunction.Length - 1)
            {
                functionName = lineData.ifFunction[..equalsIndex];
                desiredResult = lineData.ifFunction[(equalsIndex + 1)..];
            }
            else if (colonsIndex == -1 && equalsIndex == -1)
            {
                functionName = lineData.ifFunction;
            }

            // Replace inline text in parameter and desiredResult, and normalize desiredResult to lowercase for comparison
            if (!string.IsNullOrEmpty(parameter))
            {
                parameter = ReplaceInlineText(parameter);
            }

            if (!string.IsNullOrEmpty(desiredResult))
            {
                desiredResult = ReplaceInlineText(desiredResult);
            }

            desiredResult = !string.IsNullOrEmpty(desiredResult) ? desiredResult.ToLower() : string.Empty;

            // Check if functionName is not null or empty, then attempt to evaluate the function on DialogProcessor, DialogAgent, and all responders.
            if (!string.IsNullOrEmpty(functionName))
            {
                if (DialogFunction(functionName, parameter, out string dialogProcessorResult))
                {
                    dialogProcessorResult = !string.IsNullOrEmpty(dialogProcessorResult) ? dialogProcessorResult.ToLower() : string.Empty;
                    result = desiredResult.Equals(dialogProcessorResult);
                    return true;
                }
                else if (DialogAgent.DialogFunction(functionName, parameter, out string dialogAgentResult))
                {
                    dialogAgentResult = !string.IsNullOrEmpty(dialogAgentResult) ? dialogAgentResult.ToLower() : string.Empty;
                    result = desiredResult.Equals(dialogAgentResult);
                    return true;
                }
                else
                {
                    foreach (IDialogResponder dialogResponder in m_responders)
                    {
                        if (dialogResponder.DialogFunction(functionName, parameter, out string dialogResponderResult))
                        {
                            dialogResponderResult = !string.IsNullOrEmpty(dialogResponderResult) ? dialogResponderResult.ToLower() : string.Empty;
                            result = desiredResult.Equals(dialogResponderResult);
                            return true;
                        }
                    }
                }
            }

            result = false;
            return true;
        }

        /// <summary>
        /// Checks if the current line is a query dialog.
        /// </summary>
        /// <param name="lineData">The line data containing the query data.</param>
        /// <returns>True if the line is a query dialog; otherwise, false.</returns>
        private bool CheckQuery(LineData lineData)
        {
            QueryData queryData = lineData.query;

            // Check if the queryData is valid, has non-empty text, and contains at least one response
            if (queryData != null && !string.IsNullOrEmpty(queryData.text) && queryData.responses != null && queryData.responses.Length > 0)
            {
                string[] responses = new string[queryData.responses.Length];

                // Loop through each response in queryData.responses and replace the inline text with the appropriate keyword or function result
                for (int i = 0; i < queryData.responses.Length; i++)
                {
                    responses[i] = ReplaceInlineText(queryData.responses[i].text);
                }

                // Initialize the query UI with the description and responses, and set the OnQueryResponse callback
                string description = !string.IsNullOrEmpty(queryData.description) ? queryData.description : string.Empty;
                Query.QueryUI.InitiateWithDescription(ReplaceInlineText(queryData.text), ReplaceInlineText(description), responses, OnQueryResponse);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles the response from a query dialog.
        /// </summary>
        /// <param name="response">The selected response text.</param>
        private void OnQueryResponse(string response)
        {
            QueryData queryData = m_nodeData.lines[m_lineIndex].query;
            foreach (ResponseData possibleResponse in queryData.responses.Where(possibleResponse => ReplaceInlineText(possibleResponse.text).Equals(response)))
            {
                AdvanceLine(possibleResponse.link);
                break;
            }
        }

        /// <summary>
        /// Checks if the dialog count meets the specified condition.
        /// </summary>
        /// <param name="lineData">The line data containing the dialog count condition.</param>
        /// <param name="result">The result of the dialog count check.</param>
        /// <returns>True if the dialog count condition is met; otherwise, false.</returns>
        private bool CheckDialogCount(LineData lineData, out bool result)
        {
            result = false;

            if (string.IsNullOrEmpty(lineData.ifDialogCount) || lineData.ifDialogCount.Length <= 1) return false;

            string leftSide = lineData.ifDialogCount[..1];
            string rightSide = lineData.ifDialogCount[1..];

            if (int.TryParse(rightSide, out int intValue))
            {
                int count = GetDialogCount(DialogAgent.ID);
                switch (leftSide)
                {
                    case "<":
                        result = count < intValue;
                        return true;
                    case ">":
                        result = count > intValue;
                        return true;
                    case "=":
                        result = count == intValue;
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Advances to the next line if the current line is an if statement.
        /// </summary>
        private void PassOverIf()
        {
            m_lineIndex++;

            if (m_lineIndex >= m_nodeData.lines.Length)
            {
                RunLine();
            }
            else if (m_nodeData.lines[m_lineIndex].elseIf || m_nodeData.lines[m_lineIndex].endIf)
            {
                AdvanceLine(null);
            }
            else
            {
                PassOverIf();
            }
        }

        /// <summary>
        /// Exits the dialog and triggers the appropriate callbacks.
        /// </summary>
        private void ExitDialog()
        {
            DialogAgent = null;
            m_nodeData = null;
            m_lineIndex = 0;
            if (gameObject.activeSelf)
            {
                StartCoroutine(DelayedExit());
            }
            DialogUI.Instance.Close();
            m_exitDialog?.Invoke();
            OnExitDialog?.Invoke();
            m_exitDialogCode?.Invoke();
        }

        /// <summary>
        /// Delays the exit of the dialog by 0.15 seconds to ensure the dialog UI is closed.
        /// </summary>
        /// <returns>An IEnumerator that waits for 0.15 seconds before setting the dialog inactive.</returns>
        private IEnumerator DelayedExit()
        {
            yield return new WaitForSecondsRealtime(0.15f);
            m_isActive = false;
        }

        /// <summary>
        /// Sets a variable in the DataStorage based on the provided VarData.
        /// </summary>
        /// <param name="varData">The variable data containing key and value.</param>
        private void SetVar(VarData varData)
        {
            if (!string.IsNullOrEmpty(varData.key))
            {
                DataStorage.SetValue(varData.key, varData.value);
            }
        }

        /// <summary>
        /// Processes the current line of dialog, handling conditions, queries, and other logic.
        /// </summary>
        private void RunLine()
        {
            // Check if the current line index is out of bounds; if so, exit the dialog.
            if (m_lineIndex < 0 || m_lineIndex >= m_nodeData.lines.Length)
            {
                ExitDialog();
                return;
            }

            // Set the variable in the DataStorage based on the setVar field of the current line.
            SetVar(m_nodeData.lines[m_lineIndex].setVar);

            // Call the function on the current line.
            CallFunction(m_nodeData.lines[m_lineIndex].callFunction);

            // Check if the current line is an if statement that evaluates a variable.
            if (CheckVar(m_nodeData.lines[m_lineIndex], out bool varResult))
            {
                if (varResult)
                {
                    AdvanceLine(null);
                }
                else
                {
                    PassOverIf();
                }

                return;
            }
            // Check if the current line is an if statement that evaluates a function.
            else if (CheckFunction(m_nodeData.lines[m_lineIndex], out bool funcResult))
            {
                if (funcResult)
                {
                    AdvanceLine(null);
                }
                else
                {
                    PassOverIf();
                }

                return;
            }
            // Check if the current line is a query dialog.
            else if (CheckQuery(m_nodeData.lines[m_lineIndex]))
            {

            }
            // Check if the dialog count meets the specified condition.
            else if (CheckDialogCount(m_nodeData.lines[m_lineIndex], out bool dialogResult))
            {
                if (dialogResult)
                {
                    AdvanceLine(null);
                }
                else
                {
                    PassOverIf();
                }

                return;
            }
            // Check if the current line is an else if statement.
            else if (m_nodeData.lines[m_lineIndex].elseIf)
            {
                PassOverIf();
                return;
            }
            // Check if the current line is an end if statement.
            else if (m_nodeData.lines[m_lineIndex].endIf)
            {
                AdvanceLine(null);
                return;
            }
            // Check if the current line is empty.
            else if (string.IsNullOrEmpty(m_nodeData.lines[m_lineIndex].speaker) && string.IsNullOrEmpty(m_nodeData.lines[m_lineIndex].text))
            {
                AdvanceLine(null);
                return;
            }
            // Check if the current line is an exit statement.
            else if (m_nodeData.lines[m_lineIndex].exit)
            {
                ExitDialog();
                return;
            }
            // If the current line is not an if statement, query dialog, dialog count, else if, end if, empty, exit, or set var, add the node name to the node history and call the DialogUI.Instance.Initiate method to initiate the line.
            else
            {
                m_nodeHistory.Add(m_nodeData.name);
                DialogUI.Instance.Initiate(m_nodeData.lines[m_lineIndex], AdvanceLine, ReplaceInlineText);
                m_initiateLine?.Invoke(m_nodeData.lines[m_lineIndex], AdvanceLine, ReplaceInlineText);
            }
        }

        /// <summary>
        /// Increments the dialog count for a given ID and stores it in the DataStorage.
        /// </summary>
        /// <param name="id">The unique identifier for the dialog.</param>
        private static void AddDialogCount(string id)
        {
            if (DataStorage.TryGetValue(id, out string value))
            {
                if (int.TryParse(value, out int result))
                {
                    DataStorage.SetValue(id, (++result).ToString());
                }
            }
            else
            {
                DataStorage.Add(id, "1");
            }
        }

        /// <summary>
        /// Retrieves the number of times a dialog with the specified ID has been initiated.
        /// </summary>
        /// <param name="id">The unique identifier for the dialog.</param>
        /// <returns>The count of dialog initiations for the given ID, or 0 if none exist.</returns>
        private static int GetDialogCount(string id)
        {
            if (DataStorage.TryGetValue(id, out string value) && int.TryParse(value, out int result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Attempts to find a node with the specified name in the given DialogData.
        /// </summary>
        /// <param name="dialogData">The dialog data containing nodes.</param>
        /// <param name="name">The name of the node to find.</param>
        /// <param name="nodeData">The found node, if any.</param>
        /// <returns>True if a node with the specified name is found; otherwise, false.</returns>
        private static bool TryGetNode(DialogData dialogData, string name, out NodeData nodeData)
        {
            nodeData = null;
            foreach (NodeData nd in dialogData.nodes.Where(nd => nd.name.Equals(name)))
            {
                nodeData = nd;
                return true;
            }

            return false;
        }
        #endregion
    }
}