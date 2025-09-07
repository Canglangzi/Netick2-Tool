using Netick;

namespace NetickLeague
{
    [Networked]
    public struct WheelState
    {
        [Networked][Smooth] public float Offset { get; set; }
    }

    [Networked]
    public struct CarState
    {
        public WheelsState Wheels;
        [Networked][Smooth] public float Steering{ get; set;}
    }
}
