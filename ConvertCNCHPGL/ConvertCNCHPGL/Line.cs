using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class Line : FileCoordinateObject
    {
        private Coordinate _Destination;
        public Coordinate Destination
        {
            get
            {
                return _Destination;
            }
            set
            {
                _Destination = value;
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

        public Line()
            : this(new Coordinate(), new Coordinate(), 0)
        {
            
        }

        public Line(Coordinate origen, Coordinate destination)
            : this(origen, destination, 0)
        {
            
        }

        public Line(Coordinate origen, Coordinate destination, int color)
            : base(origen, color)
        {
            Destination = destination;
        }

        public Line(Coordinate origen, Coordinate destination, int color, double depth)
            : this (origen, destination, color)
        {
            Depth = depth;
        }

        public override void Write(Class_Output output)
        {
            output.OutputLine(Origen.X, Origen.Y, Destination.X, Destination.Y, Color, Depth);
        }
    }
}
