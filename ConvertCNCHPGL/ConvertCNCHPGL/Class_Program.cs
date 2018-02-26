using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertCNCHPGL
{
    class Class_Program
    {
        private string[] gProgram = null;
        private string[] mSubprogram = new string[65];

        private int lgFileSize = 0;
        private int ngFileNum = 0;

        private int gNbSubMain = 0;
        private bool mSubsCounted = false;

        public int gNbProgramLines = 0;
        public int CurrentLineIndex = 0;
        public int LinesConverted = 0;

        public int submains = 0;
        public Dictionary<int, int> submainI;

        public void IncLineConverted()
        {
            LinesConverted++;
        }

        public string GetSubprogram(int aIndex)
        {
            try
            {
                return mSubprogram[aIndex];
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            return System.String.Empty;
        }

        public string GetLine(int aIndex)
        {
            try
            {
                return gProgram[aIndex].Trim();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            return System.String.Empty;
        }

        public string CurrentLine
        {
            get
            {
                try
                {
                    return gProgram[CurrentLineIndex].Trim();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
                return System.String.Empty;
            }
        }

        public int SubCount
        {
            get
            {
                CountSubMain();
                return gNbSubMain;
            }
        }

        //--------------------------------------------------------------------------------------------------
        // Procedure ReadFile
        //
        //   Functie     : read a file and store it in the global array 'gProgram'
        //
        //   Input       : Geen
        //
        //   Output      : Geen
        //--------------------------------------------------------------------------------------------------
        public void ReadFile(string sInputFilename)
        {

            try
            {
                StreamReader sr = new StreamReader(sInputFilename);
                ngFileNum = File.ReadAllLines(sInputFilename).Length;
                gProgram = new string[ngFileNum];
                string allLines = File.ReadAllText(sInputFilename);
                int subcounter = 0;
                
                int nFirstEnter = 0;
                int nLine = 0;
                string sInputLine = String.Empty;

                gNbProgramLines = 0;
                LinesConverted = 0;


                //-- Open and read selected file ------------------------------------------------------------------------------------

                for (int i = 0; i < ngFileNum; i++)
                {
                    gProgram[i] = sr.ReadLine();
                }

                //-- Set submain indexes -------------------------------------------------------------------------------------------
                if (!allLines.Contains(';'))
                {
                    submains = CountSubs(allLines);
                    submainI = new Dictionary<int,int>();
                    
                    for (int i = 0; i < submains - 1; i++)
                    {
                        int index = CurrentLineIndex;
                        for (int n = 0; n < ngFileNum; n++)
                        {
                            CurrentLineIndex++;
                            if (CurrentLine.Contains("%") || CurrentLine.Contains("$PROGIN") || CurrentLine.Contains("DFS,P"))
                            {
                                break;
                            }
                        }
                        
                        submainI.Add(index, CurrentLineIndex); 
                    }
                }
                
                gNbProgramLines = ngFileNum - 1;
                CurrentLineIndex = 0;
                sr.Close();
            }
            catch (System.Exception excep)
            {
                MessageBox.Show(excep.Message);
            }
            
        }

        //--------------------------------------------------------------------------------------------------
        //Procedure CountSubMain
        //
        //   Functie     : count the number of subroutines in the program
        //
        //   Input       : none
        //
        //   Output      : none
        //--------------------------------------------------------------------------------------------------
        private void CountSubMain()
        {
            if (mSubsCounted)
            {
                return;
            }
            mSubsCounted = true;

            gNbSubMain = 0;
            string Subprog = String.Empty;

            for (int nLine = 1; nLine < gNbProgramLines; nLine++)
            {
                Subprog = DetectStartSubMain(ref gProgram[nLine]);
                if (Subprog.Length > 0)
                {
                    mSubprogram[gNbSubMain] = Subprog;
                    gNbSubMain++;
                }
            }
        }

        //Procedure DetectStartSubMain
        //
        //   Functie     : Detect a start of a submain in the current program line
        //
        //   Input       : the present line
        //
        //   Output      : =0 if none atherwise the number  of the submain
        //--------------------------------------------------------------------------------------------------
        public string DetectStartSubMain(ref string sLine)
        {

            string result = String.Empty;
            object PType = null;
            string stemp = String.Empty;
            int pos = 0;
            bool found = false;

            result = "";

            if (sLine != "")
            {
                // look for "IBH Controller PXXX"
                if (sLine.Substring(1) == "P")
                {
                    stemp = sLine.Substring(sLine.Length - 1, sLine.Length);
                    result = stemp.Trim();
                    // look for "OX ..."
                }
                else if (sLine.Substring(1) == "O")
                {
                    // with this machine laser must be explicitely actived
                    stemp = sLine.Substring(sLine.Length - 1);
                    pos = (stemp.IndexOf('(') + 1);
                    if (pos != 0)
                    {
                        stemp = stemp.Substring(stemp.IndexOf('('));
                    }
                    return stemp.Trim();
                    // look for "(DFS, PXX, ...) CC220 with DFS Blocks"
                }
                else if (sLine.IndexOf("DFS") >= 0)
                {
                    stemp = sLine.Substring(sLine.LastIndexOf(',') - 1, 1);
                    if (stemp.Contains('.'))
                        stemp = sLine.Substring(stemp.IndexOf('.'));
                    result = stemp.Trim();

                    // look for "$PROGINXX"
                }
                else if (sLine.IndexOf("$PROGIN") >= 0)
                {
                    stemp = sLine.Substring(sLine.Length - (sLine.IndexOf("$PROGIN") + 1) - 6);
                    result = stemp.Trim();
                }
                else if (sLine.IndexOf("%") >= 0)
                {
                    stemp = sLine.Substring(sLine.IndexOf("%") + 1);
                    result = stemp.Trim();
                }
            }
            
            return result; 
        }

        int CountSubs(string filelines)
        {
            string aChar = "";
            if (filelines.Contains("$PROGIN"))
                aChar = "$PROGIN";
            else if (filelines.Contains("%"))
                aChar = "%";
            else if (filelines.Contains("DFS,P"))
                aChar = "DFS,P";

            if (aChar != "" && aChar != null)
                return new Regex(Regex.Escape(aChar)).Matches(filelines).Count;
            else
                return 0;
        }
    }
}