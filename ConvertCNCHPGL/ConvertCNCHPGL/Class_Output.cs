using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.IO;
using System.Windows.Forms;

namespace ConvertCNCHPGL
{
    class Class_Output
    {
        private int OutPutFileNumber = 0;
        public int CNTR = 1;
        private StreamWriter sw;

        public Class_Output()
        {

        }

        public void CloseFile()
        {
            OutPutFileNumber = 0;
            CNTR = 1;
            sw.Close();
        }

        public bool OpenFile(string sOutputFilename)
        {
            string folderPath = sOutputFilename.Substring(0, sOutputFilename.LastIndexOf("\\"));

            if (Directory.Exists(folderPath))
            {
                if (File.Exists(sOutputFilename))
                {
                    bool go = Module_Main.FileAlreadyExists(sOutputFilename);
                    
                    if (go)
                        File.Delete(sOutputFilename);
                    else
                        return false;
                }
                
                sw = new StreamWriter(sOutputFilename, true);
            }

            return true;
        }

        public void OutputLine(double fLastX, double fLastY, double fFirstPointX, double fFirstPointY, int nColor)
        {
            PrintLine("L|" +
                      CNTR.ToString().Trim() + "|" + ConvDouble(fLastX) +
                      "|" + ConvDouble(fLastY) + "|" + ConvDouble(fFirstPointX) +
                      "|" + ConvDouble(fFirstPointY) + "|0|0|0|" +
                      nColor.ToString().Trim() + "|0|0||");
            CNTR++;
        }

        public void OutputLine(double fLastX, double fLastY, double fFirstPointX, double fFirstPointY, int nColor, double depth)
        {
            PrintLine("L|" +
                      CNTR.ToString().Trim() + "|" + ConvDouble(fLastX) +
                      "|" + ConvDouble(fLastY) + "|" + ConvDouble(fFirstPointX) +
                      "|" + ConvDouble(fFirstPointY) + "|0|0|0|" +
                      nColor.ToString().Trim() + "|0|0|0|0||0|0||" + depth + "|");
            CNTR++;
        }

        public void OutputCircle(double fI, double fJ, double radius, int nColor)
        {
            PrintLine("C|" + CNTR.ToString().Trim() +
                      "|" + ConvDouble(fI) + "|" +
                      ConvDouble(fJ) + "|" +
                      ConvDouble(radius) + "|0|0|0|" +
                      nColor.ToString().Trim() + "|0|0||");
            CNTR++;
        }

        public void OutputArc(double fLastX, double fLastY, double fNewX, double fNewY, double fI, double fJ, double radius, int nColor)
        {
            PrintLine("A|" + CNTR.ToString().Trim() +
                      "|" + ConvDouble(fLastX) + "|" +
                      ConvDouble(fLastY) + "|" +
                      ConvDouble(fNewX) + "|" +
                      ConvDouble(fNewY) + "|" +
                      ConvDouble(fI) + "|" +
                      ConvDouble(fJ) + "|" +
                      ConvDouble(radius) + "|0|0|0|" +
                      nColor.ToString().Trim() + "|0|0||");
            CNTR++;
        }

        public void OutputArc(double fLastX, double fLastY, double fNewX, double fNewY, double fI, double fJ, double radius, int nColor, double depth)
        {
            PrintLine("A|" + CNTR.ToString().Trim() +
                      "|" + ConvDouble(fLastX) + "|" +
                      ConvDouble(fLastY) + "|" +
                      ConvDouble(fNewX) + "|" +
                      ConvDouble(fNewY) + "|" +
                      ConvDouble(fI) + "|" +
                      ConvDouble(fJ) + "|" +
                      ConvDouble(radius) + "|0|0|0|" +
                      nColor.ToString().Trim() + "|0|0|0|0||0|0||" + depth + "|");
            CNTR++;
        }

        public void OutputFileHeader(string sOutputFilename)
        {
            PrintLine("F|" + sOutputFilename + "|0|0|" + Environment.NewLine + "U|0|" + Environment.NewLine + "W|0|0|1|1|");
        }

        public void OutputBoxStart(string aName)
        {
            PrintLine("B|" + aName + "||0|0|1|1||||");
            CNTR = 1;
        }

        public void OutputBoxEnd()
        {
            PrintLine("b|");
        }

        public void OutputSectionStart(string aName, string section)
        {
            PrintLine("S|" + section.ToString() + "|" + aName + "|0|0|1|1|");
            CNTR = 1;
        }

        public void OutputInstance(int nNbInstance, double fLastX, double fLastY)
        {
            PrintLine("I|" + nNbInstance.ToString() + "|" +
                      ConvDouble(fLastX) + "|" + ConvDouble(fLastY) +
                      "|0|1|1|0||");
        }

        public void OutputNullInstance()
        {
            PrintLine("I|1|0|0|0|1|1|0||");
        }

        public void OutputSectionEnd()
        {
            PrintLine("s|");
        }

        public void OutputFileEnd()
        {
            PrintLine("q|");
        }

        private void PrintLine(string a)
        {
            sw.WriteLine(a);
        }

        private string ConvDouble(double a)
        {
            double x = Math.Round((double)(a * 1000), 0) / 1000;
            return x.ToString();
        }
    }
}
