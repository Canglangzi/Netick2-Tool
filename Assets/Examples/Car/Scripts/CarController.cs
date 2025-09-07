using System;
using Netick;
using Netick.Unity;
using UnityEngine;

namespace NetickLeague
{
    public partial class CarController : NetworkBehaviour
    {
        [SerializeField] private GameObject _camera;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _collider;
        [SerializeField] private Wheel[] _wheels;
        [SerializeField] private float steeringSpeed = 200f;
        [SerializeField] private float speed = 20f;
        [SerializeField] private float maxSpeed = 50f;
        [SerializeField] private float steeringAngle = 40;
        [SerializeField] private AnimationCurve _powerCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private CarState _carState;
       [Networked] private CarInput _input { get; set; }

        public override void NetworkStart()
        {
            _camera.SetActive(IsInputSource);
        }
        
        
        public override void NetworkUpdate()
        {
            var networkInput = Sandbox.GetInput<CarInput>();
            networkInput.Throttle = Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1f, 1f);
            networkInput.Steering = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f);

            Sandbox.SetInput(networkInput);
        }

        
        public override void NetworkFixedUpdate()
        {
            if(!FetchInput(out CarInput input)) return;

            _input = input;
            var steeringRotation = _input.Steering * steeringAngle;
            _carState.Steering = Mathf.MoveTowards(_carState.Steering, steeringRotation, steeringSpeed * Sandbox.FixedDeltaTime);

            var len = _wheels.Length;
            for (var i = 0; i < len; i++)
            {
                var wheel = _wheels[i];

                if (wheel.canSteer)
                    wheel.transform.localRotation = Quaternion.Euler(0, _carState.Steering, 0);

                CalculateWheelForces(i, _input, ref _carState.Wheels);
            }
        }
 
        public override void NetworkRender()
        {
                CarStateVisual();
        }

        private void CarStateVisual()
        {
            for (var i = 0; i < _wheels.Length; i++)
            {
                var wheel = _wheels[i];
                var wheelState = _carState.Wheels[i];

                if (wheel.canSteer)
                    wheel.visualTrs.transform.localRotation = Quaternion.Euler(0, _carState.Steering, 0);

                var targetPos =
                    new Vector3(0, wheelState.Offset, 0);

                wheel.visualTrs.transform.localPosition = Vector3.Lerp(
                    wheel.visualTrs.transform.localPosition,
                    targetPos,
                    Sandbox.DeltaTime * 10f);
            }
        }

        private void CalculateWheelForces(int i, CarInput input, ref WheelsState state)
        {
            var wheel = _wheels[i];
            
            var springDir = wheel.transform.up;
            var accelDir = wheel.transform.forward;
            var tirePos = wheel.transform.position;

            var springRoot = tirePos + springDir * wheel.radius;
            var suspensionRestDist = wheel.radius;

            var ray = new Ray(springRoot, -springDir);
            var maxDistance = wheel.radius * 2f;

            var rayHit = PhysicsUtils.Raycast(ray, out var hit, suspensionRestDist + maxDistance, _collider);
            var offset = suspensionRestDist - maxDistance;

            if (rayHit)
            {
                // handle spring force
                var tireWorldVel = _rigidbody.GetPointVelocity(tirePos);
                var dist = hit.distance - wheel.radius;
                offset = suspensionRestDist - dist;

                var vel = Vector3.Dot(springDir, tireWorldVel);
                var force = offset * wheel.springStrength - vel * wheel.springDamping;

                var springForce = springDir * force;

                // handle steering force
                var steeringDir = wheel.flipSteerDir ? -wheel.transform.right : wheel.transform.right;
                var steeringVel = Vector3.Dot(steeringDir, tireWorldVel);

                // var steeringForce = steeringDir * desiredAccel;
                var steeringForce = steeringDir * -steeringVel;

                // acceleration/braking force
                var accelForce = Vector3.zero;
                if (wheel.motorized)
                {
                    var carSpeed = Vector3.Dot(transform.forward, _rigidbody.linearVelocity);
                    var normalizedAccel = Mathf.Clamp01(Mathf.Abs(carSpeed) / maxSpeed);
                    var availableTorque = _powerCurve.Evaluate(normalizedAccel) * input.Throttle * speed;
                    accelForce = accelDir * availableTorque;
                }
                
                _rigidbody.AddForceAtPosition(steeringForce + springForce + accelForce, tirePos);
            }

            var s = state[i];
            s.Offset = offset;
            state[i] = s;
        }
    }
}
