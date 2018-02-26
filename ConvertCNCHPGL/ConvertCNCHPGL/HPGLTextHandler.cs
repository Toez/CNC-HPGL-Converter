using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class HPGLTextHandler : TextHandler
    {
        public HPGLTextHandler()
            : base()
        {
            
        }

        public override string CutCommand(string line)
        {
            string result = "";
            int fLetterIndex = 0;
            bool fFound = false;
            int temp = 0;

            for (int i = 0; i < line.Length; i++)
            {
                int x = 0;

                if (!int.TryParse(line[i].ToString(), out x))
                {
                    if (!fFound)
                    {
                        fLetterIndex = i;
                        fFound = true;
                    }
                    else
                    {
                        temp = i;
                        break;
                    }
                }
            }

            result = line.Substring(fLetterIndex, temp - fLetterIndex + 1);

            return result;
        }
    }
}
