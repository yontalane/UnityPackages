using System.Collections.Generic;

namespace Yontalane.Dialog
{
    internal static class TextDataConverter
    {
        private static string m_startNode = string.Empty;
        private static readonly List<NodeData> m_nodeData = new();
        private static readonly List<LineData> m_lineData = new();
        private static readonly List<ResponseData> m_responseData = new();

        internal static DialogData Convert(string data, string start)
        {
            string[] lines = data.Split('\n');
            GetNodeData(lines);
            return new() { nodes = m_nodeData.ToArray(), start = m_startNode };
        }


        private static void GetNodeData(IReadOnlyList<string> lines)
        {
            bool startedNodes = false;

            m_startNode = string.Empty;
            m_nodeData.Clear();
            m_lineData.Clear();
            m_responseData.Clear();

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
                    if (string.IsNullOrEmpty(m_startNode))
                    {
                        m_startNode = FormatLink(line[2..]);
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
                    m_lineData.Add(new() { exit = true, });
                    m_responseData.Clear();
                    continue;
                }

                int poundIndex = line.IndexOf('#');
                int colonIndex = line.IndexOf(':');
                int hyphenIndex = line.IndexOf('-');
                int arrowIndex = line.IndexOf("=>");

                if (poundIndex == 0)
                {
                    startedNodes = true;
                    if (m_nodeData.Count > 0 && m_lineData.Count > 0)
                    {
                        m_nodeData[^1].lines = m_lineData.ToArray();
                    }
                    m_lineData.Clear();
                    m_nodeData.Add(new() { name = line[1..].Trim() });
                }
                else if (colonIndex != -1)
                {
                    m_lineData.Add(new()
                    {
                        speaker = line[..colonIndex].Trim(),
                        text = line[(colonIndex + 1)..].Trim(),
                    });
                    m_responseData.Clear();
                }
                else if (hyphenIndex == 0 && arrowIndex != -1)
                {
                    string responseText = line[1..arrowIndex].Trim();
                    string responseLink = FormatLink(line[(arrowIndex + 2)..]);

                    m_responseData.Add(new()
                    {
                        text = responseText,
                        link = responseLink,
                    });
                    m_lineData[^1].responses = m_responseData.ToArray();
                }
            }

            if (m_nodeData.Count > 0 && m_lineData.Count > 0)
            {
                m_nodeData[^1].lines = m_lineData.ToArray();
            }
        }

        private static void LineToSet(string line)
        {
            string[] parts = line.Split(':');

            if (parts.Length != 2)
            {
                return;
            }

            VarData varData = new()
            {
                key = parts[0].Trim(),
                value = parts[1].Trim(),
            };

            m_lineData.Add(new() { setVar = varData, });
            m_responseData.Clear();
        }

        private static void LineToCount(string line)
        {
            string[] setting = line.Split("=>");

            if (setting.Length != 2)
            {
                return;
            }

            setting[0] = setting[0].Replace(" ", "");
            m_lineData.Add(new() { ifDialogCount = setting[0], });
            m_lineData.Add(new() { link = FormatLink(setting[1]), });
            m_lineData.Add(new() { endIf = true, });
            m_responseData.Clear();
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

            m_lineData.Add(new() { ifVar = $"{checking[0].Trim()}={checking[1].Trim()}", });
            m_lineData.Add(new() { link = FormatLink(setting[1]), });
            m_lineData.Add(new() { endIf = true, });
            m_responseData.Clear();
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

            m_lineData.Add(new() { ifFunction = $"{func}::{param}={checking[1].Trim()}", });
            m_lineData.Add(new() { link = FormatLink(setting[1]), });
            m_lineData.Add(new() { endIf = true, });
            m_responseData.Clear();
        }

        private static void LineToDo(string line)
        {
            string[] s = line.Split(":");

            if (s.Length < 1)
            {
                return;
            }

            string functionName = s[0].Trim();
            string parameter = s.Length > 1 ? s[1].Trim() : string.Empty;

            m_lineData.Add(new() { callFunction = $"{functionName}::{parameter}", });
            m_responseData.Clear();
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
