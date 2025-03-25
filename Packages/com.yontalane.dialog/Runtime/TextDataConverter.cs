using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yontalane.Dialog
{
    internal struct TextDataConversion
    {
        public TextAsset asset;
        public DialogData data;
    }

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

                if (!startedNodes && line.IndexOf("=>") == 0)
                {
                    if (string.IsNullOrEmpty(s_startNode))
                    {
                        s_startNode = FormatLink(line[2..]);
                    }
                    continue;
                }
                else if (lineUp.IndexOf("COUNT") == 0)
                {
                    LineToCount(line[5..]);
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
                    (string speaker, string portrait) = GetSpeakerAndPortrait(line[..colonIndex]);
                    s_lineData.Add(new()
                    {
                        speaker = speaker,
                        portrait = portrait,
                        text = ProcessLineBreaks(line[(colonIndex + 1)..].Trim()),
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

        private static (string speaker, string portrait) GetSpeakerAndPortrait(string text)
        {
            text = text.Trim();

            int openBracketIndex = text.LastIndexOf("[");
            int closeBracketIndex = text.LastIndexOf("]");

            if (openBracketIndex == -1 || closeBracketIndex == -1 || openBracketIndex > closeBracketIndex)
            {
                return (text, string.Empty);
            }
            else
            {
                text = text[..closeBracketIndex];
                string portrait = text[(openBracketIndex + 1)..].Trim();
                string speaker = text[..openBracketIndex].Trim();
                if (string.IsNullOrEmpty(portrait))
                {
                    portrait = speaker;
                }
                return (speaker, portrait);
            }
        }

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

        private static void LineToIf(string line)
        {
            string[] setting = line.Split("=>");

            if (setting.Length != 2)
            {
                return;
            }

            string[] checking = setting[0].Split('=');

            if (checking.Length != 2)
            {
                return;
            }

            s_lineData.Add(new() { ifVar = $"{checking[0].Trim()}={checking[1].Trim()}", });
            s_lineData.Add(new() { link = FormatLink(setting[1]), });
            s_lineData.Add(new() { endIf = true, });
            s_responseData.Clear();
        }

        private static void LineToQuery(string line)
        {
            string[] linePieces = line.Split("&&");
            if (linePieces.Length < 2)
            {
                return;
            }

            string queryText = linePieces[0].Trim();
            string descriptionText = string.Empty;

            if (queryText.Contains("||"))
            {
                string[] texts = queryText.Split("||");
                queryText = texts[0].Trim();
                descriptionText = texts[1].Trim();
            }

            string[] queryResponses = new string[linePieces.Length - 1];
            for (int i = 1; i < linePieces.Length; i++)
            {
                queryResponses[i - 1] = linePieces[i];
            }

            string[] queryResponseTexts = new string[queryResponses.Length];
            string[] queryResponseLinks = new string[queryResponses.Length];

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

            LineData lineData = new()
            {
                query = new()
                {
                    text = queryText,
                    description = descriptionText,
                    responses = new ResponseData[queryResponses.Length],
                }
            };

            for (int i = 0; i < queryResponses.Length; i++)
            {
                lineData.query.responses[i] = new()
                {
                    text = queryResponseTexts[i],
                    link = queryResponseLinks[i],
                };
            }

            s_lineData.Add(lineData);
            s_responseData.Clear();
        }

        private static void LineToIfFunction(string line)
        {
            string[] setting = line.Split("=>");
            if (setting.Length != 2)
            {
                return;
            }

            string[] checking = setting[0].Split('=');
            if (checking.Length != 2)
            {
                return;
            }

            string func, param;
            string[] init = checking[0].Split(',');
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

            s_lineData.Add(new() { ifFunction = $"{func}::{param}={checking[1].Trim()}", });
            s_lineData.Add(new() { link = FormatLink(setting[1]), });
            s_lineData.Add(new() { endIf = true, });
            s_responseData.Clear();
        }

        private static void LineToDo(string line)
        {
            string functionName, parameter;

            int i = line.IndexOf(',');

            if (i == -1)
            {
                functionName = line.Trim();
                parameter = string.Empty;
            }
            else
            {
                string[] s;

                if (i < line.Length - 1)
                {
                    s = new string[] { line[..i], line[(i + 1)..] };
                }
                else
                {
                    s = new string[] { line[..i] };
                }

                functionName = s[0].Trim();
                parameter = s.Length > 1 ? s[1].Trim() : string.Empty;
            }

            s_lineData.Add(new() { callFunction = $"{functionName}::{parameter}", });
            s_responseData.Clear();
        }

        private static string FormatLink(string link)
        {
            link = link.Trim();
            if (link[..1] == "#")
            {
                link = link[1..];
            }
            return link.TrimStart();
        }

        private static string ProcessLineBreaks(string text)
        {
            return text.Replace("|:", "\n");
        }
    }
}
