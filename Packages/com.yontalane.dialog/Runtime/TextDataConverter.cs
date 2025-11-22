using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yontalane.Dialog
{
    // Summary:
    // The TextDataConversion struct is an internal data structure used to associate a TextAsset
    // (typically a text file in Unity) with its corresponding DialogData after conversion.
    // This allows for efficient caching and retrieval of dialog data that has already been processed,
    // preventing redundant conversions of the same asset.
    internal struct TextDataConversion
    {
        /// <summary>
        /// The TextAsset associated with this conversion. Represents the source text file containing dialog data.
        /// </summary>
        public TextAsset asset;

        /// <summary>
        /// The DialogData generated from the associated TextAsset after conversion.
        /// </summary>
        public DialogData data;
    }

    /// <summary>
    /// Provides functionality to convert Unity TextAsset files containing dialog scripts into DialogData objects.
    /// Handles parsing, caching, and processing of dialog nodes, lines, and responses for efficient reuse.
    /// </summary>
    internal static class TextDataConverter
    {
        private const string LINE_JOINER = "--";

        private static readonly List<TextDataConversion> s_conversions = new();

        private static string s_startNode = string.Empty;
        private static readonly List<string> s_lines = new();
        private static readonly List<NodeData> s_nodeData = new();
        private static readonly List<LineData> s_lineData = new();
        private static readonly List<ResponseData> s_responseData = new();

        internal static DialogData Convert(TextAsset asset, string start)
        {
            foreach (TextDataConversion textDataConversion in s_conversions.Where(textDataConversion => textDataConversion.asset == asset))
            {
                return new(textDataConversion.data)
                {
                    start = start
                };
            }

            s_startNode = start;

            s_nodeData.Clear();
            s_lineData.Clear();
            s_responseData.Clear();

            s_lines.Clear();
            s_lines.AddRange(asset.text.Split('\n'));

            FixForLineJoiners(s_lines);

            GetNodeData(s_lines);

            DialogData data = new()
            {
                nodes = s_nodeData.ToArray(),
                start = s_startNode,
            };

            s_conversions.Add(new()
            {
                asset = asset,
                data = data,
            });

            return data;
        }

        /// <summary>
        /// Merges lines in the provided list that begin with the line joiner marker ("--") into the previous line.
        /// This is used to handle cases where a logical line of text is split across multiple lines in the source.
        /// </summary>
        /// <param name="lines">The list of lines to process and merge as needed.</param>
        private static void FixForLineJoiners(List<string> lines)
        {
            if (lines == null || lines.Count < 2)
            {
                return;
            }

            string line;
            int length = LINE_JOINER.Length;

            for (int i = lines.Count - 1; i >= 1; i--)
            {
                line = lines[i].TrimStart();

                if (line.IndexOf(LINE_JOINER) == 0)
                {
                    lines[i - 1] += line[length..];
                    lines.RemoveAt(i);
                }
            }
        }


        private static void GetNodeData(IReadOnlyList<string> lines)
        {
            bool startedNodes = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                string lineUp = line.ToUpper();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (line.IndexOf('/') == 0)
                {
                    continue;
                }

                if (!startedNodes && line.IndexOf("==>") == 0)
                {
                    if (string.IsNullOrEmpty(s_startNode))
                    {
                        s_startNode = FormatLink(line[2..]);
                    }
                    continue;
                }
                else if (lineUp.IndexOf("=>") == 0)
                {
                    LineToLink(line[2..]);
                    continue;
                }
                else if (lineUp.IndexOf("COUNT") == 0)
                {
                    LineToCount(line[5..]);
                    continue;
                }
                else if (lineUp.IndexOf("SET COUNT") == 0)
                {
                    LineToSetCount(line[9..]);
                    continue;
                }
                else if (lineUp.IndexOf("SETCOUNT") == 0)
                {
                    LineToSetCount(line[8..]);
                    continue;
                }
                else if (lineUp.IndexOf("ADD COUNT") == 0)
                {
                    LineToAddCount(line[9..]);
                    continue;
                }
                else if (lineUp.IndexOf("ADDCOUNT") == 0)
                {
                    LineToAddCount(line[8..]);
                    continue;
                }
                else if (lineUp.IndexOf("SET:") == 0)
                {
                    LineToSet(line[4..]);
                    continue;
                }
                else if (lineUp.IndexOf("IF FUNCTION:") == 0)
                {
                    LineToIfFunction(line["IF FUNCTION:".Length..]);
                    continue;
                }
                else if (lineUp.IndexOf("IF FUNC:") == 0)
                {
                    LineToIfFunction(line["IF FUNC:".Length..]);
                    continue;
                }
                else if (lineUp.IndexOf("IFF:") == 0)
                {
                    LineToIfFunction(line["IFF:".Length..]);
                    continue;
                }
                else if (lineUp.IndexOf("IF:") == 0)
                {
                    LineToIf(line[3..]);
                    continue;
                }
                else if (lineUp.IndexOf("DO:") == 0)
                {
                    LineToDo(line[3..]);
                    continue;
                }
                else if (lineUp.IndexOf("!:") == 0)
                {
                    LineToDo(line[2..]);
                    continue;
                }
                else if (lineUp.IndexOf("LINE:") == 0)
                {
                    LineToBuild(line[5..]);
                    continue;
                }
                else if (lineUp.IndexOf("?:") == 0)
                {
                    LineToQuery(line["?:".Length..]);
                    continue;
                }
                else if (lineUp.IndexOf("?") == 0)
                {
                    LineToQuery(line["?".Length..]);
                    continue;
                }
                else if (lineUp == "EXIT")
                {
                    s_lineData.Add(new() { exit = true, });
                    s_responseData.Clear();
                    continue;
                }

                int poundIndex = line.IndexOf('#');
                int colonIndex = line.IndexOf(':');
                int hyphenIndex = line.IndexOf('-');
                int arrowIndex = line.IndexOf("=>");

                if (poundIndex == 0)
                {
                    startedNodes = true;
                    if (s_nodeData.Count > 0 && s_lineData.Count > 0)
                    {
                        s_nodeData[^1].lines = s_lineData.ToArray();
                    }
                    s_lineData.Clear();
                    s_nodeData.Add(new() { name = line[1..].Trim() });
                }
                else if (colonIndex != -1)
                {
                    (string speaker, string portrait, string sound, string voice, string typing, string typingLoop) = GetSpeakerAndDisplayData(line[..colonIndex]);

                    // If the text is empty, make it be a single space.
                    string text = ProcessLineBreaks(line[(colonIndex + 1)..].Trim());
                    if (string.IsNullOrEmpty(text))
                    {
                        text = " ";
                    }

                    s_lineData.Add(new()
                    {
                        speaker = speaker,
                        portrait = portrait,
                        sound = sound,
                        voice = voice,
                        typing = typing,
                        typingLoop = typingLoop,
                        text = text,
                    });
                    s_responseData.Clear();
                }
                else if (hyphenIndex == 0 && arrowIndex != -1 && s_lineData != null && s_lineData.Count > 0)
                {
                    string responseText = line[1..arrowIndex].Trim();
                    string responseLink = FormatLink(line[(arrowIndex + 2)..]);

                    s_responseData.Add(new()
                    {
                        text = responseText,
                        link = responseLink,
                    });
                    s_lineData[^1].responses = s_responseData.ToArray();
                }
            }

            if (s_nodeData.Count > 0 && s_lineData.Count > 0)
            {
                s_nodeData[^1].lines = s_lineData.ToArray();
            }
        }

        /// <summary>
        /// Parse the speaker and display data text.
        /// </summary>
        /// <param name="text">The snippet of dialog script that we're parsing. This should be the beginning of a line of script, up to, but not including, the colon.</param>
        /// <returns>The speaker name, and, optionally: portrait asset name, sound asset name, voice clip asset name, typing sound effect asset name, and typing loop sound effect asset name.</returns>
        private static (string speaker, string portrait, string sound, string voice, string typing, string typingLoop) GetSpeakerAndDisplayData(string text)
        {
            // Trim white space from the beginning and end of the line.
            text = text.Trim();

            // If the text is empty, return empty values.
            if (string.IsNullOrEmpty(text))
            {
                return default;
            }

            // Get the location of the square brackets containing display meta data.
            int openBracketIndex = text.LastIndexOf("[");
            int closeBracketIndex = text.LastIndexOf("]");

            // If the brackets don't exist, then there's no display data. Return the speaker name and nothing else.
            if (openBracketIndex == -1 || closeBracketIndex == -1 || openBracketIndex > closeBracketIndex)
            {
                return (text, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            }
            // If the brackets do exist, then return the speaker name plus the display data.
            else
            {
                // Split the text into speaker and display data.
                text = text[..closeBracketIndex];
                string displayData = text[(openBracketIndex + 1)..].Trim();
                string speaker = text[..openBracketIndex].Trim();

                // If the brackets are empty ("[]"), then that's a shortcut to use the speaker name as the portrait.
                if (string.IsNullOrEmpty(displayData))
                {
                    return (speaker, speaker, string.Empty, string.Empty, string.Empty, string.Empty);
                }

                // Return the speaker and display data.
                (string portrait, string sound, string voice, string typing, string typingLoop) = GetDisplayData(displayData);
                return (speaker, portrait, sound, voice, typing, typingLoop);
            }
        }

        /// <summary>
        /// Parse the display data text.
        /// </summary>
        /// <param name="text">The snippet of dialog script that we're parsing. This should be the text between the square brackets, before the colon, at the beginning of a single line of dialog script.</param>
        /// <returns>The portrait asset name, sound asset name, voice clip asset name, typing sound effect asset name, and typing loop sound effect asset name.</returns>
        private static (string portrait, string sound, string voice, string typing, string typingLoop) GetDisplayData(string text)
        {
            // Trim white space from the beginning and end of the line.
            text = text.Trim();

            // If the text is empty, return empty values.
            if (string.IsNullOrEmpty(text))
            {
                return default;
            }

            // Parse the text into multiple sections on a comma delimiter.
            string[] sections = text.Split(',');

            // If there's only one section (no commas) AND if there's no equal sign (this isn't a key/value pair),
            // then return the text as the portrait name and all other display data as empty strings.
            if (sections.Length == 1 && !sections[0].Contains('='))
            {
                return (sections[0].Trim(), string.Empty, string.Empty, string.Empty, string.Empty);
            }

            // Parse each piece of display data and return it.
            string portrait = ParseDisplayData(sections, "Portrait");
            string sound = ParseDisplayData(sections, "Sound");
            string voice = ParseDisplayData(sections, "Voice");
            string typing = ParseDisplayData(sections, "Typing");
            string typingLoop = ParseDisplayData(sections, "Typing Loop", "TypingLoop");
            return (portrait, sound, voice, typing, typingLoop);
        }

        /// <summary>
        /// Search through a list of key/value pair strings and find the value for the given key.
        /// </summary>
        /// <param name="keys">The keys we're searching for.</param>
        /// <param name="texts">A list of key/value pairs.</param>
        /// <returns>The value for the key. If none is found, then an empty string.</returns>
        private static string ParseDisplayData(IReadOnlyList<string> texts, params string[] keys)
        {
            // Key is case-insensitive, so make it lowercase;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = keys[i].ToLower();
            }

            // Scan through each text item in the provided list.
            foreach (string text in texts)
            {
                // Split the item into a key/value pair using the equals sign as the delimiter.
                string[] parts = text.Split('=');

                // If there aren't two parts, then it's not a key/value pair, so continue to the next item.
                if (parts.Length != 2)
                {
                    continue;
                }

                // If we've found the key is the key we're searching for, then return the value.
                foreach (string key in keys)
                {
                    if (parts[0].Trim().ToLower() == key)
                    {
                        return parts[1].Trim();
                    }
                }
            }

            // If we never found the value, then return an empty string.
            return string.Empty;
        }

        /// <summary>
        /// Converts a line to a setVar function.
        /// </summary>
        /// <param name="line">The line of text to parse.</param>
        private static void LineToSet(string line)
        {
            string[] parts = line.Split('=');

            if (parts.Length != 2)
            {
                return;
            }

            VarData varData = new()
            {
                key = parts[0].Trim(),
                value = parts[1].Trim(),
            };

            s_lineData.Add(new() { setVar = varData, });
            s_responseData.Clear();
        }

        /// <summary>
        /// Parses a line to extract and set the dialog count value.
        /// Adds a setDialogCount marker to s_lineData and clears any existing response data.
        /// </summary>
        /// <param name="line">The line of text to parse for setting dialog count.</param>
        private static void LineToSetCount(string line)
        {
            line = line.Replace("=", "");
            line = line.Replace(":", "");
            line = line.Trim();

            s_lineData.Add(new() { setDialogCount = line, });
            s_responseData.Clear();
        }

        /// <summary>
        /// Parses a line to extract and add to the dialog count value.
        /// Adds a addDialogCount marker to s_lineData and clears any existing response data.
        /// </summary>
        /// <param name="line">The line of text to parse for adding to dialog count.</param>
        private static void LineToAddCount(string line)
        {
            line = line.Replace("=", "");
            line = line.Replace(":", "");
            line = line.Trim();

            s_lineData.Add(new() { addDialogCount = line, });
            s_responseData.Clear();
        }

        /// <summary>
        /// Converts a line to a Link function.
        /// Adds a link marker to s_lineData and clears any existing response data.
        /// </summary>
        /// <param name="line">The line of text to parse.</param>
        private static void LineToLink(string line)
        {
            line = line.Trim();

            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            s_lineData.Add(new() { link = FormatLink(line), });
            s_responseData.Clear();
        }

        /// <summary>
        /// Converts a line to a ifDialogCount function.
        /// Adds an ifDialogCount, a link, and an endIf marker to s_lineData, and clears any existing response data.
        /// </summary>
        /// <param name="line">The line of text to parse.</param>
        private static void LineToCount(string line)
        {
            string[] setting = line.Split("=>");

            if (setting.Length != 2)
            {
                return;
            }

            setting[0] = setting[0].Replace(" ", "");
            s_lineData.Add(new() { ifDialogCount = setting[0], });
            s_lineData.Add(new() { link = FormatLink(setting[1]), });
            s_lineData.Add(new() { endIf = true, });
            s_responseData.Clear();
        }

        /// <summary>
        /// Converts a line to an ifVar function.
        /// Adds an ifVar, a link, and an endIf marker to s_lineData, and clears any existing response data.
        /// </summary>
        /// <param name="line">The line of text to parse.</param>
        private static void LineToIf(string line)
        {
            // Split the input line into two parts using "=>" to separate the condition from the link.
            string[] setting = line.Split("=>");

            // If the split does not result in exactly two parts, exit the function.
            if (setting.Length != 2)
            {
                return;
            }

            // Further split the condition part using "=" to separate the variable and its expected value.
            string[] checking = setting[0].Split('=');

            // If the split does not result in exactly two parts, exit the function.
            if (checking.Length != 2)
            {
                return;
            }

            // Add an ifVar entry, a link entry, an endIf marker to s_lineData, and clear any existing response data.
            s_lineData.Add(new() { ifVar = $"{checking[0].Trim()}={checking[1].Trim()}", });
            s_lineData.Add(new() { link = FormatLink(setting[1]), });
            s_lineData.Add(new() { endIf = true, });
            s_responseData.Clear();
        }

        /// <summary>
        /// Parses a line representing a query with multiple response options, extracting the query text,
        /// optional description, and associated responses and links, and adds the resulting query data to s_lineData.
        /// </summary>
        /// <param name="line">The line of text to parse as a query.</param>
        private static void LineToQuery(string line)
        {
            // Split the input line into pieces using "&&" as the delimiter to separate the query and its responses.
            string[] linePieces = line.Split("&&");
            if (linePieces.Length < 2)
            {
                return;
            }

            // Extract the main query text and optional description (if present, separated by "||").
            string queryText = linePieces[0].Trim();
            string descriptionText = string.Empty;

            if (queryText.Contains("||"))
            {
                string[] texts = queryText.Split("||");
                queryText = texts[0].Trim();
                descriptionText = texts[1].Trim();
            }

            // Gather all response options from the remaining line pieces.
            string[] queryResponses = new string[linePieces.Length - 1];
            for (int i = 1; i < linePieces.Length; i++)
            {
                queryResponses[i - 1] = linePieces[i];
            }

            // Prepare arrays to hold the response texts and their corresponding links.
            string[] queryResponseTexts = new string[queryResponses.Length];
            string[] queryResponseLinks = new string[queryResponses.Length];

            // For each response, split into text and link using "=>".
            for (int i = 0; i < queryResponses.Length; i++)
            {
                string[] pieces = queryResponses[i].Split("=>");

                if (pieces.Length != 2)
                {
                    return;
                }

                queryResponseTexts[i] = pieces[0].Trim();
                queryResponseLinks[i] = pieces[1].Trim();
            }

            // Create a new LineData object to hold the query and its responses.
            LineData lineData = new()
            {
                query = new()
                {
                    text = queryText,
                    description = descriptionText,
                    responses = new ResponseData[queryResponses.Length],
                }
            };

            // Populate the responses array with the extracted texts and links.
            for (int i = 0; i < queryResponses.Length; i++)
            {
                lineData.query.responses[i] = new()
                {
                    text = queryResponseTexts[i],
                    link = queryResponseLinks[i],
                };
            }

            // Add the constructed LineData to the list and clear any existing response data.
            s_lineData.Add(lineData);
            s_responseData.Clear();
        }

        /// <summary>
        /// Converts a line to an ifFunction function.
        /// Adds an ifFunction, a link, and an endIf marker to s_lineData, and clears any existing response data.
        /// </summary>
        /// <param name="line">The line of text to parse.</param>
        private static void LineToIfFunction(string line)
        {
            // Split the line into the condition and the link using "=>"
            string[] setting = line.Split("=>");
            if (setting.Length != 2)
            {
                return;
            }

            // Split the condition part into the function/parameter and the value using "="
            int index = setting[0].LastIndexOf('=');
            if (index <= 0 || index >= setting[0].Length - 1)
            {
                return;
            }
            string[] checking = new string[2];
            checking[0] = setting[0][..index];
            checking[1] = setting[0][(index + 1)..];

            // Further split the function/parameter part by "," to separate function and parameter
            string func, param;
            string[] init = checking[0].Split(',');

            // Determine the function and parameter from the split; if there is more than one part, assign both, otherwise assign only the function and leave parameter empty.
            if (init.Length > 1)
            {
                func = init[0].Trim();
                param = init[1].Trim();
            }
            else
            {
                func = init[0].Trim();
                param = string.Empty;
            }

            // Add the ifFunction, link, and endIf markers to s_lineData, and clear response data
            s_lineData.Add(new() { ifFunction = $"{func}::{param}={checking[1].Trim()}", });
            s_lineData.Add(new() { link = FormatLink(setting[1]), });
            s_lineData.Add(new() { endIf = true, });
            s_responseData.Clear();
        }

        /// <summary>
        /// Converts a line to a callFunction.
        /// Adds an callFunction marker to s_lineData, and clears any existing response data.
        /// </summary>
        /// <param name="line">The line of text to parse.</param>
        private static void LineToDo(string line)
        {
            // Parse the line to extract the function name and parameter (if any), then add a callFunction marker to s_lineData and clear s_responseData.
            string functionName, parameter;

            // Find the first comma in the line to separate function name and parameter (if any)
            int i = line.IndexOf(',');

            // If there is no comma, the entire line is the function name, and parameter is empty
            if (i == -1)
            {
                functionName = line.Trim();
                parameter = string.Empty;
            }
            else
            {
                // If there is a comma, split into function name and parameter
                string[] s;

                // If the comma is not the last character, split into two parts
                if (i < line.Length - 1)
                {
                    s = new string[] { line[..i], line[(i + 1)..] };
                }
                else
                {
                    // If the comma is the last character, only function name is present
                    s = new string[] { line[..i] };
                }

                functionName = s[0].Trim();
                parameter = s.Length > 1 ? s[1].Trim() : string.Empty;
            }

            // Add the callFunction marker to s_lineData and clear any existing response data
            s_lineData.Add(new() { callFunction = $"{functionName}::{parameter}", });
            s_responseData.Clear();
        }

        /// <summary>
        /// Converts a line to a lineBuilderFunction marker.
        /// Adds a lineBuilderFunction marker to s_lineData and clears any existing response data.
        /// </summary>
        /// <param name="line">The line of text to parse as a line builder function.</param>
        private static void LineToBuild(string line)
        {
            s_lineData.Add(new() { lineBuilderFunction = line.Trim(), });
            s_responseData.Clear();
        }

        /// <summary>
        /// Formats a link string by trimming whitespace and removing a leading '#' character if present.
        /// This is used to standardize node or link references within dialog scripts.
        /// </summary>
        /// <param name="link">The link string to format.</param>
        /// <returns>The formatted link string without leading '#' and trimmed of whitespace.</returns>
        private static string FormatLink(string link)
        {
            link = link.Trim();
            if (link[..1] == "#")
            {
                link = link[1..];
            }
            return link.TrimStart();
        }

        /// <summary>
        /// Replaces all occurrences of the line break marker ("|:") in the input text with actual newline characters.
        /// This allows for custom line breaks within dialog scripts.
        /// </summary>
        /// <param name="text">The input string potentially containing line break markers.</param>
        /// <returns>The processed string with line break markers replaced by newline characters.</returns>
        private static string ProcessLineBreaks(string text)
        {
            return text.Replace("|:", "\n");
        }
    }
}
