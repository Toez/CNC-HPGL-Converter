using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;

namespace ConvertCNCHPGL
{
    class Module_Main
    {
        public const double PI = 3.14159265358979d;
        // Dictionary contains CNC or HPGL conversion types
        public static Dictionary<string, string[]> types = new Dictionary<string, string[]>();
        public static bool RelativeArc = false;
        public static Class_Output outp = new Class_Output();

        public static void StartConversion(string sInputFilename, string sOutputFilename, double aScaleFactor)
        {
            try
            {
                Line l = new Line(new Coordinate(3, 5), new Coordinate(20, 15));


                int nPageNum = 0;
                int i = 0;
                Class_Program prg = new Class_Program();
                
                prg.ReadFile(sInputFilename);
                
                if (!IsHPGL(sInputFilename))
                {
                    Conversion con = new Conversion(sOutputFilename, prg, outp, new CNCFunctions());

                    if (con.Start())
                        MessageBox.Show("File succesfully converted.\nSaved file as " + sOutputFilename + "\nLogfile saved as:" + sOutputFilename.Substring(0, sOutputFilename.Length - 1) + "txt\nDate: " + DateTime.UtcNow.ToLocalTime().ToString(), "(C) BCSI Systems BV CNC and HPGL Converter");
                }
                else
                {
                    Conversion con = new Conversion(sOutputFilename, prg, outp, new HPGLFunctions(aScaleFactor));

                    if (con.Start())
                        MessageBox.Show("File succesfully converted.\nSaved file as " + sOutputFilename + "\nLogfile saved as:" + sOutputFilename.Substring(0, sOutputFilename.Length - 1) + "txt\nDate: " + DateTime.UtcNow.ToLocalTime().ToString(), "(C) BCSI Systems BV CNC and HPGL Converter");
                }
            }
            catch (System.Exception excep)
            {
                MessageBox.Show(excep.Message);
            }
        }

        public static bool IsHPGL(string aFilename)
        {
            bool result = false;
            StreamReader txt = null;
            string s = String.Empty;
            int i1 = 0;
            int i2 = 0;

            if (File.Exists(aFilename))
            {
                txt = File.OpenText(aFilename);
                s = txt.ReadToEnd();
                txt.Close();
                string tempRefParam = ";";
                i1 = CountOccurence(ref s, ref tempRefParam);
                string tempRefParam2 = "G0";
                i2 = CountOccurence(ref s, ref tempRefParam2);
                if (i1 == i2)
                {
                    string tempRefParam3 = "PD";
                    i1 = CountOccurence(ref s, ref tempRefParam3);
                    result = i1 > i2;
                }
                else
                {
                    result = i1 > i2;
                }
            }
            return result;
        }

        /// <summary>
        /// Counts how many times a string contains a certain character
        /// </summary>
        /// <param name="aString">String you want to count in</param>
        /// <param name="aChar">Character you want to count</param>
        /// <returns>Times "aChar" occurs in "aString"</returns>
        public static int CountOccurence(ref string aString, ref string aChar)
		{
            return new Regex(Regex.Escape(aChar)).Matches(aString).Count;
		}

        public static bool FileAlreadyExists(string sOutputFilename)
        {
            DialogResult dia = MessageBox.Show("The file " + sOutputFilename + " already exists. Click OK to overwrite this file or CANCEL to exit the program.", "", MessageBoxButtons.OKCancel);

            if (dia == DialogResult.OK)
                return true;
            else if (dia == DialogResult.Cancel)
                return false;
            else
                return false;
        }

        /// <summary>
        /// Creates a new dictionary from the xml file "Settings.xml"
        /// </summary>
        private static void ConstructDictionary(string itemsname)
        {
            // Create new instance to handle Xml
            Module_XML xml = new Module_XML();
            // Return CNCitems from default file "Settings.xml"
            XmlNode items = xml.ReturnXml("Settings.xml", itemsname);

            // Construct dictionary from childnodes
            foreach (XmlNode item in items)
            {
                // Read inner text of array
                string name = item.InnerText.Substring(0, item.InnerText.IndexOf('{'));
                // Seperate each element with ','
                string seperator = ",";
                string node = item.InnerText.Trim();
                // Count how many commands the inner text contains
                int count = CountOccurence(ref node, ref seperator);
                // Create new string array for commands
                string[] ext = new string[count + 1];
                // Add first item to the array because it starts with a '{'
                ext[0] = node.Substring(node.IndexOf('{') + 1, node.IndexOf(',') - node.IndexOf('{') - 1).Trim();

                // Add the other items to the array
                for (int i = 1; i < count; i++)
                {
                    // Cut node to next seperator
                    node = node.Substring(node.IndexOf(seperator) + 1).Trim();
                    // Add node to array
                    ext[i] = node.Substring(0, node.IndexOf(seperator));
                }

                // Add the last item to the array because it ends with a '}'
                ext[count] = node.Substring(node.IndexOf(seperator) + 1, node.IndexOf('}') - node.IndexOf(seperator) - 1).Trim();
                // Add array to dictionary
                types.Add(name, ext);
            }
        }
    }
}
