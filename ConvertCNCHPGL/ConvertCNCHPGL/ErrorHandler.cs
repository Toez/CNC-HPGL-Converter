using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    static class ErrorHandler
    {
        private static string _FileName;
        /// <summary>
        /// Path to logfile
        /// </summary>
        public static string FileName
        {
            get
            {
                // Use default folder for logfile
                return _FileName ?? (_FileName = @".\Logfile.txt");
            }
            set
            {
                _FileName = value;
            }
        }

        private static string[] _Messages;
        /// <summary>
        /// Array of messages received from errors thrown
        /// </summary>
        public static string[] Messages
        {
            get
            {
                // Use current date and time as first line of logfile
                return _Messages ?? (_Messages = new string[] { FileName, DateTime.Now.ToString(), "\r\n" });
            }
            private set
            {
                _Messages = value;
            }
        }

        /// <summary>
        /// Adds a new message to errorlist
        /// </summary>
        /// <param name="e">Exception with message and stack trace</param>
        public static void AddMessage(Exception e)
        {
            try
            {
                // Construct message for the array
                string message = "Message: " + e.Message + Environment.NewLine + "Stack Trace: " + e.StackTrace;
                // Clone array
                string[] temp = (string[])Messages.Clone();
                // Make it bigger
                Array.Resize(ref temp, Messages.Length + 1);
                // Write last entry
                temp[temp.Length - 1] = message;
                // Clone again
                Messages = (string[])temp.Clone();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Adds a new message to errorlist
        /// </summary>
        /// <param name="e">Exception with message and stack trace</param>
        /// <param name="line">Line in convert file</param>
        public static void AddMessage(Exception e, string line)
        {
            try
            {
                // Construct message for the array
                string message = "Message: " + e.Message + Environment.NewLine + "Stack Trace: " + e.StackTrace + Environment.NewLine + "Line: " + line;
                // Clone array
                string[] temp = (string[])Messages.Clone();
                // Make it bigger
                Array.Resize(ref temp, Messages.Length + 1);
                // Write last entry
                temp[temp.Length - 1] = message;
                // Clone again
                Messages = (string[])temp.Clone();
            }
            catch (Exception)
            {
                
            }
        }

        public static void AddMessage(string message)
        {
            try
            {
                // Clone array
                string[] temp = (string[])Messages.Clone();
                // Make it bigger
                Array.Resize(ref temp, Messages.Length + 1);
                // Write last entry
                temp[temp.Length - 1] = message;
                // Clone again
                Messages = (string[])temp.Clone();
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Writes and saves the logfile
        /// </summary>
        public static void WriteOutputFile()
        {
            try
            {
                LogFileOutput fh = new LogFileOutput(FileName);

                fh.WriteStart();

                // Write all messages in logfile
                for (int i = 0; i < Messages.Length; i++)
                {
                    fh.WriteLine(Messages[i]);
                }

                // Save logfile
                fh.SaveFile();
            }
            catch (Exception)
            {

            }
        }
    }
}
