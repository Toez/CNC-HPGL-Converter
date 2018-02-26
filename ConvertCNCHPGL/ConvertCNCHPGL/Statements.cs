using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    static class Statements
    {
        public static bool IsNull<T>(T obj)
        {
            bool result = false;

            try
            {
                if (obj == null)
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public static bool IsNull(Commands obj)
        {
            bool result = false;

            try
            {
                if (obj == Commands.NONE || obj == null)
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public static bool IsNull(string obj)
        {
            bool result = false;

            try
            {
                if (obj == string.Empty || obj == "" || obj == null)
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public static bool IsNull(int obj)
        {
            bool result = false;

            try
            {
                if (obj == 0)
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public static bool IsNull(CommandRef obj)
        {
            bool result = false;

            try
            {
                if (IsNull(obj.Command) || IsNull(obj.Line) || IsNull(obj.Reference))
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public static bool IsNull(Coordinate obj)
        {
            bool result = false;

            try
            {
                if (obj == null)
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }

        public static bool IsNull(SubProgram obj)
        {
            bool result = false;

            try
            {
                if (obj == null)
                    result = true;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }

            return result;
        }
    }
}
