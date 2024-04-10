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
        private static readonly List<TextDataConversion> s_conversions = new();

        private static string s_startNode = string.Empty;
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

            string[] lines = asset.text.Split('\n');

            GetNodeData(lines);

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
                    s_lineData.Add(new()
                    {
                        speaker = line[..colonIndex].Trim(),
                        text = line[(colonIndex + 1)..].Trim(),
                    });
                    s_responseData.Clear();
                }
                else if (hyphenIndex == 0 && arrowIndex != -1)
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
            int i = line.IndexOf(',');

            if (i == -1)
            {
                return;
            }

            string[] s;

            if (i < line.Length - 1)
            {
                s = new string[] { line[..i], line[(i + 1)..] };
            }
            else
            {
                s = new string[] { line[..i] };
            }

            string functionName = s[0].Trim();
            string parameter = s.Length > 1 ? s[1].Trim() : string.Empty;

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
    }
}
