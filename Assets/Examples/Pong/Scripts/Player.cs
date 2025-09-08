using Netick.Unity;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class Player : NetworkBehaviour
    {
        public float speed = 30;
        public Rigidbody2D rigidbody2d;

        public void NetworkFixedUpdate()
        {
            // only let the local player control the racket.
            // don't control other player's rackets
            if (IsInputSource)
#if UNITY_6000_0_OR_NEWER
                rigidbody2d.linearVelocity = new Vector2(0, Input.GetAxisRaw("Vertical")) * speed * Time.fixedDeltaTime;
#else
                rigidbody2d.velocity = new Vector2(0, Input.GetAxisRaw("Vertical")) * speed * Time.fixedDeltaTime;
#endif
        }
    }
}
