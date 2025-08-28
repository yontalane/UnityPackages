using System;
using UnityEngine;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Represents the root data structure for a dialog, including its starting node, nodes, window type, and additional data.
    /// </summary>
    [Serializable]
    public class DialogData
    {
        /// <summary>
        /// The name of the node where the dialog starts.
        /// </summary>
        [Tooltip("The name of the node where the dialog starts.")]
        public string start = "";

        /// <summary>
        /// The array of all nodes that make up this dialog.
        /// </summary>
        [Tooltip("The array of all nodes that make up this dialog.")]
        public NodeData[] nodes = new NodeData[0];

        /// <summary>
        /// The type of window to use for displaying this dialog.
        /// </summary>
        [Tooltip("The type of window to use for displaying this dialog.")]
        public string windowType = "";

        /// <summary>
        /// Additional custom data for this dialog, stored as a string.
        /// </summary>
        [Tooltip("Additional custom data for this dialog, stored as a string.")]
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

    /// <summary>
    /// Represents a single node within a dialog, containing its name, lines, and any additional custom data.
    /// </summary>
    [Serializable]
    public class NodeData
    {
        /// <summary>
        /// The unique name of this node within the dialog.
        /// </summary>
        [Tooltip("The unique name of this node within the dialog.")]
        public string name = "";

        /// <summary>
        /// The array of lines that make up the content of this node.
        /// </summary>
        [Tooltip("The array of lines that make up the content of this node.")]
        public LineData[] lines = new LineData[0];

        /// <summary>
        /// Additional custom data for this node, stored as a string.
        /// </summary>
        [Tooltip("Additional custom data for this node, stored as a string.")]
        public string data = "";
    }

    /// <summary>
    /// Represents a single line of dialog, including speaker, text, responses, and related metadata.
    /// </summary>
    [Serializable]
    public class LineData
    {
        /// <summary>
        /// The name of the character or entity speaking this line.
        /// </summary>
        [Tooltip("The name of the character or entity speaking this line.")]
        public string speaker = "";

        /// <summary>
        /// The dialog text to be displayed for this line.
        /// </summary>
        [Tooltip("The dialog text to be displayed for this line.")]
        [TextArea]
        public string text = "";

        /// <summary>
        /// The name or identifier of the portrait to display for this line.
        /// </summary>
        [Tooltip("The name or identifier of the portrait to display for this line.")]
        public string portrait = "";

        /// <summary>
        /// The name or identifier of the typing sound effect to use for this line.
        /// </summary>
        [Tooltip("The name or identifier of the typing sound effect to use for this line.")]
        public string typing = "";

        /// <summary>
        /// The name or identifier of the typing sound effect to loop while writing this line.
        /// </summary>
        [Tooltip("The name or identifier of the typing sound effect to loop while writing this line.")]
        public string typingLoop = "";

        /// <summary>
        /// The name or identifier of the sound effect to play for this line.
        /// </summary>
        [Tooltip("The name or identifier of the sound effect to play for this line.")]
        public string sound = "";

        /// <summary>
        /// The name or identifier of the voice clip to play for this line.
        /// </summary>
        [Tooltip("The name or identifier of the voice clip to play for this line.")]
        public string voice = "";

        /// <summary>
        /// The link or identifier to the next dialog node or line.
        /// </summary>
        [Tooltip("The link or identifier to the next dialog node or line.")]
        public string link = "";

        /// <summary>
        /// The array of possible responses the player can select after this line.
        /// </summary>
        [Tooltip("The array of possible responses the player can select after this line.")]
        public ResponseData[] responses = new ResponseData[0];

        /// <summary>
        /// Additional custom data for this line, stored as a string.
        /// </summary>
        [Tooltip("Additional custom data for this line, stored as a string.")]
        public string data = "";

        /// <summary>
        /// The required dialog count for this line to be shown. Leave empty to ignore.
        /// </summary>
        [Tooltip("The required dialog count for this line to be shown. Leave empty to ignore.")]
        public string ifDialogCount = "";

        /// <summary>
        /// Assigns a new value to the dialog count.
        /// </summary>
        [Tooltip("Assigns a new value to the dialog count.")]
        public string setDialogCount = "";

        /// <summary>
        /// Adds a value to the dialog count.
        /// </summary>
        [Tooltip("Adds a value to the dialog count.")]
        public string addDialogCount = "";

        /// <summary>
        /// The name of a function that must return true for this line to be shown. Leave empty to ignore.
        /// </summary>
        [Tooltip("The name of a function that must return true for this line to be shown. Leave empty to ignore.")]
        public string ifFunction = "";

        /// <summary>
        /// The name of a variable that must be true or non-empty for this line to be shown. Leave empty to ignore.
        /// </summary>
        [Tooltip("The name of a variable that must be true or non-empty for this line to be shown. Leave empty to ignore.")]
        public string ifVar = "";

        /// <summary>
        /// If true, this line acts as an 'else if' in a conditional dialog branch.
        /// </summary>
        [Tooltip("If true, this line acts as an 'else if' in a conditional dialog branch.")]
        public bool elseIf = false;

        /// <summary>
        /// If true, this line marks the end of an 'if' or 'else if' conditional block.
        /// </summary>
        [Tooltip("If true, this line marks the end of an 'if' or 'else if' conditional block.")]
        public bool endIf = false;

        /// <summary>
        /// If true, the dialog will exit after this line.
        /// </summary>
        [Tooltip("If true, the dialog will exit after this line.")]
        public bool exit = false;

        /// <summary>
        /// Variable assignment to perform when this line is reached.
        /// </summary>
        [Tooltip("Variable assignment to perform when this line is reached.")]
        public VarData setVar = new();

        /// <summary>
        /// Query to present to the user at this line, if any.
        /// </summary>
        [Tooltip("Query to present to the user at this line, if any.")]
        public QueryData query = new();

        /// <summary>
        /// The name of a function to call when this line is reached.
        /// </summary>
        [Tooltip("The name of a function to call when this line is reached.")]
        public string callFunction = "";

        /// <summary>
        /// The name of a function to call when this line is reached. The function builds the following line, including speaker, text, and responses.
        /// </summary>
        [Tooltip("The name of a function to call when this line is reached. The function builds the following line, including speaker, text, and responses.")]
        public string lineBuilderFunction = "";
    }

    /// <summary>
    /// Represents a key-value pair for storing a variable in the dialog system.
    /// </summary>
    [Serializable]
    public class VarData
    {
        /// <summary>
        /// The key (name) of the variable to store or modify in the dialog system.
        /// </summary>
        [Tooltip("The key (name) of the variable to store or modify in the dialog system.")]
        public string key = "";

        /// <summary>
        /// The value to assign to the variable specified by the key.
        /// </summary>
        [Tooltip("The value to assign to the variable specified by the key.")]
        public string value = "";
    }

    /// <summary>
    /// Represents a possible response option in a dialog, including the response text and the link to the next dialog line.
    /// </summary>
    [Serializable]
    public class ResponseData
    {
        /// <summary>
        /// The text displayed for this response option.
        /// </summary>
        [Tooltip("The text displayed for this response option.")]
        public string text = "";

        /// <summary>
        /// The link or identifier to the next dialog node if this response is selected.
        /// </summary>
        [Tooltip("The link or identifier to the next dialog node if this response is selected.")]
        public string link = "";
    }

    /// <summary>
    /// Represents a dialog query, including the prompt text, description, and possible response options.
    /// </summary>
    [Serializable]
    public class QueryData
    {
        /// <summary>
        /// The main prompt or question text presented to the user in this query.
        /// </summary>
        [Tooltip("The main prompt or question text presented to the user in this query.")]
        public string text = "";

        /// <summary>
        /// Additional description or context for the query, providing more information to the user.
        /// </summary>
        [Tooltip("Additional description or context for the query.")]
        public string description = "";

        /// <summary>
        /// The array of possible response options the user can select in response to this query.
        /// </summary>
        [Tooltip("The array of possible response options for this query.")]
        public ResponseData[] responses = new ResponseData[0];
    }
}