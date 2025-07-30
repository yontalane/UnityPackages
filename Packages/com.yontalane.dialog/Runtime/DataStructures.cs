//
// Summary:
// This file defines core data structures and enumerations used by the Yontalane Dialog system.
// It includes types for keyword replacement, dialog agent input modes, speaker types, portrait events,
// color sets for speakers, and inline image replacement information for rich text dialog rendering.
//

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.Dialog
{
    /// <summary>
    /// Represents a key-value pair used for keyword replacement in dialog scripts.
    /// </summary>
    [Serializable]
    internal class KeywordPair
    {
        /// <summary>
        /// The keyword to be replaced in the dialog script.
        /// </summary>
        [Tooltip("The keyword to be replaced in the dialog script.")]
        public string key = "";

        /// <summary>
        /// The value to replace the keyword with in the dialog script.
        /// </summary>
        [Tooltip("The value to replace the keyword with in the dialog script.")]
        public string value = "";
    }

    /// <summary>
    /// Specifies the type of input used to provide dialog data to a DialogAgent.
    /// </summary>
    public enum DialogAgentInputType
    {
        Data = 0,
        Json = 1,
        String = 2,
        TextData = 40,
    }

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

    /// <summary>
    /// Represents an event containing information about a dialog portrait, including the agent, line data, image, and speaker.
    /// </summary>
    public struct PortraitEvent
    {
        [Tooltip("The dialog agent associated with this portrait event.")]
        public IDialogAgent agent;

        [Tooltip("The line data associated with this portrait event.")]
        public LineData data;

        [Tooltip("The UI Image component displaying the portrait.")]
        public Image image;

        [Tooltip("The name of the speaker for this portrait event.")]
        public string speaker;
    }

    /// <summary>
    /// Represents a set of color values for a specific speaker in the dialog.
    /// </summary>
    [Serializable]
    internal struct ColorSet
    {
        [Tooltip("The name of the speaker for this color set.")]
        public string speaker;

        [Tooltip("The color value to use for this speaker.")]
        public Color color;
    }

    [Serializable]
    /// <summary>
    /// Contains information for replacing a specific text substring with an inline image (sprite) in dialog, including the replacement text, the sprite to use, and the scale of the image.
    /// </summary>
    public struct InlineImageReplacementInfo
    {
        /// <summary>
        /// The substring in the dialog text that should be replaced with an inline image.
        /// </summary>
        [Tooltip("The substring in the dialog text to be replaced by the inline image.")]
        public string textToReplace;

        /// <summary>
        /// The sprite to use as the inline image replacement.
        /// </summary>
        [Tooltip("The sprite that will replace the specified text in the dialog.")]
        public Sprite sprite;

        /// <summary>
        /// The scale factor to apply to the inline image.
        /// </summary>
        [Tooltip("The scale factor for the inline image relative to its default size.")]
        [Min(0.0001f)]
        public float scale;
    }

    /// <summary>
    /// Data for replacing text with an inline image.
    /// </summary>
    internal struct InlineImageReplacementPostProcessingInfo
    {
        /// <summary>
        /// The text string that is being replaced.
        /// </summary>
        public string text;

        /// <summary>
        /// The index of the first character of the string within the dialog.
        /// </summary>
        public int startIndex;

        /// <summary>
        /// The index of the last character of the string within the dialog.
        /// </summary>
        public int endIndex;

        /// <summary>
        /// The UI element containing the text.
        /// </summary>
        public TMP_Text textField;

        /// <summary>
        /// The image component for the inline image.
        /// </summary>
        public Image image;
    }
}