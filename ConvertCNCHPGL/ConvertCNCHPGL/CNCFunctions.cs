using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class CNCFunctions : BaseFunctions
    {
        public CNCFunctions()
            : base()
        {

        }

        public CNCFunctions(Class_Output Output)
            : base(Output)
        {
            this.Output = Output;
        }

        public override void ReplaceAxis(ref string cmdLine)
        {
            base.ReplaceAxis(ref cmdLine);
        }

        public override bool Line(string cmdLine)
        {
            return base.Line(cmdLine);
        }

        public override bool Arc(string cmdLine)
        {
            return base.Arc(cmdLine);
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
            return base.On(cmdLine);
        }

        public override bool Off(string cmdLine)
        {
            return base.Off(cmdLine);
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

        public int DetectSub(string cmdLine)
        {
            string result = "";
            string[] subRefs = { "P", "O", "DFS", "$PROGIN", "%" };
            string thisSubRef = "";
            bool subFound = false;

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

            switch (thisSubRef)
            {
                case "P":
                    result = cmdLine.Substring(cmdLine.IndexOf(thisSubRef) + thisSubRef.Length);
                    break;
                case "O":
                    result = cmdLine.Substring(cmdLine.IndexOf(thisSubRef) + thisSubRef.Length);
                    break;
                case "DFS":
                    result = cmdLine.Substring(cmdLine.IndexOf("P") + 1, 1);
                    break;
                case "$PROGIN":
                    result = cmdLine.Substring(cmdLine.IndexOf(thisSubRef) + thisSubRef.Length);
                    break;
                case "%":
                    result = cmdLine.Substring(cmdLine.IndexOf(thisSubRef) + thisSubRef.Length);
                    break;
                default:
                    break;
            }

            CurrentSub = result;

            return int.Parse(result);
        }
    }
}
