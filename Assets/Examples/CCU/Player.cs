using Netick;
using Netick.Unity;
using UnityEngine;

namespace Netick.Examples.CCU
{
    public class Player : NetworkBehaviour
    {
        public Vector3 cameraOffset = new Vector3(0, 40, -40);

        [Header("Automated Movement")]
        public bool autoMove = true;
        public float autoSpeed = 2;
        public float movementProbability = 0.5f;
        public float movementDistance = 20;

        [Header("Manual Movement")]
        public float manualSpeed = 10;

        // 网络变量
        [Networked] private bool Moving { get; set; }
        [Networked] private Vector3 StartPosition { get; set; }
        [Networked] private Vector3 Destination { get; set; }
        [Networked] private bool AutoMoveMode { get; set; }

        // 输入结构
        public struct PlayerInput : INetworkInput
        {
            public float horizontal;
            public float vertical;
            public bool toggleAutoMove;
        }

        // 缓存 transform 引用
        private Transform _tf;

        public override void NetworkStart()
        {
            _tf = transform;
            StartPosition = _tf.position;

            // 只有本地玩家设置相机
            if (IsInputSource)
            {
                Camera.main.transform.SetParent(transform, false);
                Camera.main.transform.localPosition = cameraOffset;
            }
        }

        public override void NetworkDestroy()
        {
            // 释放相机
            if (IsInputSource && Camera.main != null)
            {
                Camera.main.transform.SetParent(null, true);
            }
        }

        public override void NetworkUpdate()
        {
            // 只有输入源处理输入
            if (!IsInputSource) return;

            // 获取输入
            PlayerInput input = Sandbox.GetInput<PlayerInput>();

            // 设置移动输入
            input.horizontal = Input.GetAxis("Horizontal");
            input.vertical = Input.GetAxis("Vertical");

            // 切换自动移动模式（例如按Tab键）
            input.toggleAutoMove |= Input.GetKeyDown(KeyCode.Tab);

            // 提交输入
            Sandbox.SetInput(input);
        }

        public override void NetworkFixedUpdate()
        {
            // 获取输入
            if (!FetchInput(out PlayerInput input)) return;

            // 处理自动移动模式切换
            if (input.toggleAutoMove)
            {
                AutoMoveMode = !AutoMoveMode;
            }

            // 根据模式选择移动方式
            if (AutoMoveMode)
            {
                AutoMove();
            }
            else
            {
                ManualMove(input);
            }
        }

        private void AutoMove()
        {
            if (Moving)
            {
                if (Vector3.Distance(_tf.position, Destination) <= 0.01f)
                {
                    Moving = false;
                }
                else
                {
                    _tf.position = Vector3.MoveTowards(_tf.position, Destination, autoSpeed * Sandbox.FixedDeltaTime);
                }
            }
            else
            {
                float r = Random.value;
                if (r < movementProbability * Sandbox.FixedDeltaTime)
                {
                    // 计算随机位置
                    float circleX = Mathf.Cos(Random.value * Mathf.PI * 2);
                    float circleZ = Mathf.Sin(Random.value * Mathf.PI * 2);
                    Vector2 circlePos = new Vector2(circleX, circleZ);
                    Vector3 dir = new Vector3(circlePos.x, 0, circlePos.y);

                    // 设置目标位置
                    Destination = StartPosition + dir * movementDistance;
                    Moving = true;
                }
            }
        }

        private void ManualMove(PlayerInput input)
        {
            Vector3 direction = new Vector3(input.horizontal, 0, input.vertical);
            _tf.position += direction.normalized * (Sandbox.FixedDeltaTime * manualSpeed);
        }

        public override void NetworkRender()
        {
            // 这里可以添加视觉特效或插值
            // 例如，如果需要平滑移动，可以在这里进行插值
        }
    }
}