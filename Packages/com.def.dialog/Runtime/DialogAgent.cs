using System;
using UnityEngine;

namespace DEF.Dialog
{
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

        [SerializeField] private TextAsset m_json = null;
        [SerializeField] private string m_staticText = "";
        [SerializeField] private KeywordPair[] m_keywords = new KeywordPair[0];

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

        public void Initialize(string id, string json)
        {
            ID = id;
            Data = JsonUtility.FromJson<DialogData>(json);
        }

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

        public void InitializeStatic(string text)
        {
            InitializeStatic("", text);
        }

        public override bool DialogFunction(string call, string parameter, out string result)
        {
            result = null;
            return false;
        }

        public override bool GetKeyword(string key, out string result)
        {
            foreach (KeywordPair keywordPair in m_keywords)
                if (keywordPair.key.Equals(key))
                {
                    result = keywordPair.value;
                    return true;
                }
            result = null;
            return false;
        }
    }
}