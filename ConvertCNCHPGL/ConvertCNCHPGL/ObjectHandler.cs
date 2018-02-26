using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConvertCNCHPGL
{
    public enum Commands
    {
        NONE, LINE, ARC, SUBPROG, SUBEND, SUBCALL, ABSOLUTE, RELATIVE, TOOL, ON, OFF
    }

    class ObjectHandler
    {
        private Module_XML _Xml;
        public Module_XML Xml
        {
            get
            {
                return _Xml;
            }
            set
            {
                _Xml = value;
            }
        }

        private SortedDictionary<Commands, string[]> _CommandList;
        public SortedDictionary<Commands, string[]> CommandList
        {
            get
            {
                return _CommandList;
            }
            private set
            {
                _CommandList = value;
            }
        }

        private string _ModularCommand;
        public string ModularCommand
        {
            get
            {
                return _ModularCommand;
            }
            set
            {
                _ModularCommand = value;
            }
        }

        private string _ModularArcCommand;
        public string ModularArcCommand
        {
            get
            {
                return _ModularArcCommand;
            }
            set
            {
                _ModularArcCommand = value;
            }
        }

        private int _LastSubIndex;
        public int LastSubIndex
        {
            get
            {
                return _LastSubIndex;
            }
            set
            {
                _LastSubIndex = value;
            }
        }

        private bool _InSub;
        public bool InSub
        {
            get
            {
                return _InSub;
            }
            set
            {
                _InSub = value;
            }
        }

        private bool _SubStarted;
        public bool SubStarted
        {
            get
            {
                return _SubStarted;
            }
            set
            {
                if (value == true)
                    SubsStarted++;

                _SubStarted = value;
            }
        }

        private int _SubsStarted;
        public int SubsStarted
        {
            get
            {
                return _SubsStarted;
            }
            set
            {
                _SubsStarted = value;
            }
        }

        private string[] _Lines;
        public string[] Lines
        {
            get
            {
                return _Lines;
            }
            private set
            {
                _Lines = value;
            }
        }

        public ObjectHandler(string[] lines)
        {
            Xml = new Module_XML();
            CommandList = GetCommandList();
            Lines = lines;
            LastSubIndex = GetLastSubIndex();
            DefineDoubleCommands();
        }

        private SortedDictionary<Commands, string[]> GetCommandList()
        {
            SortedDictionary<Commands, string[]> result = new SortedDictionary<Commands, string[]>();

            try
            {
                XmlNode node = Xml.ReturnXml(@".\Settings.xml", "CNCCmds");

                foreach (XmlNode item in node)
                {
                    string[] args = item.InnerText.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                    switch (item.Name)
                    {
                        case "Relative":
                            result.Add(Commands.RELATIVE, args);
                            break;
                        case "Arc":
                            result.Add(Commands.ARC, args);
                            break;
                        case "Line":
                            result.Add(Commands.LINE, args);
                            break;
                        case "On":
                            result.Add(Commands.ON, args);
                            break;
                        case "Tool":
                            result.Add(Commands.TOOL, args);
                            break;
                        case "CallSub":
                            result.Add(Commands.SUBCALL, args);
                            break;
                        case "StartSub":
                            result.Add(Commands.SUBPROG, args);
                            break;
                        case "EndSub":
                            result.Add(Commands.SUBEND, args);
                            break;
                        case "Absolute":
                            result.Add(Commands.ABSOLUTE, args);
                            break;
                        case "Off":
                            result.Add(Commands.OFF, args);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public CommandRef DetectMain(string line)
        {
            CommandRef result = new CommandRef(Commands.NONE, "", "");

            try
            {
                string[] cmds;
                CommandList.TryGetValue(Commands.SUBPROG, out cmds);

                for (int i = 0; i < cmds.Length; i++)
                {
                    if (line.Contains(cmds[i]))
                        result = new CommandRef(Commands.SUBPROG, cmds[i], line);
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public CommandRef DetectCommand(string line)
        {
            CommandRef result = new CommandRef(Commands.NONE, "", "");
            List<CommandRef> results = new List<CommandRef>();

            try
            {
                for (int k = 0; k < CommandList.Count; k++)
                {
                    for (int v = 0; v < CommandList.ElementAt(k).Value.Length; v++)
                    {
                        if (line.Contains(CommandList.ElementAt(k).Value[v]))
                        {
                            results.Add(new CommandRef(CommandList.ElementAt(k).Key, CommandList.ElementAt(k).Value[v], line));
                        }
                    }
                }

                int highestLength = 0;

                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].Reference.Length > highestLength)
                    {
                        highestLength = results[i].Reference.Length;
                        result = results[i];
                    }
                    else if (results[i].Reference.Length == highestLength)
                    {
                        if (results[i].Line.IndexOf(results[i].Reference) < result.Line.IndexOf(result.Reference))
                            result = results[i];
                    }
                }

                if (Statements.IsNull(result.Command))
                    result = HasCoordinates(line);
                
                if (result.Command == Commands.LINE || result.Command == Commands.ARC)
                    ModularCommand = result.Reference;
                if (result.Command == Commands.ARC)
                    ModularArcCommand = result.Reference;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        private CommandRef HasCoordinates(string line)
        {
            CommandRef result = new CommandRef();

            try
            {
                if (ModularCommand == "" || ModularCommand == string.Empty)
                    return result;

                Commands cmd = GetCommandFromReference(ModularCommand);

                if (line.Contains('X') || line.Contains('Y') || line.Contains('Z'))
                {
                    if (line.Contains('I') || line.Contains('J'))
                    {
                        if (!Statements.IsNull(ModularArcCommand))
                        {
                            line = ModularArcCommand + line.Substring(line.IndexOf('X'));
                            result = new CommandRef(Commands.ARC, ModularArcCommand, line);
                        }
                        else
                        {
                            line = "G03" + line.Substring(line.IndexOf('X'));
                            result = new CommandRef(Commands.ARC, "G03", line);
                        }
                    }
                    else
                    {
                        if (cmd == Commands.LINE)
                        {
                            if (line.Contains('X'))
                                line = ModularCommand + line.Substring(line.IndexOf('X'));
                            else if (line.Contains('Y'))
                                line = ModularCommand + line.Substring(line.IndexOf('Y'));
                            else if (line.Contains('Z'))
                                line = ModularCommand + line.Substring(line.IndexOf('Z'));

                            result = new CommandRef(Commands.LINE, ModularCommand, line);
                        }
                        else
                        {
                            if (line.Contains('X'))
                                line = "G0" + line.Substring(line.IndexOf('X'));
                            else if (line.Contains('Y'))
                                line = "G0" + line.Substring(line.IndexOf('Y'));
                            
                            result = new CommandRef(Commands.LINE, "G0", line);
                        }
                    }
                }
                else
                    result = new CommandRef();
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        private bool HasComments(string line)
        {
            bool result = false;

            try
            {
                if (line.Contains('(') || line.Contains(')'))
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public bool LineContainsSecondCommand(CommandRef command)
        {
            bool result = false;

            try
            {
                CommandRef cmd = DetectCommand(command.Line.Substring(command.Line.IndexOf(command.Reference) + command.Reference.Length));

                if (cmd.Command == command.Command && cmd.Line == command.Line && cmd.Reference == command.Reference)
                    result = false;
                else if (Statements.IsNull(cmd))
                    result = false;
                else
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public int[] GetSubIndexs(string[] lines, int startIndex)
        {
            int[] result = new int[]{};

            try
            {
                string[] cmds;
                CommandList.TryGetValue(Commands.SUBEND, out cmds);

                for (int i = startIndex; i < lines.Length; i++)
                {
                    for (int j = 0; j < cmds.Length; j++)
                    {
                        if (lines[i].Contains(cmds[j]))
                        {
                            return result = new int[] { startIndex, i };
                        }
                    }
                }

                result = new int[] { startIndex, lines.Length };
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public Commands GetCommandFromReference(string reference)
        {
            Commands result = Commands.NONE;

            try
            {
                for (int i = 0; i < CommandList.Count; i++)
                {
                    string[] refs = { };
                    CommandList.TryGetValue((Commands)i, out refs);

                    if (refs != null)
                    {
                        if (refs.Contains(reference))
                        {
                            result = (Commands)i;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public Commands[] GetCommandsFromReference(string reference)
        {
            Commands[] result = { };

            try
            {
                for (int i = 0; i < CommandList.Count; i++)
                {
                    string[] refs = { };
                    CommandList.TryGetValue((Commands)i, out refs);

                    if (!Statements.IsNull(refs))
                    {
                        if (refs.Contains(reference))
                        {
                            Array.Resize<Commands>(ref result, result.Length + 1);
                            result[result.Length - 1] = (Commands)i;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public string[] GetReferencesFromList(Commands cmd)
        {
            string[] result = { };

            try
            {
                result = CommandList.ElementAt((int)cmd - 1).Value;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        private void DefineDoubleCommands()
        {
            try
            {
                string[] refs = {};

                for (int i = 0; i < CommandList.Count; i++)
                {
                    for (int j = 0; j < CommandList.Count; j++)
                    {
                        // Skip same command
                        if (i == j)
                            continue;
                        else
                        {
                            string[] refsi = GetReferencesFromList((Commands)i);
                            string[] refsj = GetReferencesFromList((Commands)j);

                            for (int h = 0; h < refsi.Length; h++)
                            {
                                for (int k = 0; k < refsj.Length; k++)
                                {
                                    if (refsi[h] == refsj[k] && !refs.Contains(refsi[h]))
                                    {
                                        Array.Resize<string>(ref refs, refs.Length + 1);
                                        refs[refs.Length - 1] = refsi[h];
                                    }
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < refs.Length; i++)
                {
                    Commands[] cmds = GetCommandsFromReference(refs[i]);

                    for (int j = 0; j < Lines.Length; j++)
                    {
                        SubPassed(Lines[j]);

                        if (Lines[j].Contains(refs[i]))
                        {
                            for (int k = 0; k < cmds.Length; k++)
                            {
                                Commands cmd = cmds[k];

                                switch (cmd)
                                {
                                    case Commands.NONE:
                                        break;
                                    case Commands.LINE:
                                        if (Lines[j].Contains("I") || Lines[j].Contains("J"))
                                        {
                                            List<string> temp = new List<string>((string[])CommandList[Commands.LINE]);

                                            for (int t = 0; t < temp.Count; t++)
                                            {
                                                if (temp[t] == refs[i])
                                                {
                                                    temp.RemoveAt(t);
                                                }
                                            }
                                            
                                            CommandList[Commands.LINE] = temp.ToArray<string>();
                                        }
                                        break;
                                    case Commands.ARC:
                                        break;
                                    case Commands.SUBPROG:
                                        if (InSub)
                                        {
                                            List<string> temp = new List<string>((string[])CommandList[Commands.SUBPROG]);

                                            for (int t = 0; t < temp.Count; t++)
                                            {
                                                if (temp[t] == refs[i])
                                                {
                                                    temp.RemoveAt(t);
                                                }
                                            }
                                            
                                            CommandList[Commands.SUBPROG] = temp.ToArray<string>();
                                        }
                                        break;
                                    case Commands.SUBEND:
                                        break;
                                    case Commands.SUBCALL:
                                        if (!InSub || SubsStarted > 1)
                                        {
                                            List<string> temp = new List<string>((string[])CommandList[Commands.SUBCALL]);
                                            
                                            for (int t = 0; t < temp.Count; t++)
                                            {
                                                if (temp[t] == refs[i])
                                                {
                                                    temp.RemoveAt(t);
                                                }
                                            }

                                            CommandList[Commands.SUBCALL] = temp.ToArray<string>();
                                        }
                                        break;
                                    case Commands.ABSOLUTE:
                                        break;
                                    case Commands.RELATIVE:
                                        break;
                                    case Commands.TOOL:
                                        if (!InSub)
                                        {
                                            List<string> temp = new List<string>((string[])CommandList[Commands.TOOL]);

                                            for (int t = 0; t < temp.Count; t++)
                                            {
                                                if (temp[t] == refs[i])
                                                {
                                                    temp.RemoveAt(t);
                                                }
                                            }

                                            CommandList[Commands.TOOL] = temp.ToArray<string>();
                                        }
                                        break;
                                    case Commands.ON:
                                        break;
                                    case Commands.OFF:
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }
        }

        private bool SubPassed(string line)
        {
            bool result = false;

            try
            {
                if (SubStarted && !InSub)
                    return result = InSub = true;

                string[] subrefs = GetReferencesFromList(Commands.SUBPROG);
                string[] endsubrefs = GetReferencesFromList(Commands.SUBEND);

                for (int i = 0; i < subrefs.Length; i++)
                {
                    if (line.Contains(subrefs[i]))
                    {
                        SubStarted = result = true;
                        break;
                    }
                }

                for (int i = 0; i < endsubrefs.Length; i++)
                {
                    if (line.Contains(endsubrefs[i]))
                    {
                        SubStarted = InSub = result = false;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public bool CompareLines(string line, string compare)
        {
            bool result = false;

            try
            {
                CommandRef cmd1 = DetectCommand(line);
                CommandRef cmd2 = DetectCommand(compare);

                if (cmd1.Command == cmd2.Command)
                {
                    if (cmd1.Reference == cmd2.Reference)
                    {
                        string cut1 = cmd1.Line.Substring(cmd1.Line.IndexOf(cmd1.Reference) + cmd1.Reference.Length).Trim();
                        string cut2 = cmd2.Line.Substring(cmd2.Line.IndexOf(cmd2.Reference) + cmd2.Reference.Length).Trim();

                        if (cut1 == cut2 || cmd1.Command == cmd2.Command)
                            result = true;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public int CountSubs(string[] lines)
        {
            int result = 0;

            try
            {
                string[] refs;
                CommandList.TryGetValue(Commands.SUBPROG, out refs);

                for (int i = 0; i < lines.Length; i++)
                {
                    for (int j = 0; j < refs.Length; j++)
                    {
                        if (lines[i].Contains(refs[j]))
                        {
                            result++;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public bool IsLastSub(int section)
        {
            bool result = false;

            try
            {
                if (section == LastSubIndex)
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public int GetSection(string line)
        {
            int result = 0;

            try
            {
                string[] refs;
                CommandList.TryGetValue(Commands.SUBPROG, out refs);

                for (int i = 0; i < refs.Length; i++)
                {
                    if (line.Contains(refs[i]))
                    {
                        string temp = line.Substring(line.IndexOf(refs[i]) + refs[i].Length);

                        if (int.TryParse(temp, out result))
                            break;
                        else
                        {
                            temp = temp.Substring(0, FindNextLetterIndex(temp));
                            int.TryParse(temp, out result);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public int FindNextLetterIndex(string line)
        {
            int result = 0;

            try
            {
                for (int i = 0; i < line.Length; i++)
                {
                    int x = 0;

                    if (!int.TryParse(line[i].ToString(), out x))
                    {
                        result = i;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public int GetLastSubIndex()
        {
            int result = 0;

            try
            {
                string[] refs;
                CommandList.TryGetValue(Commands.SUBPROG, out refs);

                for (int i = 0; i < Lines.Length; i++)
                {
                    for (int j = 0; j < refs.Length; j++)
                    {
                        if (Lines[i].Contains(refs[j]))
                        {
                            int temp = GetSection(Lines[i]);

                            if (temp > result || (result == 100 && temp != 0))
                                result = temp;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }
    }

    public struct CommandRef
    {
        public Commands Command;
        public string Reference;
        public string Line;

        public CommandRef(Commands command, string reference, string line)
        {
            Command = command;
            Reference = reference;
            Line = line;
        }
    }
}
