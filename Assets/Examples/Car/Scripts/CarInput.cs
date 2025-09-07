

using Netick;

namespace NetickLeague
{
    [Networked]
    public struct CarInput : INetworkInput
    {
        [Networked] public float Throttle { get; set; }
        [Networked] public float Steering { get; set; }
    }
}