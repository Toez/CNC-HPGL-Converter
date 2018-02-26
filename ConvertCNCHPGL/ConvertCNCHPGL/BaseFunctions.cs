using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class BaseFunctions
    {
        // Handles all texts and command lines
        private TextHandler _TextHndlr;
        protected TextHandler TextHndlr
        {
            get
            {
                return _TextHndlr;
            }
            set
            {
                _TextHndlr = value;
            }
        }

        // Writes output to file
        private Class_Output _Output;
        protected Class_Output Output
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

        // Paint if true
        private bool _PenDown;
        protected bool PenDown
        {
            get
            {
                return _PenDown;
            }
            set
            {
                _PenDown = value;
            }
        }

        // Coordinates are relative if true
        private bool _RelativeCoord;
        protected bool RelativeCoord
        {
            get
            {
                return _RelativeCoord;
            }
            set
            {
                _RelativeCoord = value;
            }
        }

        // True if first submain is found
        private bool _SubFirst;
        public bool SubFirst
        {
            get
            {
                return _SubFirst;
            }
            set
            {
                _SubFirst = value;
            }
        }

        // X coordinate
        private double _X;
        protected double X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        // Old X coordinate
        private double _LastX;
        protected double LastX
        {
            get
            {
                return _LastX;
            }
            set
            {
                _LastX = value;
            }
        }

        // New X coordinate
        private double _NewX;
        protected double NewX
        {
            get
            {
                return _NewX;
            }
            set
            {
                _NewX = value;
            }
        }

        // Arc length X
        protected double DiX
        {
            get
            {
                return LastX - I;
            }
        }

        // Y coordinate
        private double _Y;
        protected double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        // Old Y coordinate
        private double _LastY;
        protected double LastY
        {
            get
            {
                return _LastY;
            }
            set
            {
                _LastY = value;
            }
        }

        // New Y coordinate
        private double _NewY;
        protected double NewY
        {
            get
            {
                return _NewY;
            }
            set
            {
                _NewY = value;
            }
        }

        // Arc length Y
        protected double DiY
        {
            get
            {
                return LastY - J;
            }
        }

        // Distance from the start point of the arc to the arc center along the X axis
        private double _I;
        protected double I
        {
            get
            {
                return _I;
            }
            set
            {
                _I = value;
            }
        }

        // Distance from the start point of the arc to the arc center along the Y axis
        private double _J;
        protected double J
        {
            get
            {
                return _J;
            }
            set
            {
                _J = value;
            }
        }

        // Arc radius
        private double _Radius;
        protected double Radius
        {
            get
            {
                double sum = Math.Sqrt(DiX * DiX + DiY * DiY);

                if (_Radius != -sum)
                    _Radius = sum;

                return _Radius;
            }
            set
            {
                // Only allow change to negative
                if (_Radius == -value)
                    _Radius = value;
                else
                    _Radius = Math.Sqrt(DiX * DiX + DiY * DiY);
            }
        }

        // Pen tool color
        private int _Color;
        protected int Color
        {
            get
            {
                return _Color;
            }
            set
            {
                _Color = value;
            }
        }

        // NewSubMain index
        private int _NewSubMain;
        protected int NewSubMain
        {
            get
            {
                return _NewSubMain;
            }
            set
            {
                _NewSubMain = value;
            }
        }

        // Current sub index
        private string _CurrentSub;
        public string CurrentSub
        {
            get
            {
                return _CurrentSub;
            }
            set
            {
                _CurrentSub = value;
            }
        }

        private bool _SubOpen;
        public bool SubOpen
        {
            get
            {
                return _SubOpen;
            }
            set
            {
                _SubOpen = value;
            }
        }

        private bool _BoxOpen;
        public bool BoxOpen
        {
            get
            {
                return _BoxOpen;
            }
            set
            {
                _BoxOpen = value;
            }
        }

        private int _LastSub;
        public int LastSub
        {
            get
            {
                return _LastSub;
            }
            set
            {
                _LastSub = value;
            }
        }

        private int _LastSubCall;
        public int LastSubCall
        {
            get
            {
                return _LastSubCall;
            }
            set
            {
                _LastSubCall = value;
            }
        }

        private bool IsLastSub
        {
            get
            {
                return LastSub.ToString() == CurrentSub;
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

        private Dictionary<string, int> _SubColors;
        public Dictionary<string, int> SubColors
        {
            get
            {
                return _SubColors;
            }
            set
            {
                _SubColors = value;
            }
        }

        /// <summary>
        /// Default constructor. Sets properties to default value.
        /// </summary>
        public BaseFunctions()
        {
            TextHndlr = new TextHandler();
            Output = Module_Main.outp;
            PenDown = false;
            RelativeCoord = false;
            SubFirst = true;
            SubOpen = false;
            BoxOpen = true;
            SubColors = new Dictionary<string, int>();
            SubColors.Add("Zero", 0);
        }

        public BaseFunctions(Class_Output Output)
        {
            TextHndlr = new TextHandler();
            this.Output = Output;
            PenDown = false;
            RelativeCoord = false;
            SubFirst = true;
            SubOpen = false;
            BoxOpen = true;
            SubColors = new Dictionary<string, int>();
            SubColors.Add("Zero", 0);
        }

        /// <summary>
        /// Replaces A and K coordinates to Y and J
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        public virtual void ReplaceAxis(ref string cmdLine)
        {
            if (cmdLine.ToUpper().Contains('A'))
            {
                if (cmdLine.IndexOf('A') > cmdLine.IndexOf('X'))
                    cmdLine = cmdLine.Replace('A', 'Y');

                if (cmdLine.ToUpper().Contains('K'))
                    cmdLine = cmdLine.Replace('K', 'J');
            }
        }

        /// <summary>
        /// Creates a line and writes it in output file
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>Returns true or false whether line is drawn or not</returns>
        public virtual bool Line(string cmdLine)
        {
            ReplaceAxis(ref cmdLine);

            if (((cmdLine.Contains("G0") || cmdLine.Contains("G00")) && !cmdLine.Contains("G01")) && SubColors.ContainsKey(CurrentSub))
                Color = SubColors[CurrentSub];
            
            // If there is no axis, skip command
            if (!cmdLine.Contains('X') && !cmdLine.Contains('Y'))
            {
                ModularCommand = TextHndlr.CutCommand(cmdLine);
                
                if (cmdLine.Contains('Z'))
                    PenDown = cmdLine[cmdLine.IndexOf('Z') + 1] == '-';

                return true;
            }
            
            // Detect move command
            if ((cmdLine.Contains("G0") || cmdLine.Contains("G00")) && !cmdLine.Contains("G01"))
                PenDown = false;
            else if (!cmdLine.Contains("G0") && !cmdLine.Contains("G00") && !cmdLine.Contains("G01") && !cmdLine.Contains("G1") && cmdLine.Contains("X"))
                PenDown = false;
            else
                PenDown = true;

            // Get first command
            string fCmd = TextHndlr.CutCommand(cmdLine);
            // Get begin of coordinates
            if (fCmd != "X")
                cmdLine = TextHndlr.NextCommand(cmdLine, fCmd);
            // If X is still not found, use modular command
            if (fCmd != "X")
                ModularCommand = fCmd;

            if (cmdLine.Contains("G76"))
                cmdLine = TextHndlr.NextCommand(cmdLine, fCmd);

            // Split the string for X and Y coords
            if (cmdLine.Contains('X') && cmdLine.Contains('Y'))
            {
                string[] cmds = cmdLine.Split('X', 'Y');
                X = double.Parse(cmds[1], CultureInfo.InvariantCulture);
                Y = double.Parse(cmds[2], CultureInfo.InvariantCulture);
            }
            // Check if only X has to be changes
            else if (cmdLine.Contains('X'))
            {
                // If the line contains an F, cut number from X to F
                if (cmdLine.Contains('F'))
                    X = double.Parse(cmdLine.Substring(cmdLine.IndexOf('X') + 1, cmdLine.IndexOf('F') - cmdLine.IndexOf('X') - 1).Trim());
                else
                    X = double.Parse(cmdLine.Substring(cmdLine.IndexOf('X') + 1).Trim());
            }
            // Check if only Y has to be changes
            else if (cmdLine.Contains('Y'))
            {
                // If the line contains an F, cut number from Y to F
                if (cmdLine.Contains('F'))
                    Y = double.Parse(cmdLine.Substring(cmdLine.IndexOf('Y') + 1, cmdLine.IndexOf('F') - cmdLine.IndexOf('Y') - 1).Trim());
                else
                    Y = double.Parse(cmdLine.Substring(cmdLine.IndexOf('Y') + 1).Trim());
            }
            
            // If line contains Z, use Z to move pen up and down
            if (cmdLine.Contains('Z'))
                PenDown = cmdLine[cmdLine.IndexOf('Z') + 1] == '-';
            
            // Add old coordinates if coordinates are relative
            NewX = (RelativeCoord) ? X + LastX : X;
            NewY = (RelativeCoord) ? Y + LastY : Y;

            // Draw line
            if (PenDown)
                Output.OutputLine(LastX, LastY, NewX, NewY, Color);

            // Set new coordinates to old coordinates
            LastX = NewX;
            LastY = NewY;

            if (PenDown)
                ModularCommand = "G1";
            else
                ModularCommand = "G0";

            return true;
        }

        /// <summary>
        /// Creates an arc in the output file
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True of false whether the arc is drawn</returns>
        public virtual bool Arc(string cmdLine)
        {
            ReplaceAxis(ref cmdLine);

            if (((cmdLine.Contains("G0") || cmdLine.Contains("G00")) && !cmdLine.Contains("G01")) && SubColors.ContainsKey(CurrentSub))
                Color = SubColors[CurrentSub];
            
            // Detect move command
            //if ((cmdLine.Contains("G2") || cmdLine.Contains("G02")) && !cmdLine.Contains("G03"))
            //    PenDown = false;
            //else
            //    PenDown = true;

            // Get first command
            string fCmd = TextHndlr.CutCommand(cmdLine);
            cmdLine = TextHndlr.NextCommand(cmdLine, fCmd);
            
            // If X is still not found, use modular command
            if (!cmdLine.Contains('X') && !cmdLine.Contains('Y'))
            {
                ModularCommand = fCmd;

                if (cmdLine.Contains('Z'))
                    PenDown = cmdLine[cmdLine.IndexOf('Z') + 1] == '-';

                return true;
            }

            // Split the string
            string[] cmds;
            if (!cmdLine.Contains("F"))
                cmds = cmdLine.Split('X', 'Y', 'I', 'J');
            else
                cmds = cmdLine.Split('X', 'Y', 'I', 'J', 'F');

            X = double.Parse(cmds[1], CultureInfo.InvariantCulture);
            Y = double.Parse(cmds[2], CultureInfo.InvariantCulture);
            I = double.Parse(cmds[3], CultureInfo.InvariantCulture);
            J = double.Parse(cmds[4], CultureInfo.InvariantCulture);

            // Add old coordinates if coordinates are relative
            NewX = (RelativeCoord) ? X + LastX : X;
            NewY = (RelativeCoord) ? Y + LastY : Y;

            // Add old centre coordinates if whole program is relative
            if (Module_Main.RelativeArc)
            {
                I = I + LastX;
                J = J + LastY;
            }
            // Add old centre coordinates if centre coordinates are relative
            else
            {
                I = (RelativeCoord) ? I + LastX : I;
                J = (RelativeCoord) ? J + LastY : J;
            }
            
            // Skip if radius is too small
            if (Radius == 0)
                return false;

            // Draw circle if coordinates are from the same origen
            if (LastX == NewX && LastY == NewY)
            {
                // Draw circle
                if (PenDown)
                    Output.OutputCircle(I, J, Radius, Color);
            }
            else
            {
                // otherwise it is an arc
                if (fCmd == "G02" || fCmd == "G2")
                    Radius = -Radius;
                //  Draw arc
                if (PenDown)
                    Output.OutputArc(LastX, LastY, NewX, NewY, I, J, Radius, Color);
            };

            // Set new coordinates to old coordinates
            LastX = NewX;
            LastY = NewY;

            ModularCommand = fCmd;

            return true;
        }

        /// <summary>
        /// Sets pen mode to relative
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True or false whether relative is set to true</returns>
        public virtual bool Relative(string cmdLine)
        {
            // Draw coordinates in relative mode
            RelativeCoord = true;
            return true;
        }

        /// <summary>
        /// Sets pen mode to absolute
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True or false whether absolute is set to true</returns>
        public virtual bool Absolute(string cmdLine)
        {
            // Draw coordinates in absolute mode
            RelativeCoord = false;
            return true;
        }

        /// <summary>
        /// Sets pen to down (Starts drawing)
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True or false whether the pen is down</returns>
        public virtual bool On(string cmdLine)
        {
            PenDown = true;
            return true;
        }

        /// <summary>
        /// Sets pen to up (Stops drawing)
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True or false whether the pen is up</returns>
        public virtual bool Off(string cmdLine)
        {
            PenDown = false;
            return true;
        }

        /// <summary>
        /// Sets color of the pen
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True or false whether tool is changed or not</returns>
        public virtual bool Tool(string cmdLine)
        {
            if (cmdLine[cmdLine.Length - 1] == ';')
                cmdLine = cmdLine.Substring(0, cmdLine.Length - 1).Trim();

            if (cmdLine[0] == 'M')
                cmdLine = TextHndlr.NextCommand(cmdLine, "M");

            int color;

            // Skip command after tool select
            if (cmdLine.Contains('F'))
                color = int.Parse(cmdLine.Substring(1, cmdLine.IndexOf('F') - 1).Trim());
            else if (cmdLine.Contains('H'))
                color = int.Parse(cmdLine.Substring(1, cmdLine.IndexOf('M') - 1).Trim());
            else if (cmdLine.Contains('T'))
            {
                if (cmdLine.Substring(1).Contains('M'))
                    color = int.Parse(cmdLine.Substring(1, cmdLine.IndexOf('M') - 1));
                else
                    color = int.Parse(cmdLine.Substring(1).Trim());
            }
            else if (cmdLine.Contains("SP"))
                color = int.Parse(cmdLine.Substring(2).Trim());
            else if (TextHndlr.IsNumber(cmdLine.Substring(1)))
                color = int.Parse(cmdLine.Substring(1).Trim());
            else
                color = Color;

            // Set color
            Color = color;

            return true;
        }

        /// <summary>
        /// Call a new sub
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True of false whether a new sub is called or not</returns>
        public virtual bool CallSubmain(string cmdLine)
        {
            if (TextHndlr.IsNumber(cmdLine[1].ToString()))
            {
                NewSubMain = int.Parse(cmdLine.Substring(1), CultureInfo.InvariantCulture);
                // Call the sub
                Output.OutputInstance(NewSubMain, LastX, LastY);

                if (!SubColors.ContainsKey(NewSubMain.ToString()))
                    SubColors.Add(NewSubMain.ToString(), Color);
            }

            return true;
        }

        /// <summary>
        /// Begins writing a new sub
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True of false whether a new sub began</returns>
        public virtual bool StartSub(string cmdLine)
        {
            string result = "";
            // Sub keywords
            string[] subRefs = { "P", "O", "DFS", "$PROGIN", "%" };
            // String used to detect start of sub
            string thisSubRef = "";
            bool subFound = false;
            
            // Look for the sub reference in line
            for (int i = 0; i < cmdLine.Length && !subFound; i++)
            {
                for (int n = 0; n < subRefs.Length && !subFound; n++)
                {
                    if (cmdLine[i] == subRefs[n][0])
                    {
                        if (cmdLine.Substring(i, subRefs[n].Length) == subRefs[n])
                        {
                            subFound = true;
                            thisSubRef = subRefs[n];
                        }
                    }
                }
            }
            
            // Get index of current sub
            switch (thisSubRef)
            {
                case "P":
                    result = cmdLine.Substring(cmdLine.IndexOf(thisSubRef) + thisSubRef.Length);
                    break;
                case "O":
                    result = cmdLine.Substring(cmdLine.IndexOf(thisSubRef) + thisSubRef.Length);
                    break;
                case "DFS":
                    result = cmdLine.Substring(cmdLine.IndexOf('P') + 1, (cmdLine.IndexOf(',') + cmdLine.Substring(cmdLine.IndexOf(',') + 1).IndexOf(',')) - cmdLine.IndexOf('P'));
                    break;
                case "$PROGIN":
                    result = cmdLine.Substring(cmdLine.IndexOf(thisSubRef) + thisSubRef.Length);
                    break;
                case "%":
                    if (cmdLine.Length > 1 && TextHndlr.IsNumber(cmdLine.Substring(cmdLine.IndexOf("%")) + 1))
                        result = cmdLine.Substring(cmdLine.IndexOf(thisSubRef) + thisSubRef.Length);
                    else
                        return true;
                    break;
                default:
                    result = "";
                    break;
            }

            if (TextHndlr.IsNumber(result))
                CurrentSub = result;

            if (TextHndlr.IsNumber(result) && (!SubFirst && !IsLastSub) && !SubOpen)
            {
                // Change current sub
                CurrentSub = result;
                
                // Start new section
                Output.OutputSectionStart(CurrentSub, CurrentSub);
                
                // The sub is now open
                SubOpen = true;
                
                LastX = 0;
                LastY = 0;
            }
            else if (TextHndlr.IsNumber(result) && (SubFirst || IsLastSub) && !SubOpen)
            {
                CurrentSub = result;
                BoxOpen = true;
                
                Output.OutputBoxEnd();
                Output.OutputBoxStart(CurrentSub);
                Output.OutputSectionStart(CurrentSub, CurrentSub);

                LastX = 0;
                LastY = 0;
            }
            
            return true;
        }

        /// <summary>
        /// Start writing the end of a sub
        /// </summary>
        /// <param name="cmdLine">Current command line</param>
        /// <returns>True of false whether the sub has ended</returns>
        public virtual bool EndSub(string cmdLine)
        {
            // End section
            if ((!SubFirst || !IsLastSub) && SubOpen && !BoxOpen)
            {
                Output.OutputSectionEnd();
            }
            else if (BoxOpen)
            {
                Output.OutputBoxEnd();
                BoxOpen = false;
            }
            
            // First sub ended
            SubFirst = false;
            // the sub is now closed
            SubOpen = false;

            return true;
        }

        public virtual bool OutputSub(string cmdLine)
        {
            string isub = "0";

            if (cmdLine.Contains("DFS,P"))
            {
                string temp = cmdLine.Substring(cmdLine.LastIndexOf("P") + 1);
                isub = temp.Substring(0, temp.IndexOf(",")).Trim();

                if (temp.Substring(temp.IndexOf(",") + 1).Trim() != "")
                {
                    Output.OutputBoxStart("CKNIFE");
                    Output.OutputNullInstance();
                    Output.OutputSectionStart("CKNIFE", "1");
                    Output.OutputNullInstance();
                    return true;
                }
            }
            if (cmdLine.Contains("%"))
                isub = cmdLine.Substring(1).Trim();

            if (isub == "1")
            {
                Output.OutputBoxStart("CKNIFE");
                Output.OutputNullInstance();
                Output.OutputSectionStart("CKNIFE", "1");
                Output.OutputNullInstance();
            }

            Output.OutputSectionStart(isub, "1");

            return true;
        }

        public virtual bool Rotary(string cmdLine)
        {
            string fCmd = cmdLine.Substring(0, cmdLine.IndexOf('X')).Trim();
            cmdLine = TextHndlr.NextCommand(cmdLine, fCmd);

            string[] cmds = cmdLine.Split('X', 'A', 'R');
            X = double.Parse(cmds[1], CultureInfo.InvariantCulture);
            Y = double.Parse(cmds[2], CultureInfo.InvariantCulture);
            I = double.Parse(cmds[3], CultureInfo.InvariantCulture);

            NewX = (RelativeCoord) ? X + LastX : X;
            NewY = (RelativeCoord) ? Y * I + LastY : Y * I;

            if (PenDown)
                Output.OutputLine(LastX, LastY, NewX, NewY, Color);

            LastX = NewX;
            LastY = NewY;

            return true;
        }
    }
}
