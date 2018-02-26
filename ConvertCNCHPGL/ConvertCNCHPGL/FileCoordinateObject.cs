using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class FileCoordinateObject : FileObject
    {
        private Coordinate _Origen;
        public Coordinate Origen
        {
            get
            {
                return _Origen;
            }
            set
            {
                _Origen = value;
            }
        }

        private int _Color;
        public virtual int Color
        {
            get
            {
                return _Color;
            }
            set
            {
                _Color = value;
            }
        }

        public FileCoordinateObject()
            : this(new Coordinate(), 0)
        {

        }

        public FileCoordinateObject(Coordinate origen)
            : this(origen, 0)
        {
            
        }

        public FileCoordinateObject(Coordinate origen, int color)
            : base()
        {
            Origen = origen;
            Color = color;
        }
    }
}
