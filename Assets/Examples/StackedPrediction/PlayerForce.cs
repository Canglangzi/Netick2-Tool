// players can apply force to any stacked cube.
// this has to be on the player instead of on the cube via OnMouseDown,
// because OnMouseDown would get blocked by the predicted ghost objects.

using Netick.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Netick.Examples.PredictionBenchmark
{
    public class PlayerForce : NetworkBehaviour
    {
        public float force = 50;
        public struct PlayerForceInput : INetworkInput
        {
            public Vector3 velocity;
        }
        public override void NetworkUpdate()
        {
            if (IsInputSource)
            {
                PlayerForceInput input = Sandbox.GetInput<PlayerForceInput>();
                input.velocity = Input.mousePosition;
                Sandbox.SetInput(input);
            }
        }

        public override void NetworkFixedUpdate()
        {
            if (FetchInput(out  PlayerForceInput input))
            {
                Ray ray = Camera.main.ScreenPointToRay( input.velocity);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 impulse = Random.insideUnitSphere * force;
                    hit.collider.GetComponent<Rigidbody>()?.AddForce(impulse, ForceMode.Impulse);
                }
            }
        }
    }
}
