using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class Coordinate
    {
        private double _X;
        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        private double _Y;
        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        public Coordinate()
        {
            X = 0;
            Y = 0;
        }

        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void Scale(double factor)
        {
            try
            {
                X = X * factor;
                Y = Y * factor;
            }
            catch (Exception e)
            {
                ErrorHandler.AddMessage(e);
            }
        }

        public static Coordinate operator +(Coordinate one, Coordinate two)
        {
            return new Coordinate(one.X + two.X, one.Y + two.Y);
        }
        
        public static Coordinate operator *(Coordinate one, Coordinate two)
        {
            return new Coordinate(one.X * two.X, one.Y * two.Y);
        }

        public static Coordinate operator -(Coordinate one, Coordinate two)
        {
            return new Coordinate(one.X - two.X, one.Y - two.Y);
        }

        public static Coordinate operator /(Coordinate one, Coordinate two)
        {
            return new Coordinate(one.X / two.X, one.Y / two.Y);
        }

        public static Coordinate operator %(Coordinate one, Coordinate two)
        {
            return new Coordinate(one.X % two.X, one.Y % two.Y);
        }
    }
}
