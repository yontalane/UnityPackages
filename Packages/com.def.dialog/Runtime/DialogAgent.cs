using System;
using System.Linq;
using UnityEngine;

namespace DEF.Dialog
{
    [AddComponentMenu("DEF/Dialog/Dialog Agent")]
    [DisallowMultipleComponent]
    public class DialogAgent : DialogResponder
    {
        private const string STATIC_ID = "Static Dialog";

        [Serializable]
        private class KeywordPair
        {
            public string key = "";
            public string value = "";
        }

        public string ID { get; private set; } = "";
        public DialogData Data { get; private set; } = null;

        [Header("Dialog Script Data")]

        [SerializeField]
        [Tooltip("A complex dialog script. Used to generate DialogData.")]
        private TextAsset m_json = null;

        [SerializeField]
        [Tooltip("A single, unchanging line of dialog. Used in place of a complex script for simple things like signs.")]
        private string m_staticText = "";

        [Header("Keyword Replacement")]

        [SerializeField]
        [Tooltip("Text to replace keywords in dialog.")]
        private KeywordPair[] m_keywords = new KeywordPair[0];

        /// <summary>
        /// If the <c>m_json</c> field has been assigned, use that to generate DialogData. Otherwise, use <c>m_staticText</c>.
        /// </summary>
        private void Awake()
        {
            if (Data == null || (string.IsNullOrEmpty(Data.start) && string.IsNullOrEmpty(Data.windowType) && string.IsNullOrEmpty(Data.data) && Data.nodes.Length == 0))
            {
                if (m_json != null)
                    Initialize(name, m_json.text);
                else
                    InitializeStatic(m_staticText);
            }
        }

        /// <summary>
        /// Generate DialogData using JSON.
        /// </summary>
        public void Initialize(string id, string json)
        {
            ID = id;
            Data = JsonUtility.FromJson<DialogData>(json);
        }

        /// <summary>
        /// Generate DialogData using a non-JSON string.
        /// </summary>
        public void InitializeStatic(string speaker, string text)
        {
            ID = STATIC_ID;
            Data = new DialogData();
            Data.nodes = new NodeData[1];
            Data.nodes[0] = new NodeData();
            Data.nodes[0].lines = new LineData[1];
            Data.nodes[0].lines[0] = new LineData();
            Data.nodes[0].lines[0].speaker = speaker;
            Data.nodes[0].lines[0].text = text;
        }

        /// <summary>
        /// Generate DialogData using a non-JSON string.
        /// </summary>
        public void InitializeStatic(string text) => InitializeStatic("", text);

        /// <summary>
        /// Use <c>m_keywords</c> to replace a keyword in dialog script.
        /// </summary>
        public override bool GetKeyword(string key, out string result)
        {
            foreach (KeywordPair keywordPair in m_keywords.Where(keywordPair => keywordPair.key.Equals(key)))
            {
                result = keywordPair.value;
                return true;
            }

            result = null;
            return false;
        }
    }
}