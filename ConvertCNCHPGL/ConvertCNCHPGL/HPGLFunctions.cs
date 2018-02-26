using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class HPGLFunctions : BaseFunctions
    {
        private bool _PolygonMode;
        protected bool PolygonMode
        {
            get
            {
                return _PolygonMode;
            }
            set
            {
                _PolygonMode = value;
            }
        }

        private double _FirstPointX;
        protected double FirstPointX
        {
            get
            {
                return _FirstPointX;
            }
            set
            {
                _FirstPointX = value;
            }
        }

        private double _FirstPointY;
        protected double FirstPointY
        {
            get
            {
                return _FirstPointY;
            }
            set
            {
                _FirstPointY = value;
            }
        }

        private double _Angle;
        protected double Angle
        {
            get
            {
                return _Angle;
            }
            set
            {
                _Angle = value;
            }
        }

        // Start of angle
        private double _StartAngle;
        protected double StartAngle
        {
            get
            {
                return _StartAngle;
            }
            set
            {
                _StartAngle = value;
            }
        }

        // End of angle
        private double _EndAngle;
        protected double EndAngle
        {
            get
            {
                return _EndAngle;
            }
            set
            {
                _EndAngle = value;
            }
        }

        // Program scale
        private double _Factor;
        public double Factor
        {
            get
            {
                return _Factor;
            }
            set
            {
                _Factor = value;
            }
        }

        /// <summary>
        /// Default constructor. Sets properties to default value.
        /// </summary>
        public HPGLFunctions()
            : base()
        {
            Factor = 100d;
            Output = Module_Main.outp;
            TextHndlr = new HPGLTextHandler();
        }

        public HPGLFunctions(Class_Output Output)
            : base(Output)
        {
            Factor = 100d;
            TextHndlr = new HPGLTextHandler();
        }

        public HPGLFunctions(double Factor)
            : base()
        {
            this.Factor = Factor;
            Output = Module_Main.outp;
            TextHndlr = new HPGLTextHandler();
        }

        public HPGLFunctions(Class_Output Output, double Factor)
            : base (Output)
        {
            this.Factor = Factor;
            TextHndlr = new HPGLTextHandler();
        }

        public override bool Line(string cmdLine)
        {
            try
            {
                // Get the first command it finds
                string fCmd = TextHndlr.CutCommand(cmdLine);
                //cmdLine = TextHndlr.NextCommand(cmdLine, fCmd);
                // Create a string array of coordinates
                string[] coords = { };

                //PenDown = (fCmd == "PD" && fCmd != "PU") ? true : false;

                if (cmdLine.Contains("AA"))
                    Arc(cmdLine);
                
                // Look if the command is absolute
                if (fCmd == "PA")
                    Absolute(cmdLine);
                // Look if the command is relative
                else if (fCmd == "PR")
                    Relative(cmdLine);

                // Look if the commandline contains any coords
                if (cmdLine.Contains(','))
                    // Fill string array with coordinates
                    coords = SplitLine(cmdLine);
                else
                    return false;

                // Define X coord
                X = double.Parse(coords[0]);
                // Define Y coord
                Y = double.Parse(coords[1]);
                // Scale new by factor
                NewX = X / Factor;
                NewY = Y / Factor;

                // Write if current penstate is down
                if (PenDown)
                    Output.OutputLine(LastX, LastY, NewX, NewY, Color);

                // Change old coords to new coords
                LastX = NewX;
                LastY = NewY;

                if (coords.Length > 2)
                {
                    string temp = fCmd;

                    for (int i = 2; i < coords.Length; i++)
                    {
                        temp += coords[i] + ",";
                    }

                    Line(temp);
                }
            }
            catch (Exception e)
            {
                
            }

            return true;
        }

        public override bool Arc(string cmdLine)
        {
            // Get the first command it finds
            string fCmd = TextHndlr.CutCommand(cmdLine);
            //cmdLine = TextHndlr.NextCommand(cmdLine, fCmd);

            if (fCmd == "AA")
                Absolute(cmdLine);
            else if (fCmd == "AR")
                Relative(cmdLine);

            // Get coordinates and angle from line
            string[] coords = SplitLine(cmdLine);
            // Define I coord
            I = Math.Round(double.Parse(coords[0]) / Factor, 3);
            // Define J coord
            J = Math.Round(double.Parse(coords[1]) / Factor, 3);
            // Define angle
            Angle = Math.Round(double.Parse(coords[2]), 3);
            
            // 360 Angle is a circle
            if (Angle == 360)
            {
                // Draw circle
                Output.OutputCircle(I, J, Radius, Color);
            }
            else
            {
                if (DiX == 0)
                {
                    // Calcute end of angle
                    EndAngle = Angle * Module_Main.PI / 180 + (Module_Main.PI / 2) * Math.Sign(DiY);
                }
                else
                {
                    if (DiY != 0)
                    {
                        if (DiX < 0)
                        {
                            // Calculate start of angle
                            StartAngle = (DiY < 0) ? Math.Atan(DiY / DiX) + Module_Main.PI : Module_Main.PI - Math.Atan(DiY / (-DiX));
                        }
                        else
                        {
                            // Calculate start of angle
                            StartAngle = Math.Atan(DiY / DiX);
                        }

                        // Calculate end of angle
                        EndAngle = Angle * Module_Main.PI / 180 + StartAngle;
                    }
                    else
                    {
                        // Calculate end of engle
                        EndAngle = Angle * Module_Main.PI / 180 + ((DiX > 0) ? 0 : Module_Main.PI);
                    }
                }

                // Calculate new X and Y coordinates
                NewX = Radius * Math.Cos(EndAngle) + I;
                NewY = Radius * Math.Sin(EndAngle) + J;

                // Clockwise or counterclockwise
                if (Angle < 0)
                {
                    Radius = -Radius;
                }
                
                if (PenDown)
                {
                    Output.OutputArc(LastX, LastY, NewX, NewY, I, J, Radius, Color);
                }

                LastX = NewX;
                LastY = NewY;
            }

            return true;
        }

        public override bool Relative(string cmdLine)
        {
            return base.Relative(cmdLine);
        }

        public override bool Absolute(string cmdLine)
        {
            return base.Absolute(cmdLine);
        }

        public override bool On(string cmdLine)
        {
            base.On(cmdLine);

            if (cmdLine.Contains(',') && !cmdLine.Contains("AA") && !cmdLine.Contains("AR"))
                Line(cmdLine);

            return true;
        }

        public override bool Off(string cmdLine)
        {
            base.Off(cmdLine);
            
            if (cmdLine.Contains(',') && !cmdLine.Contains("AA") && !cmdLine.Contains("AR"))
                Line(cmdLine);

            return true;
        }

        public override bool Tool(string cmdLine)
        {
            return base.Tool(cmdLine);
        }

        public override bool CallSubmain(string cmdLine)
        {
            return base.CallSubmain(cmdLine);
        }

        public override bool OutputSub(string cmdLine)
        {
            return base.OutputSub(cmdLine);
        }

        public override bool Rotary(string cmdLine)
        {
            return base.Rotary(cmdLine);
        }

        public bool Polygon(string cmdLine)
        {
            bool result = false;

            string[] temp = cmdLine.Split(new char[] { ',' });

            switch (temp[0])
            {
                case "":
                    break;
                case "0":
                    PolygonMode = true;
                    FirstPointX = LastX;
                    FirstPointY = LastY;
                    break;
                case "1":
                    Output.OutputLine(LastX, LastY, FirstPointX, FirstPointY, Color);
                    break;
                case "2":
                    Output.OutputLine(LastX, LastY, FirstPointX, FirstPointY, Color);
                    PolygonMode = false;
                    break;
            }

            return result;
        }

        public bool Circle(string cmdLine)
        {
            string fCmd = TextHndlr.CutCommand(cmdLine);
            string[] coords = SplitLine(cmdLine);

            if (PenDown)
                Output.OutputCircle(LastX, LastY, Radius, Color);

            return false;
        }

        private string[] SplitLine(string cmdLine)
        {
            string cmd = TextHndlr.CutCommand(cmdLine);
            string[] result = { };
            string[] temp = { };

            temp = cmdLine.Split(new char[] { ';', ',' });
            result = (string[])temp.Clone();

            for (int i = 0; i < result.Length; i++)
            {
                for (int n = 0; n < result[i].Length; n++)
                {
                    if (TextHndlr.IsLetter(result[i][n].ToString()))
                    {
                        temp[i] = result[i].Substring(n + 1);
                    }
                    else if (TextHndlr.IsNumber(result[i][n].ToString()))
                    {
                        continue;
                    }
                    else
                    {
                        if (result[i][n] != '.' && result[i][n] != '-')
                            temp[i] = result[i].Substring(n + 1);
                    }
                }

                if (temp[i] == "")
                {
                    Array.Clear(temp, i, 1);
                }
            }

            result = (string[])temp.Clone();
            temp = new string[] { };

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != null)
                {
                    Array.Resize(ref temp, temp.Count() + 1);
                    temp[temp.Count() - 1] = result[i];
                }
            }

            result = temp;

            return result;
        }
    }
}
