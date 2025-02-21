using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Dialog
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Dialog/Dialog Processor")]
    public sealed class DialogProcessor : MonoBehaviour, IDialogResponder
    {
        public delegate void InitiateDialogDelegate();
        public InitiateDialogDelegate OnInitiateDialog = null;

        public delegate void ExitDialogDelegate();
        public ExitDialogDelegate OnExitDialog = null;

        [Serializable]
        class LineCallback : UnityEvent<LineData, Action<string>, Func<string, string>> { }

        public enum SpeakerType
        {
            Player,
            Self,
            Other
        }

        private bool m_isActive = false;
        private NodeData m_nodeData = null;
        private int m_lineIndex = 0;

        public IDialogAgent DialogAgent { get; private set; } = null;
        public static string PlayerName { get; set; } = "";

        [Header("Callbacks")]

        [SerializeField]
        [Tooltip("Callback when dialog begins.")]
        private UnityEvent m_initiateDialog = new();

        [SerializeField]
        [Tooltip("Callback when dialog ends.")]
        private UnityEvent m_exitDialog = new();
        private UnityAction m_exitDialogCode = null;

        [SerializeField]
        [Tooltip("Callback when a new line of dialog begins.")]
        private LineCallback m_initiateLine = new();

        [Header("Responders")]

        [SerializeField]
        [Tooltip("Every DialogResponder that this DialogProcessor should check for keywords and function calls. Note that this field will accept any GameObject, but only objects that contain IDialogResponder will be used.")]
        private GameObject[] m_dialogResponders = new GameObject[0];
        private readonly List<IDialogResponder> m_responders = new();

        private static DialogProcessor s_instance = null;
        public static DialogProcessor Instance
        {
            get
            {
                if (s_instance == null) s_instance = FindAnyObjectByType<DialogProcessor>();
                return s_instance;
            }
        }

        public static bool IsActive => Instance != null && Instance.m_isActive;

        private void Start()
        {
            Component[] components;
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

        internal static void InitiateDialog(IDialogAgent dialogAgent, UnityAction onExitDialog)
        {
            if (dialogAgent == null) return;

            if (Instance == null)
            {
                Debug.LogError("DialogProcessor could not be found.");
                return;
            }

            NodeData nodeData = null;

            if (GetNode(dialogAgent.Data, dialogAgent.Data.start, out NodeData startNodeData))
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

        internal static void InitiateDialog(IDialogAgent dialogAgent) => InitiateDialog(dialogAgent, null);

        private static void AddDialogCount(string id)
        {
            if (DataStorage.Vars.TryGetValue(id, out string value))
            {
                if (int.TryParse(value, out int result))
                {
                    DataStorage.Vars[id] = (++result).ToString();
                }
            }
            else
            {
                DataStorage.Vars.Add(id, "1");
            }
        }

        public void KillDialog() => ExitDialog();

        private static int GetDialogCount(string id)
        {
            if (DataStorage.Vars.TryGetValue(id, out string value) && int.TryParse(value, out int result))
            {
                return result;
            }

            return 0;
        }

        private static bool GetNode(DialogData dialogData, string name, out NodeData nodeData)
        {
            nodeData = null;
            foreach (NodeData nd in dialogData.nodes.Where(nd => nd.name.Equals(name)))
            {
                nodeData = nd;
                return true;
            }

            return false;
        }

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

        private void AdvanceLine(string link)
        {
            if (m_nodeData == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(link) && GetNode(DialogAgent.Data, link, out NodeData linkedNodeDataFromResponse))
            {
                m_nodeData = linkedNodeDataFromResponse;
                m_lineIndex = 0;
            }
            else if (!string.IsNullOrEmpty(m_nodeData.lines[m_lineIndex].link) && GetNode(DialogAgent.Data, m_nodeData.lines[m_lineIndex].link, out NodeData linkedNodeDataFromEmbed))
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

        private bool CheckVar(LineData lineData, out bool result)
        {
            result = false;

            if (!string.IsNullOrEmpty(lineData.ifVar))
            {
                string[] parts = lineData.ifVar.Split('=');
                if (parts.Length == 2 && DataStorage.Vars.TryGetValue(parts[0], out string value))
                {
                    result = parts[1].Equals(value);
                }
                return true;
            }

            return false;
        }

        private bool CheckFunction(LineData lineData, out bool result)
        {
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

            if (!string.IsNullOrEmpty(parameter))
            {
                parameter = ReplaceInlineText(parameter);
            }

            if (!string.IsNullOrEmpty(desiredResult))
            {
                desiredResult = ReplaceInlineText(desiredResult);
            }

            desiredResult = !string.IsNullOrEmpty(desiredResult) ? desiredResult.ToLower() : string.Empty;

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

        private bool CheckQuery(LineData lineData)
        {
            QueryData queryData = lineData.query;

            if (queryData != null && !string.IsNullOrEmpty(queryData.text) && queryData.responses.Length > 0)
            {
                string[] responses = new string[queryData.responses.Length];
                for (int i = 0; i < queryData.responses.Length; i++)
                {
                    responses[i] = ReplaceInlineText(queryData.responses[i].text);
                }
                string description = !string.IsNullOrEmpty(queryData.description) ? queryData.description : string.Empty;
                Query.QueryUI.Initiate(ReplaceInlineText(queryData.text), description, responses, OnQueryResponse);
                return true;
            }

            return false;
        }

        private void OnQueryResponse(string response)
        {
            QueryData queryData = m_nodeData.lines[m_lineIndex].query;
            foreach (ResponseData possibleResponse in queryData.responses.Where(possibleResponse => ReplaceInlineText(possibleResponse.text).Equals(response)))
            {
                AdvanceLine(possibleResponse.link);
                break;
            }
        }

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

        private IEnumerator DelayedExit()
        {
            yield return new WaitForSecondsRealtime(0.15f);
            m_isActive = false;
        }

        private void SetVar(VarData varData)
        {
            if (!string.IsNullOrEmpty(varData.key))
            {
                if (DataStorage.Vars.ContainsKey(varData.key))
                {
                    DataStorage.Vars[varData.key] = varData.value;
                }
                else
                {
                    DataStorage.Vars.Add(varData.key, varData.value);
                }
            }
        }

        private void RunLine()
        {
            if (m_lineIndex < 0 || m_lineIndex >= m_nodeData.lines.Length)
            {
                ExitDialog();
                return;
            }

            SetVar(m_nodeData.lines[m_lineIndex].setVar);
            CallFunction(m_nodeData.lines[m_lineIndex].callFunction);

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
            else if (CheckFunction(m_nodeData.lines[m_lineIndex], out bool funcResult))
            {
                if (funcResult)
                {
                    AdvanceLine(null);
                }
                else
                    PassOverIf();
                return;
            }
            else if (CheckQuery(m_nodeData.lines[m_lineIndex]))
            {

            }
            else if (CheckDialogCount(m_nodeData.lines[m_lineIndex], out bool dialogResult))
            {
                if (dialogResult)
                {
                    AdvanceLine(null);
                }
                else
                    PassOverIf();
                return;
            }
            else if (m_nodeData.lines[m_lineIndex].elseIf)
            {
                PassOverIf();
                return;
            }
            else if (m_nodeData.lines[m_lineIndex].endIf)
            {
                AdvanceLine(null);
                return;
            }
            else if (string.IsNullOrEmpty(m_nodeData.lines[m_lineIndex].speaker) && string.IsNullOrEmpty(m_nodeData.lines[m_lineIndex].text))
            {
                AdvanceLine(null);
                return;
            }
            else if (m_nodeData.lines[m_lineIndex].exit)
            {
                ExitDialog();
                return;
            }
            else
            {
                DialogUI.Instance.Initiate(m_nodeData.lines[m_lineIndex], AdvanceLine, ReplaceInlineText);
                m_initiateLine?.Invoke(m_nodeData.lines[m_lineIndex], AdvanceLine, ReplaceInlineText);
            }
        }
    }
}