using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class LogFileOutput
    {
        public StreamWriter log;

        public string OutputFileName;

        public LogFileOutput(string outputFileName)
        {
            this.OutputFileName = outputFileName;

            if (File.Exists(OutputFileName))
            {
                Module_Main.FileAlreadyExists(OutputFileName);
                File.Delete(OutputFileName);
            }

            log = new StreamWriter(OutputFileName);
        }

        public void WriteStart()
        {
            //log.WriteLine(OutputFileName);
            //log.WriteLine(DateTime.Now);
            //log.WriteLine("");
        }

        public void WriteLine(string msg)
        {
            log.WriteLine(msg);
        }

        public bool SaveFile()
        {
            log.Close();

            return true;
        }
    }
}
