using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertCNCHPGL
{
    class SubCall : FileCoordinateObject
    {
        private int _Index;
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                _Index = value;
            }
        }

        [Obsolete("This property is not used in this instance.")]
        public override int Color
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SubCall()
            : this(1)
        {

        }

        public SubCall(int index)
            : this(index, new Coordinate())
        {

        }

        public SubCall(int index, Coordinate origen)
            : base(origen)
        {
            Index = index;
        }

        public override void Write(Class_Output output)
        {
            output.OutputInstance(Index, Origen.X, Origen.Y);
        }
    }
}
