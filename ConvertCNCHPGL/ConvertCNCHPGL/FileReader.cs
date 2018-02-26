using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class FileReader
    {
        private string _FileName;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value;
            }
        }

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

        public FileReader(string filename)
        {
            FileName = filename;
            Lines = Read();
        }

        private string[] Read()
        {
            string[] result = File.ReadAllLines(FileName);

            return result;
        }
    }
}
