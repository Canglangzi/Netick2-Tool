using Netick;
using Netick.Unity;
using UnityEngine;

namespace Netick.Examples.Pong
{
    public class Player : NetworkBehaviour
    {
        public float speed = 30;
        public Rigidbody2D rigidbody2d;
        
        // 输入结构
        public struct PlayerInput : INetworkInput
        {
            public float vertical;
        }

        public override void NetworkUpdate()
        {
            // 只有输入源处理输入
            if (!IsInputSource) return;
            
            // 获取输入
            PlayerInput input = Sandbox.GetInput<PlayerInput>();
            
            // 设置垂直输入
            input.vertical = Input.GetAxisRaw("Vertical");
            
            // 提交输入
            Sandbox.SetInput(input);
        }

        public override void NetworkFixedUpdate()
        {
            // 获取输入
            if (!FetchInput(out PlayerInput input)) return;
            
            // 计算速度
            Vector2 velocity = new Vector2(0, input.vertical) * speed * Sandbox.FixedDeltaTime;
            
            // 应用速度
#if UNITY_6000_0_OR_NEWER
            rigidbody2d.linearVelocity = velocity;
#else
            rigidbody2d.velocity = velocity;
#endif
        }
    }
}