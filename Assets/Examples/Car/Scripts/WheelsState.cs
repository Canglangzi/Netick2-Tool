using System;
using Netick;

namespace NetickLeague
{
    [Networked]
    public struct WheelsState
    {
        private WheelState _wheel0;
        private WheelState _wheel1;
        private WheelState _wheel2;
        private WheelState _wheel3;

        public WheelState this[int index]
        {
            get
            {
                return index switch
                {
                    0 => _wheel0,
                    1 => _wheel1,
                    2 => _wheel2,
                    3 => _wheel3,
                    _ => throw new IndexOutOfRangeException("Invalid wheel index")
                };
            }
            set
            {
                switch (index)
                {
                    case 0: _wheel0 = value; break;
                    case 1: _wheel1 = value; break;
                    case 2: _wheel2 = value; break;
                    case 3: _wheel3 = value; break;
                    default: throw new IndexOutOfRangeException("Invalid wheel index");
                }
            }
        }
    }
}
