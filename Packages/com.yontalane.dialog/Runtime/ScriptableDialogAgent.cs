using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Dialog
{
    [CreateAssetMenu(fileName = "Dialog Agent", menuName = "Yontalane/Dialog/Dialog Agent", order = 1)]
    public class ScriptableDialogAgent : ScriptableObject, IDialogAgent
    {
        public string ID { get; private set; } = "";
        public DialogData Data { get; private set; } = null;

        [Header("Dialog Script Data")]

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

        private bool m_enabled = true;
        public bool enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }

        public void InitiateDialog(string speaker, UnityAction onExitDialog)
        {
            if (Data == null || (string.IsNullOrEmpty(Data.start) && string.IsNullOrEmpty(Data.windowType) && string.IsNullOrEmpty(Data.data) && Data.nodes.Length == 0))
            {
                if (m_json != null)
                {
                    ID = name;
                    Data = JsonUtility.FromJson<DialogData>(m_json.text);
                }
                else
                {
                    ID = DialogAgent.STATIC_ID;
                    Data = new DialogData();
                    Data.nodes = new NodeData[1];
                    Data.nodes[0] = new NodeData();
                    Data.nodes[0].lines = new LineData[1];
                    Data.nodes[0].lines[0] = new LineData();
                    Data.nodes[0].lines[0].speaker = speaker;
                    Data.nodes[0].lines[0].text = m_staticText;
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
