using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Dialog
{
    [Serializable]
    internal class KeywordPair
    {
        public string key = "";
        public string value = "";
    }

    public enum DialogAgentInputType
    {
        Data = 0,
        Json = 1,
        String = 2
    }

    [AddComponentMenu("Yontalane/Dialog/Dialog Agent")]
    [DisallowMultipleComponent]
    public class DialogAgent : MonoBehaviour, IDialogAgent
    {
        internal const string STATIC_ID = "Static Dialog";

        public string ID { get; private set; } = "";
        public DialogData Data { get; set; } = null;

        [Header("Dialog Script Data")]

        [SerializeField]
        [Tooltip("The agent's name in dialog. If blank, will default to the name of this asset.")]
        private string m_displayName = "";

        [SerializeField]
        [Tooltip("The type of dialog script this object expects.")]
        private DialogAgentInputType m_inputType = DialogAgentInputType.Json;

        [SerializeField]
        [Tooltip("A complex dialog script. Used to generate DialogData.")]
        private DialogDataContainer m_data = null;

        [SerializeField]
        [Tooltip("A complex dialog script. Used to generate DialogData.")]
        private TextAsset m_json = null;

        [SerializeField]
        [TextArea]
        [Tooltip("A single, unchanging line of dialog. Used in place of a complex script for simple things like signs.")]
        private string m_staticText = "";

        [Header("Keyword Replacement")]

        [SerializeField]
        [Tooltip("Text to replace keywords in dialog.")]
        private KeywordPair[] m_keywords = new KeywordPair[0];

        private UnityAction m_onExitDialog;

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

        public void InitiateDialog(string speaker, UnityAction onExitDialog)
        {
            if (Data == null || (string.IsNullOrEmpty(Data.start) && string.IsNullOrEmpty(Data.windowType) && string.IsNullOrEmpty(Data.data) && Data.nodes.Length == 0))
            {
                switch(m_inputType)
                {
                    case DialogAgentInputType.Data:
                        ID = name;
                        Data = m_data.data;
                        break;
                    case DialogAgentInputType.Json:
                        ID = name;
                        Data = JsonUtility.FromJson<DialogData>(m_json.text);
                        break;
                    case DialogAgentInputType.String:
                        ID = STATIC_ID;
                        Data = new DialogData();
                        Data.nodes = new NodeData[1];
                        Data.nodes[0] = new NodeData();
                        Data.nodes[0].lines = new LineData[1];
                        Data.nodes[0].lines[0] = new LineData();
                        Data.nodes[0].lines[0].speaker = speaker;
                        Data.nodes[0].lines[0].text = m_staticText;
                        break;
                }
            }

            m_onExitDialog = onExitDialog;

            DialogProcessor.InitiateDialog(this, OnExitDialog);
        }

        public void InitiateDialog(string speaker) => InitiateDialog(speaker, null);

        public void InitiateDialog(UnityAction onExitDialog) => InitiateDialog(string.Empty, onExitDialog);

        public void InitiateDialog() => InitiateDialog(string.Empty, null);

        private void OnExitDialog() => m_onExitDialog?.Invoke();

        /// <summary>
        /// Use <c>m_keywords</c> to replace a keyword in dialog script.
        /// </summary>
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

        public virtual bool DialogFunction(string call, string parameter, out string result)
        {
            result = null;
            return false;
        }
    }
}
