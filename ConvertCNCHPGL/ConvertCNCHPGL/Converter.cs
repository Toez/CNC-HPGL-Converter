using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class Converter
    {
        private string[] _Lines;
        public string[] Lines
        {
            get
            {
                return _Lines;
            }
            set
            {
                _Lines = value;
            }
        }

        private int _Index;
        public int Index
        {
            get
            {
                return _Index;
            }
            private set
            {
                _Index = value;
            }
        }

        private Coordinate _OldOrigen;
        public Coordinate OldOrigen
        {
            get
            {
                return _OldOrigen;
            }
            set
            {
                _OldOrigen = value;
            }
        }

        private Coordinate _OldArcCentre;
        public Coordinate OldArcCentre
        {
            get
            {
                return _OldArcCentre;
            }
            set
            {
                _OldArcCentre = value;
            }
        }

        private int _Color;
        public int Color
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

        private bool _PenDown;
        public bool PenDown
        {
            get
            {
                return _PenDown;
            }
            private set
            {
                _PenDown = value;
            }
        }

        private bool _RelativeMode;
        public bool RelativeMode
        {
            get
            {
                return _RelativeMode;
            }
            private set
            {
                _RelativeMode = value;
            }
        }

        private int _CurrentSubIndex;
        public int CurrentSubIndex
        {
            get
            {
                return _CurrentSubIndex;
            }
            set
            {
                _CurrentSubIndex = value;
            }
        }

        private double _Depth;
        public double Depth
        {
            get
            {
                return _Depth;
            }
            set
            {
                _Depth = value;
            }
        }

        private int? _SubCounter;
        public int? SubCounter
        {
            get
            {
                return _SubCounter ?? 1;
            }
            set
            {
                _SubCounter = value;
            }
        }

        private double _TotalDistanceBridge;
        public double TotalDistanceBridge
        {
            get
            {
                return _TotalDistanceBridge;
            }
            set
            {
                _TotalDistanceBridge = value;
            }
        }

        private double _TotalDistanceDrawn;
        public double TotalDistanceDrawn
        {
            get
            {
                return _TotalDistanceDrawn;
            }
            set
            {
                _TotalDistanceDrawn = value;
            }
        }

        private double _Scaling;
        public double Scaling
        {
            get
            {
                return _Scaling;
            }
            set
            {
                _Scaling = value;
            }
        }

        private List<SubProgram> Programs;
        private ObjectHandler ObjHndlr;
        private SubProgram CurrentProgram;

        private SubProgram MainProgram;
        private bool MainFound;
        private bool InMain;
        private bool MainNotClosed;
        private bool SubCalled;

        public LogFileOutput LfOut;

        public List<SubProgram> GetProgram
        {
            get
            {
                return Programs;
            }
        }

        public Converter()
            : this(new string[]{}, 1)
        {

        }

        public Converter(string[] lines, double scaling)
        {
            Lines = lines;
            ObjHndlr = new ObjectHandler(lines);
            OldOrigen = new Coordinate(0, 0);
            OldArcCentre = new Coordinate(0, 0);
            Programs = new List<SubProgram>();
            MainFound = false;
            PenDown = false;
            InMain = false;
            RelativeMode = false;
            MainNotClosed = false;
            SubCalled = false;
            Scaling = scaling;
        }

        public FileObject CreateProgram()
        {
            FileObject result = new FileObject();

            try
            {
                while (Index < Lines.Length)
                {
                    CommandRef cmd = ObjHndlr.DetectCommand(Lines[Index]);

                    if (!MainFound)
                    {
                        if (cmd.Command == Commands.SUBPROG)
                        {
                            MainProgram = (SubProgram)HandleCommand(cmd);
                            MainFound = true;
                            MainNotClosed = true;
                            Index++;
                            continue;
                        }
                        else if (!Statements.IsNull(cmd.Command))
                        {
                            MainProgram = new SubProgram("MAIN", new Coordinate(), 0, new SectionOptions(SubPrograms.BOX, true, true, false));
                            MainFound = true;
                            MainNotClosed = true;
                            Index++;
                            continue;
                        }
                        else if (Statements.IsNull(cmd.Command) && ObjHndlr.LastSubIndex < 1)
                        {
                            CommandRef command = new CommandRef(Commands.SUBPROG, "DFS,P", "DFS,PMAIN");
                            MainProgram = (SubProgram)HandleCommand(command);
                            MainFound = true;
                        }
                    }
                    
                    if (Statements.IsNull(cmd))
                    {
                        Index++;
                        continue;
                    }
                    else if (MainFound)
                    {
                        FileObject obj = HandleCommand(cmd);

                        if ((obj.GetType() == typeof(Line) || obj.GetType() == typeof(Arc)) && MainNotClosed && SubCalled)
                        {
                            FileObject esub = HandleCommand(new CommandRef(Commands.SUBEND, "CLOSEDBYPROGRAM", "CLOSEDBYPROGRAM"));
                            FileObject nsub = HandleCommand(new CommandRef(Commands.SUBPROG, "DFS,PC", "DFS,PC" + SubCounter));
                            SubCounter++;
                            SubCalled = false;
                        }
                        if (obj.GetType().IsSubclassOf(typeof(FileCoordinateObject)) && !Statements.IsNull(CurrentProgram) && obj.GetType() != typeof(SubProgram))
                            CurrentProgram.AddContent((FileCoordinateObject)obj);
                    }
                    
                    if (ObjHndlr.LineContainsSecondCommand(cmd))
                    {
                        Lines[Index] = cmd.Line.Substring(cmd.Line.IndexOf(cmd.Reference) + cmd.Reference.Length);
                        
                        if (ObjHndlr.CompareLines(Lines[Index], cmd.Line))
                            Index++;
                        
                        continue;
                    }
                    
                    Index++;
                }
                
                if (Programs.Count <= 0)
                {
                    if (!Statements.IsNull(CurrentProgram))
                        Programs.Add(CurrentProgram);

                    CurrentProgram = null;
                }

                ErrorHandler.AddMessage("Total distance drawn: " + TotalDistanceDrawn + "\r\nTotal distance moved: " + TotalDistanceBridge);
                SortSubs();
                OneSubClause();
                result = MainProgram;
            }
            catch (Exception e)
            {
                if (!Statements.IsNull(Lines[Index]))
                    ErrorHandler.AddMessage(e, Lines[Index]);
                else
                    ErrorHandler.AddMessage(e);
            }

            return result;
        }

        private FileObject HandleCommand(CommandRef command)
        {
            FileObject result = new FileObject();

            try
            {
                command.Line = command.Line.Substring(command.Line.IndexOf(command.Reference));
                command.Line = command.Line.Replace(" ", "");
                
                switch (command.Command)
                {
                    case Commands.NONE:
                        break;
                    case Commands.LINE:
                        {
                            //PenDown = (command.Reference == "G0" || command.Reference == "G00") ? false : true;

                            string[] args = MakeArguments(command);
                            double[] coords = Array.ConvertAll(command.Line.Split(args, StringSplitOptions.RemoveEmptyEntries), double.Parse);
                            
                            for (int i = 0; i < coords.Length; i++)
                            {
                                coords[i] = coords[i] / Scaling;
                            }
                            
                            Coordinate origen = UseOldCoordinates(coords, args)[0];
                            
                            if (args.Contains("Z"))
                                Depth = coords[Array.IndexOf(args, "Z") - 1];

                            if (RelativeMode && !Statements.IsNull(origen))
                                origen += OldOrigen;

                            if (PenDown && !Statements.IsNull(origen))
                            {
                                result = new Line(OldOrigen, origen, Color, Depth);
                                TotalDistanceDrawn += AddDistance((Line)result);
                            }
                            else
                            {
                                result = new FileObject();
                                TotalDistanceBridge += AddDistance(new Line(OldOrigen, origen, Color, Depth));
                            }

                            if (origen != null)
                            {
                                OldOrigen = origen;
                                OldArcCentre = origen;
                            }

                            break;
                        }
                    case Commands.ARC:
                        {
                            string[] args = MakeArguments(command);
                            double[] coords = Array.ConvertAll(command.Line.Split(args, StringSplitOptions.RemoveEmptyEntries), double.Parse);

                            for (int i = 0; i < coords.Length; i++)
                            {
                                coords[i] = coords[i] / Scaling;
                            }

                            Coordinate[] coordinates = UseOldCoordinates(coords, args);
                            Coordinate origen = coordinates[0];
                            Coordinate distance = coordinates[1];
                            
                            if (RelativeMode)
                                origen += OldOrigen;
                            
                            if (Module_Main.RelativeArc)
                            {
                                if (!Statements.IsNull(distance) && !Statements.IsNull(OldArcCentre) && !Statements.IsNull(OldOrigen))
                                {
                                    if (distance.X == OldArcCentre.X && distance.Y != OldArcCentre.Y)
                                        distance.Y += OldOrigen.Y;
                                    else if (distance.X != OldArcCentre.X && distance.Y == OldArcCentre.Y)
                                        distance.X += OldOrigen.X;
                                    else
                                        distance += OldOrigen;
                                }
                            }
                            
                            if (PenDown)
                            {
                                if (command.Reference != "G02" && command.Reference != "G2")
                                    result = new Arc(OldOrigen, origen, distance, Color, false, Depth);
                                else
                                    result = new Arc(OldOrigen, origen, distance, Color, true, Depth);

                                TotalDistanceDrawn += AddDistance((Arc)result);
                            }
                            else
                                result = new FileObject();
                            
                            OldOrigen = origen;
                            OldArcCentre = distance;
                            break;
                        }
                    case Commands.SUBPROG:
                        {
                            if (CurrentProgram != null && CurrentProgram.MadeByProgram)
                                Programs.Add(CurrentProgram);

                            if (!Statements.IsNull(CurrentProgram))
                            {
                                if (!CurrentProgram.ContainsContent())
                                    break;
                            }

                            if (Programs.Contains(CurrentProgram))
                                break;

                            if (command.Reference.Contains("DFS,PC"))
                            {
                                result = new SubProgram("C" + SubCounter, new Coordinate(0, 0), 0, new SectionOptions(SubPrograms.BOX, true, true, true));
                                CurrentProgram = (SubProgram)result;
                                break;
                            }
                            
                            int[] indexs = ObjHndlr.GetSubIndexs(Lines, Index);
                            int size = indexs[1] - indexs[0];
                            int section = ObjHndlr.GetSection(command.Line);
                            string name = section.ToString();

                            if (section <= 0)
                                name = "MAIN";

                            SectionOptions options = new SectionOptions();
                            
                            if (section > ObjHndlr.LastSubIndex && MainFound)
                            {
                                result = new FileObject();
                                break;
                            }

                            if (!MainFound)
                                options = new SectionOptions(SubPrograms.BOX, false, false, false);
                            else if (ObjHndlr.IsLastSub(section) && section > 2)
                                options = new SectionOptions(SubPrograms.BOX, true, true, false);
                            else
                                options = new SectionOptions(SubPrograms.SECTION, false, false, false);
                            
                            result = new SubProgram(name, new Coordinate(0, 0), size, Color, options);
                            
                            if (!SubAlreadyExists((SubProgram)result))
                                CurrentProgram = (SubProgram)result;
                            else
                                result = new FileObject();

                            OldOrigen = new Coordinate(0, 0);
                            OldArcCentre = new Coordinate(0, 0);
                            break;
                        }
                    case Commands.SUBEND:
                        {
                            if (CurrentProgram.ContainsCoordinates() && CurrentProgram.Program == SubPrograms.BOX)
                            {
                                CurrentProgram.InnerSection = true;
                                CurrentProgram.InitializeNull = true;
                            }
                            
                            if (CurrentProgram != null)
                                Programs.Add(CurrentProgram);

                            if (command.Line != "CLOSEDBYPROGRAM")
                                MainNotClosed = false;

                            CurrentProgram = null;
                            break;
                        }
                    case Commands.SUBCALL:
                        {
                            int section = int.Parse(command.Line.Substring(command.Line.IndexOf(command.Reference) + command.Reference.Length));
                            
                            if (ObjHndlr.IsLastSub(section) || section > ObjHndlr.LastSubIndex)
                                result = new FileObject();
                            else
                                result = new SubCall(section, OldOrigen);

                            MainProgram.AddContent((SubCall)result);
                            SubCalled = true;
                            result = null;
                            break;
                        }
                    case Commands.ABSOLUTE:
                        {
                            RelativeMode = false;
                            break;
                        }
                    case Commands.RELATIVE:
                        {
                            RelativeMode = true;
                            break;
                        }
                    case Commands.TOOL:
                        {
                            int temp = 0;
                            
                            if (int.TryParse(command.Line.Substring(command.Line.IndexOf(command.Reference) + command.Reference.Length), out temp))
                                Color = temp;
                            else
                            {
                                for (int i = 0; i < command.Line.Length - command.Reference.Length; i++)
                                {
                                    string s = command.Line.Substring(command.Reference.Length, i);
                                    
                                    if (!int.TryParse(s, out temp))
                                    {
                                        if (i > 0)
                                        {
                                            if (int.TryParse(command.Line.Substring(command.Reference.Length, i - 1), out temp))
                                            {
                                                Color = int.Parse(command.Line.Substring(command.Reference.Length, i - 1));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            
                            break;
                        }
                    case Commands.ON:
                        {
                            PenDown = true;
                            break;
                        }
                    case Commands.OFF:
                        {
                            PenDown = false;
                            break;
                        }
                    default:
                        break;
                }

                if (result.GetType().IsSubclassOf(typeof(FileCoordinateObject)) && result.GetType() != typeof(SubProgram) && Statements.IsNull(CurrentProgram))
                    NoSubClause();
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e, Index + " | " + Lines[Index]);
                result = new FileObject();
            }

            return result;
        }

        private bool SubAlreadyExists(SubProgram sub)
        {
            bool result = false;

            try
            {
                for (int i = 0; i < Programs.Count; i++)
                {
                    if (sub.Name == Programs[i].Name)
                        result = true;
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        private void OneSubClause()
        {
            try
            {
                if (Programs.Count == 0)
                    Programs.Add(MainProgram);
                if (Programs.Count == 1)
                {
                    Programs[0].InitializeNull = true;
                    Programs[0].InnerSection = true;
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }
        }

        private void NoSubClause()
        {
            try
            {
                if (Programs.Count <= 0)
                {
                    int[] indexs = ObjHndlr.GetSubIndexs(Lines, Index);
                    int size = indexs[1] - indexs[0];
                    CurrentProgram = new SubProgram("MAIN", new Coordinate(0, 0), size, Color, new SectionOptions(SubPrograms.BOX, true, true, false));
                }
                else
                {
                    int[] indexs = ObjHndlr.GetSubIndexs(Lines, Index);
                    int size = indexs[1] - indexs[0];
                    CurrentProgram = new SubProgram(Programs.Count.ToString(), new Coordinate(0, 0), size, Color, new SectionOptions(SubPrograms.SECTION, false, false, false));
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }
        }

        private void SortSubs()
        {
            try
            {
                for (int i = 0; i < Programs.Count; i++)
                {
                    if (Programs[i].Name == "MAIN" || Programs[i].Name == "1" || Programs[i].Name == "0")
                    {
                        Programs[i] = MainProgram;
                        break;
                    }
                }
                
                List<SubProgram> temp = new List<SubProgram>();

                for (int i = 0; i < Programs.Count; i++)
                {
                    if (Programs[i].MadeByProgram)
                    {
                        SubProgram clone = Programs[i];
                        temp.Add(clone);
                        Programs.Remove(clone);
                        i--;
                    }
                }

                for (int i = 0; i < temp.Count; i++)
                {
                    Programs.Add(temp[i]);
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }
        }

        private double AddDistance(Line line)
        {
            double result = 0;

            try
            {
                result = Math.Sqrt(Math.Pow(line.Origen.X - line.Destination.X, 2) + Math.Pow(line.Origen.Y - line.Destination.Y, 2));

                if (result < 0)
                    result = -result;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e, "Line: " + Lines[Index]);
            }

            return result;
        }

        private double AddDistance(Arc arc)
        {
            double result = 0;

            try
            {
                result = arc.Length;

                if (result < 0)
                    result = -result;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e, "Line: " + Lines[Index]);
            }

            return result;
        }

        private Coordinate[] UseOldCoordinates(double[] coords, string[] args)
        {
            Coordinate[] result = new Coordinate[2];

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (ObjHndlr.GetCommandFromReference(args[i]) != Commands.NONE)
                    {
                        Array.Copy(args, i + 1, args, 0, args.Length - i - 1);
                        Array.Resize(ref args, args.Length - i - 1);
                        break;
                    }
                }

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "Z" || args[i] == "F" || args[i] == "G" || args[i] == "R")
                    {
                        if (i >= 1)
                            Array.Resize<string>(ref args, i);
                        break;
                    }
                }

                if (args.Length <= 0)
                    result = null;
                else if (args.Length == 1)
                {
                    if (args.Contains("X"))
                    {
                        result[0] = new Coordinate(coords[0], OldOrigen.Y);
                        result[1] = OldArcCentre;
                    }
                    else if (args.Contains("Y"))
                    {
                        result[0] = new Coordinate(OldOrigen.X, coords[0]);
                        result[1] = OldArcCentre;
                    }
                    else if (args.Contains("I"))
                    {
                        result[0] = OldOrigen;
                        result[1] = new Coordinate(coords[0], OldArcCentre.Y);
                    }
                    else if (args.Contains("J"))
                    {
                        result[0] = OldOrigen;
                        result[1] = new Coordinate(OldArcCentre.X, coords[0]);
                    }
                }
                else if (args.Length == 2)
                {
                    if (args.Contains("X") && args.Contains("Y"))
                    {
                        result[0] = new Coordinate(coords[0], coords[1]);
                        result[1] = OldArcCentre;
                    }
                    else if (args.Contains("X") && args.Contains("I"))
                    {
                        result[0] = new Coordinate(coords[0], OldOrigen.Y);
                        result[1] = new Coordinate(coords[1], OldArcCentre.Y);
                    }
                    else if (args.Contains("X") && args.Contains("J"))
                    {
                        result[0] = new Coordinate(coords[0], OldOrigen.Y);
                        result[1] = new Coordinate(OldArcCentre.X, coords[1]);
                    }
                    else if (args.Contains("Y") && args.Contains("I"))
                    {
                        result[0] = new Coordinate(OldOrigen.X, coords[0]);
                        result[1] = new Coordinate(coords[1], OldArcCentre.Y);
                    }
                    else if (args.Contains("Y") && args.Contains("J"))
                    {
                        result[0] = new Coordinate(OldOrigen.X, coords[0]);
                        result[1] = new Coordinate(OldArcCentre.X, coords[1]);
                    }
                    else if (args.Contains("I") && args.Contains("J"))
                    {
                        result[0] = OldOrigen;
                        result[1] = new Coordinate(coords[0], coords[1]);
                    }
                }
                else if (args.Length == 3)
                {
                    if (args.Contains("X") && args.Contains("Y") && args.Contains("I"))
                    {
                        result[0] = new Coordinate(coords[0], coords[1]);
                        result[1] = new Coordinate(coords[2], OldArcCentre.Y);
                    }
                    else if (args.Contains("X") && args.Contains("Y") && args.Contains("J"))
                    {
                        result[0] = new Coordinate(coords[0], coords[1]);
                        result[1] = new Coordinate(OldArcCentre.X, coords[2]);
                    }
                    else if (args.Contains("X") && args.Contains("I") && args.Contains("J"))
                    {
                        result[0] = new Coordinate(coords[0], OldOrigen.Y);
                        result[1] = new Coordinate(coords[1], coords[2]);
                    }
                    else if (args.Contains("Y") && args.Contains("I") && args.Contains("J"))
                    {
                        result[0] = new Coordinate(OldOrigen.X, coords[0]);
                        result[1] = new Coordinate(coords[1], coords[2]);
                    }
                }
                else if (args.Length == 4)
                {
                    if (args.Contains("X") && args.Contains("Y") && args.Contains("I") && args.Contains("J"))
                    {
                        result[0] = new Coordinate(coords[0], coords[1]);
                        result[1] = new Coordinate(coords[2], coords[3]);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        private string[] MakeArguments(CommandRef command)
        {
            string[] result = { };

            try
            {
                Array.Resize<string>(ref result, result.Length + 1);
                result[result.Length - 1] = command.Reference;

                if (command.Line.Contains("X"))
                {
                    Array.Resize<string>(ref result, result.Length + 1);
                    result[result.Length - 1] = "X";
                }
                if (command.Line.Contains("Y"))
                {
                    Array.Resize<string>(ref result, result.Length + 1);
                    result[result.Length - 1] = "Y";
                }
                if (command.Line.Contains("I"))
                {
                    Array.Resize<string>(ref result, result.Length + 1);
                    result[result.Length - 1] = "I";
                }
                if (command.Line.Contains("J"))
                {
                    Array.Resize<string>(ref result, result.Length + 1);
                    result[result.Length - 1] = "J";
                }
                if (command.Line.Contains("F"))
                {
                    Array.Resize<string>(ref result, result.Length + 1);
                    result[result.Length - 1] = "F";
                }
                if (command.Line.Contains("R"))
                {
                    Array.Resize<string>(ref result, result.Length + 1);
                    result[result.Length - 1] = "R";
                }
                if (command.Line.Contains("Z"))
                {
                    Array.Resize<string>(ref result, result.Length + 1);
                    result[result.Length - 1] = "Z";
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }
    }
}
