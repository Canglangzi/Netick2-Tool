using System;
using UnityEngine;

namespace NetickLeague
{
    [Serializable]
    public struct Wheel
    {
        public Transform transform;
        public Transform visualTrs;
        public float mass;
        public float radius;
        public float springStrength;
        public float springDamping;
        [Range(0, 1)]
        public float gripFactor;
        public bool canSteer;
        public bool motorized;
        public bool flipSteerDir;
    }
}
