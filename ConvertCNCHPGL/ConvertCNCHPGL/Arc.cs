using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class Arc : FileCoordinateObject
    {
        private Coordinate _LastOrigen;
        public Coordinate LastOrigen
        {
            get
            {
                return _LastOrigen;
            }
            set
            {
                _LastOrigen = value;
            }
        }

        private Coordinate _Distance;
        public Coordinate Distance
        {
            get
            {
                return _Distance;
            }
            set
            {
                _Distance = value;
            }
        }

        private double _Radius;
        public double Radius
        {
            get
            {
                return _Radius;
            }
            set
            {
                _Radius = value;
            }
        }

        private double _Depth;
        public double Depth
        {
            get
            {
                return _Depth;
            }
            set
            {
                _Depth = value;
            }
        }

        public double Length
        {
            get
            {
                return 2 * Radius * (Angle / 360);
            }
        }

        public double Angle
        {
            get
            {
                return 60 * Math.Cos(
                    (Math.Sqrt(Math.Pow(Distance.X - LastOrigen.X, 2) + Math.Pow(Distance.Y - LastOrigen.Y, 2)) + // P12
                    Math.Sqrt(Math.Pow(Distance.X - Origen.X, 2) + Math.Pow(Distance.Y - Origen.Y, 2)) - // P13
                    Math.Sqrt(Math.Pow(LastOrigen.X - Origen.X, 2) + Math.Pow(LastOrigen.Y - Origen.Y, 2))) / // P23
                    (2 * Math.Sqrt(Math.Pow(Distance.X - LastOrigen.X, 2) + Math.Pow(Distance.Y - LastOrigen.Y, 2)) * // P12
                    Math.Sqrt(Math.Pow(Distance.X - Origen.X, 2) + Math.Pow(Distance.Y - Origen.Y, 2)))); // P13
            }
        }

        public Arc()
            : this(new Coordinate(), new Coordinate(), new Coordinate(), 0)
        {
            
        }

        public Arc(Coordinate lastOrigen)
            : this(lastOrigen, new Coordinate(), new Coordinate(), 0)
        {

        }
        
        public Arc(Coordinate lastOrigen, Coordinate origen)
            : this(lastOrigen, origen, new Coordinate(), 0)
        {

        }

        public Arc(Coordinate lastOrigen, Coordinate origen, Coordinate distance)
            : this(lastOrigen, origen, distance, 0)
        {

        }

        public Arc(Coordinate lastOrigen, Coordinate origen, Coordinate distance, int color)
            : base(origen, color)
        {
            LastOrigen = lastOrigen;
            Distance = distance;
            double dix = LastOrigen.X - Distance.X;
            double diy = LastOrigen.Y - Distance.Y;
            Radius = Math.Sqrt(dix * dix + diy * diy);
        }

        public Arc(Coordinate lastOrigen, Coordinate origen, Coordinate distance, int color, bool negativeRadius)
            : this(lastOrigen, origen, distance, color)
        {
            if (negativeRadius)
                Radius = -Radius;
        }

        public Arc(Coordinate lastOrigen, Coordinate origen, Coordinate distance, int color, bool negativeRadius, double depth)
            : this (lastOrigen, origen, distance, color, negativeRadius)
        {
            Depth = depth;
        }

        public override void Write(Class_Output output)
        {
            output.OutputArc(LastOrigen.X, LastOrigen.Y, Origen.X, Origen.Y, Distance.X, Distance.Y, Radius, Color, Depth);
        }
    }
}
