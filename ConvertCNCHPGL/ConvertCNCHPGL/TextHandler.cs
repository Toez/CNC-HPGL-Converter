using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ConvertCNCHPGL
{
    class TextHandler
    {
        public TextHandler()
        {

        }

        /// <summary>
        /// Checks if the first letter of the string is a letter or not
        /// </summary>
        /// <param name="Str">String you want to check for letters</param>
        /// <returns>True if string is a letter, false if not</returns>
        public virtual bool IsLetter(string str)
        {
            try
            {
                // comparaison of the first character with letters from A -> Z
                if (str[0] >= 'A' && str[0] <= 'Z')
                {
                    return true;
                }

                // comparaison of the first character with letters from a -> z
                if (str[0] >= 'a' && str[0] <= 'z')
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return false;
        }

        /// <summary>
        /// Tries to parse a string to a number, if succes "str" is a number
        /// </summary>
        /// <param name="str">Convertable string to number</param>
        /// <returns>True if conversion succeeds, false if "str" is not a number</returns>
        public virtual bool IsNumber(string str)
        {
            try
            {
                // Create output number
                int outp = 0;
                // Return true if convertable, else return false
                return int.TryParse(str, out outp);
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
                // Return false on error, string is not convertable
                return false;
            }
        }

        /// <summary>
        /// Cuts line into a short command
        /// </summary>
        /// <param name="line">Current line</param>
        /// <returns>Command cut from "line"</returns>
        public virtual string CutCommand(string line)
        {
            try
            {
                string result = "";
                // Index of first letter in line
                int letterIndex = 0;
                // Index of second letter
                int sLetterIndex = 0;
                // Index of first number in line
                int numberIndex = 0;
                // Found first letter
                bool fLetter = false;
                // Found first number
                bool fNumber = false;
                // Second letter if there is no number
                bool sLetter = false;
                bool coord = false;
                
                // Skip line if it's empty
                if (line == "")
                    return "";
                if (line == "%")
                    return line;
                if (line.Contains("DFS"))
                    return "DFS";
                if (line.Contains('H') && line.Contains('M') && IsNumber(line[1].ToString()))
                    return line[0].ToString();
                if (line.Contains('T') && line.Contains('M'))
                    return "T";

                for (int i = 0; i < line.Length; i++)
                {
                    // Check if current character is a letter
                    if (IsLetter(line[i].ToString()))
                    {
                        if (line[i] == 'X' && !fLetter)
                        {
                            coord = true;
                            letterIndex = i;
                            fLetter = true;
                            break;
                        }
                        else if (!fLetter)
                            letterIndex = i;
                        // If first number is found and second letter is found break the loop
                        else if (fNumber && sLetter && fLetter)
                            break;
                        else if (fNumber && fLetter)
                        {
                            sLetter = true;
                            sLetterIndex = i;
                        }

                        fLetter = true;
                    }
                    // Check if current character is a number
                    else if (IsNumber(line[i].ToString()) && fLetter)
                    {
                        if (!sLetter)
                            numberIndex = i;
                        fNumber = true;
                    }
                }

                if (line[0] == 'P')
                    result = line[0].ToString();
                else if (coord)
                    result = line[letterIndex].ToString();
                else if (fLetter && !sLetter && numberIndex == letterIndex + 1)
                    result = line[letterIndex].ToString();
                else if (sLetter)
                    result = line.Substring(letterIndex, sLetterIndex - letterIndex);
                else if (fLetter && numberIndex > 0 && !sLetter)
                    result = line.Substring(letterIndex, 3);
                // String can't be cut if there is no numberIndex
                else if (numberIndex > 0)
                    result = line.Substring(letterIndex, numberIndex - letterIndex + 1);
                // If there is no number, use first letter
                else if (fLetter)
                    result = line.Substring(letterIndex, 1);

                return result.Trim();
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
                return "";
            }
        }

        /// <summary>
        /// Vertifies if the dictionary contains the command
        /// </summary>
        /// <param name="cmd">Command you want to vertify</param>
        /// <param name="dictionary">Dictionary of commands and methods</param>
        /// <returns>True if command exists in dictionary, false if it doesn't</returns>
        public virtual bool VertifyCommand(string cmd, Dictionary<string, Func<string, bool>> dictionary)
        {
            try
            {
                bool result = false;

                // Search for command in dictionary
                if (dictionary.ContainsKey(cmd))
                    result = true;

                return result;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
                return false;
            }
        }

        /// <summary>
        /// Cuts the line to the next command
        /// </summary>
        /// <param name="line">Current line</param>
        /// <param name="lastCmd">Previous command</param>
        /// <returns>A new line with the next command or nothing if there is no command</returns>
        public virtual string NextCommand(string line, string lastCmd)
        {
            try
            {
                string result = "";

                // If the line only has 1 character or less it's useless (Skip it)
                if (line.Length <= 1 && line != "%")
                    return result;
                else if (line == "%")
                    return "";

                // Delete last command from line
                line = line.Substring(line.IndexOf(lastCmd) + lastCmd.Length);

                // Search for next letter (Command)
                for (int i = 0; i < line.Length; i++)
                {
                    if (IsLetter(line[i].ToString()))
                    {
                        result = line.Substring(i);
                        break;
                    }
                }

                return result.Trim();
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
                return "";
            }
        }

        /// <summary>
        /// Cleans comments from line
        /// </summary>
        /// <param name="line">Current line</param>
        /// <returns>Returns the line without comments or empty</returns>
        public virtual string CleanComments(string line)
        {
            try
            {
                string result = "";
                string tempLine = line;
                // Things to clean
                string[] seperator = { "N", "F#" };
                // Position of things to clean
                int[] pos = { };
                // Count of given line number commands
                int lineNumbers = 2;
                bool fLetter = false;

                // Skip line if it contains DFS or nothing
                if (line.Contains("DFS") || line == "")
                    return line;

                for (int i = 0; i < seperator.Length; i++)
                {
                    // Resize array to make room for a new position
                    Array.Resize<int>(ref pos, pos.Length + 1);
                    // Add position at the end of the array
                    pos[pos.Length - 1] = line.IndexOf(seperator[i]);
                }

                for (int ind = 0; ind < lineNumbers; ind++)
                {
                    if (line.Contains(seperator[ind]))
                    {
                        // Find position of next letter or space
                        for (int i = pos[ind]; i < line.Length; i++)
                        {
                            if (IsLetter(line[i].ToString()) || line[i] == ' ')
                            {
                                // Second letter found
                                if (fLetter)
                                {
                                    // Cut the front of the string
                                    tempLine = line.Substring(i);
                                    // Stop searching for more letter if second letter is found
                                    break;
                                }
                                
                                // Cut left end of the string
                                tempLine = line.Substring(i + 1);

                                // First letter found
                                if (!fLetter)
                                    fLetter = true;
                            }
                        }
                    }
                }

                result = tempLine;

                return result.Trim();
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
                return "";
            }
        }

        /// <summary>
        /// Removes numbers at the end of a string
        /// </summary>
        /// <param name="line">Current line</param>
        /// <returns>Returns back "line" without numbers</returns>
        public virtual string RemoveNumbers(string line)
        {
            try
            {
                string result = "";

                for (int i = 0; i < line.Length; i++)
                {
                    // Check if the current character is a number
                    if (IsNumber(line[i].ToString()))
                    {
                        // Cut numbers from the end of the line
                        result = line.Substring(0, i);
                        break;
                    }
                }

                return result.Trim();
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
                return "";
            }
        }

        public virtual Dictionary<string, Func<string, bool>> ConstructDictionary(BaseFunctions FunctionClass)
        {
            string xmlRef = "";

            // Check if inherited class from BaseFunctions is HPGL or CNC
            if (FunctionClass.GetType().Name == "HPGLFunctions")
                xmlRef = "HPGLCmds";
            else if (FunctionClass.GetType().Name == "CNCFunctions")
                xmlRef = "CNCCmds";
            
            // Get commands from xml file
            XmlNode functions = new Module_XML().ReturnXml("Settings.xml", xmlRef);
            // Create temporary dictionary to save all commands
            Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
            // Output dictionary
            Dictionary<string, Func<string, bool>> commands = new Dictionary<string, Func<string, bool>>();

            // Add each command to temporary dictionary
            foreach (XmlNode item in functions)
            {
                // Node can contain mutiple commands
                dictionary.Add(item.Name, item.InnerText.Split(','));
            }

            // Add all commands and functions to the output dictionary
            foreach (var item in dictionary)
            {
                foreach (string cmd in item.Value)
                {
                    // Check if command is not empty
                    if (cmd.Trim() != "")
                    {
                        // Add command to function
                        switch (item.Key)
                        {
                            case "Relative":
                                commands.Add(cmd.Trim(), FunctionClass.Relative);
                                break;
                            case "Arc":
                                commands.Add(cmd.Trim(), FunctionClass.Arc);
                                break;
                            case "Line":
                                commands.Add(cmd.Trim(), FunctionClass.Line);
                                break;
                            case "On":
                                commands.Add(cmd.Trim(), FunctionClass.On);
                                break;
                            case "Tool":
                                commands.Add(cmd.Trim(), FunctionClass.Tool);
                                break;
                            case "CallSub":
                                commands.Add(cmd.Trim(), FunctionClass.CallSubmain);
                                break;
                            case "StartSub":
                                commands.Add(cmd.Trim(), FunctionClass.StartSub);
                                break;
                            case "EndSub":
                                commands.Add(cmd.Trim(), FunctionClass.EndSub);
                                break;
                            case "Absolute":
                                commands.Add(cmd.Trim(), FunctionClass.Absolute);
                                break;
                            case "Off":
                                commands.Add(cmd.Trim(), FunctionClass.Off);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return commands;
        }
    }
}
