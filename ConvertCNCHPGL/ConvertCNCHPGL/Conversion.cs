using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class Conversion
    {
        // Main program
        private Class_Program _Prog;
        public Class_Program Prog
        {
            get
            {
                return _Prog;
            }
            set
            {
                _Prog = value;
            }
        }

        // Output class
        private Class_Output _Output;
        public Class_Output Output
        {
            get
            {
                return _Output;
            }
            set
            {
                _Output = value;
            }
        }

        // To handle text and commands
        private TextHandler _TextHdnlr;
        public TextHandler TextHndlr
        {
            get
            {
                return _TextHdnlr;
            }
            set
            {
                _TextHdnlr = value;
            }
        }

        // Function class
        private BaseFunctions _Functions;
        public BaseFunctions Functions
        {
            get
            {
                return _Functions;
            }
            set
            {
                _Functions = value;
            }
        }

        private LogFileOutput _LogFile;
        public LogFileOutput LogFile
        {
            get
            {
                return _LogFile;
            }
            set
            {
                _LogFile = value;
            }
        }

        // Dictionary with all commands and functions
        private Dictionary<string, Func<string, bool>> _FunctionDic;
        public Dictionary<string, Func<string, bool>> FunctionDic
        {
            get
            {
                return _FunctionDic;
            }
            set
            {
                _FunctionDic = value;
            }
        }

        // Output file name
        private string _FileName;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            private set
            {
                _FileName = value;
            }
        }

        // Current line in file
        private string CurrentLine
        {
            get
            {
                return Prog.CurrentLine;
            }
        }

        // Total file lines
        public int ProgramLines
        {
            get
            {
                return Prog.gNbProgramLines;
            }
        }

        // Current line index in file
        private int _CurrentLineIndex;
        public int CurrentLineIndex
        {
            get
            {
                return _CurrentLineIndex;
            }
            set
            {
                // Change Prog's line index because it's the same
                if (value == _CurrentLineIndex + 1)
                    Prog.CurrentLineIndex++;

                _CurrentLineIndex = value;
            }
        }

        // Current command
        private string _Command;
        public string Command
        {
            get
            {
                return _Command;
            }
            set
            {
                _Command = value;
            }
        }

        // Previous command
        private string _LastCommand;
        public string LastCommand
        {
            get
            {
                return _LastCommand;
            }
            set
            {
                _LastCommand = value;
            }
        }

        public Conversion(string FileName)
        {
            this.FileName = FileName;
            Prog = new Class_Program();
            Output = Module_Main.outp;
            TextHndlr = new HPGLTextHandler();
            Functions = new BaseFunctions(Output);
            FunctionDic = TextHndlr.ConstructDictionary(Functions);
            LogFile = new LogFileOutput(FileName);
        }

        public Conversion(string FileName, Class_Program Prog)
        {
            this.FileName = FileName;
            this.Prog = Prog;
            Output = Module_Main.outp;
            TextHndlr = new HPGLTextHandler();
            Functions = new BaseFunctions(Output);
            FunctionDic = TextHndlr.ConstructDictionary(Functions);
            LogFile = new LogFileOutput(FileName);
        }

        public Conversion(string FileName, Class_Program Prog, Class_Output Output)
        {
            this.FileName = FileName;
            this.Prog = Prog;
            this.Output = Output;
            TextHndlr = new HPGLTextHandler();
            Functions = new BaseFunctions(this.Output);
            FunctionDic = TextHndlr.ConstructDictionary(Functions);
            LogFile = new LogFileOutput(FileName);
        }

        public Conversion(string FileName, Class_Program Prog, Class_Output Ouput, BaseFunctions Functions)
        {
            this.FileName = FileName;
            this.Prog = Prog;
            this.Output = Module_Main.outp;
            TextHndlr = new HPGLTextHandler();
            this.Functions = Functions;
            FunctionDic = TextHndlr.ConstructDictionary(Functions);
            LogFile = new LogFileOutput(FileName.Substring(0, FileName.IndexOf('.')) + ".txt");
        }

        public Conversion(string FileName, Class_Program Prog, Class_Output Output, BaseFunctions Functions, Dictionary<string, Func<string, bool>> FunctionDic)
        {
            this.FileName = FileName;
            this.Prog = Prog;
            this.Output = Output;
            TextHndlr = new HPGLTextHandler();
            this.Functions = Functions;
            this.FunctionDic = FunctionDic;
            LogFile = new LogFileOutput(FileName);
        }

        public bool Start()
        {
            // Open the new file
            if (!Output.OpenFile(FileName))
                return false;
            // Write header in new file
            Output.OutputFileHeader(FileName);

            string sectionName = "";

            // Write HPGL or CNC in section
            if (Functions.GetType().Name == "HPGLFunctions")
                sectionName = "HPGLFile";
            else if (Functions.GetType().Name == "CNCFunctions")
                sectionName = "CNCFile";
            
            // Write box if CNC
            if (sectionName == "CNCFile")
            {
                Output.OutputBoxStart(sectionName);
                //Output.OutputNullInstance();
                Functions.SubOpen = true;
                Functions.BoxOpen = true;
            }
            // Write section if HPGL
            else
                Output.OutputSectionStart(sectionName, "1");
            
            // If there is only 1 sub, start writing section
            if (Prog.SubCount <= 1 && sectionName != "HPGLFile")
            {
                Output.OutputSectionStart(sectionName, "1");
                Functions.SubOpen = true;
                Functions.SubFirst = false;
            }
            
            // Start writing log file
            LogFile.WriteStart();

            Functions.LastSub = Prog.submains;

            // Process all lines
            for (CurrentLineIndex = 0; CurrentLineIndex < ProgramLines; CurrentLineIndex++)
            {
                // Process current line
                bool conv = ConvertLine(CurrentLine);
                //LogFile.WriteLine(CurrentLine, conv);
            }

            // Close section if sub is still open
            if (Functions.SubOpen || Prog.SubCount <= 1)
            {
                Output.OutputSectionEnd();
                Functions.SubOpen = false;
            }
            
            // Close box if CNC
            if (sectionName == "CNCFile" && Functions.BoxOpen)
            {
                Output.OutputBoxEnd();
                Functions.SubOpen = false;
            }
            // Close section if HPGL
            else if (Functions.SubOpen)
                Output.OutputSectionEnd();

            // Write end of file
            Output.OutputFileEnd();
            // Close and save file
            Output.CloseFile();

            LogFile.SaveFile();
            
            return true;
        }

        private bool ConvertLine(string Line)
        {
            bool result = false;

            /// Delete comments from current line
            Line = TextHndlr.CleanComments(Line);

            do
            {
                // Get command from current line
                Command = TextHndlr.CutCommand(Line);
                
                // Check if command is avaible, invoke command's method with current line
                if (FunctionDic.ContainsKey(Command))
                    result = FunctionDic[Command].Invoke(Line);
                else if (Command != "" && (Command[0] == 'X' || Command[0] == 'Y') && Functions.ModularCommand != null)
                {
                    // Get reoccurring command in line
                    Command = Functions.ModularCommand;
                    // Create new line for reoccuring command
                    Line = Command + Line;
                    // Invoke new commandline
                    result = FunctionDic[Command].Invoke(Line);
                }

                // Save command as old command
                LastCommand = Command;
                // Look for next command in line
                Line = TextHndlr.NextCommand(Line, Command);

                // Skip line if the pen already moved
                if (LastCommand == "G01" || LastCommand == "G1" || LastCommand == "G02" || LastCommand == "G2" || LastCommand == "G03" || LastCommand == "G3" || LastCommand == "G0" || LastCommand == "G00" || LastCommand == "DFS")
                    Line = "";
            } while (Line != "");

            return result;
        }
    }
}
