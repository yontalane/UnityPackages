using System;
using UnityEngine;

namespace Yontalane.Dialog
{
    [Serializable]
    public class DialogData
    {
        public string start = "";
        public NodeData[] nodes = new NodeData[0];
        public string windowType = "";
        public string data = "";

        public DialogData()
        {

        }

        public DialogData(DialogData other)
        {
            start = other.start;
            nodes = other.nodes;
            windowType = other.windowType;
            data = other.data;
        }
    }

    [Serializable]
    public class NodeData
    {
        public string name = "";
        public LineData[] lines = new LineData[0];
        public string data = "";
    }

    [Serializable]
    public class LineData
    {
        public string speaker = "";
        [TextArea]
        public string text = "";
        public string portrait = "";
        public string typing = "";
        public string sound = "";
        public string voice = "";
        public string link = "";
        public ResponseData[] responses = new ResponseData[0];
        public string data = "";

        public string ifDialogCount = "";
        public string ifFunction = "";
        public string ifVar = "";
        public bool elseIf = false;
        public bool endIf = false;
        public bool exit = false;

        public VarData setVar = new VarData();
        public QueryData query = new QueryData();
        public string callFunction = "";
    }

    [Serializable]
    public class VarData
    {
        public string key = "";
        public string value = "";
    }

    [Serializable]
    public class ResponseData
    {
        public string text = "";
        public string link = "";
    }

    [Serializable]
    public class QueryData
    {
        public string text = "";
        public string description = "";
        public ResponseData[] responses = new ResponseData[0];
    }
}