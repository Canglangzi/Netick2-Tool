using Netick;
using Netick.Unity;
using UnityEngine;
using UnityEngine.AI;

namespace Netick.Examples.TanksHybrid
{
    public class Tank : NetworkBehaviour
    {
        [Header("Components")]
        public NavMeshAgent agent;
        public Animator animator;
        public TextMesh healthBar;
        public Transform turret;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Firing")]
        public KeyCode shootKey = KeyCode.Space;
        public Projectile projectilePrefab;
        public Transform projectileMount;

        [Header("Stats")]
        public int health = 5;
        
        // 输入结构
        public struct TankInput : INetworkInput
        {
            public float horizontal;
            public float vertical;
            public bool shoot;
            public Vector3 mousePosition;
        }
        
        // 网络变量
        [Networked] public int LastHealth { get; set; }
  
        [Networked] public  Quaternion TurretRotation{ get; set; }
            
        [Networked] public  bool IsMoving { get; set; }


        public NetworkObject NetworkObject;
        public override void NetworkStart()
        {
            NetworkObject= gameObject.GetComponent<NetworkObject>();
            name = $"Player[{InputSource.PlayerId}|{(IsInputSource ? "local" : "remote")}]";
            if (IsServer)
            {
                name = $"Player[{InputSource.PlayerId}|server]";
            }
            if (agent != null)
            {
                // agent.updatePosition = false;
                // agent.updateRotation = false;
            }
        }
        public override void NetworkDestroy()
        {
            if (agent != null)
            {
                agent.velocity = Vector3.zero;
            }
        }
        public override void NetworkUpdate()
        {
            // 只有输入源处理输入
            if (!IsInputSource) return;
            
            // 获取输入
            TankInput input = Sandbox.GetInput<TankInput>();
            
            // 设置移动输入
            input.horizontal = Input.GetAxis("Horizontal");
            input.vertical = Input.GetAxis("Vertical");
            
            // 设置射击输入
            input.shoot |= Input.GetKeyDown(shootKey);
            
            // 设置鼠标位置
            input.mousePosition = Input.mousePosition;
            
            // 提交输入
            Sandbox.SetInput(input);
        }

        public override void NetworkFixedUpdate()
        {
            // 获取输入
            if (!FetchInput(out TankInput input)) return;
            HandleMovement(input);
            
            HandleTurretRotation(input);
            
            if (!IsResimulating)
            {
                if (input.shoot)
                {
                    if (IsServer)
                    {
                        CmdFire();
                    }
                } 
            }

            UpdateHealthDisplay();
        }

        public override void NetworkRender()
        {
            if (IsProxy)
            {
                animator.SetBool("Moving", IsMoving);
                turret.rotation = Quaternion.Lerp(turret.rotation, TurretRotation, Sandbox.DeltaTime * 10f);
            }
          
        }

        private void HandleMovement(TankInput input)
        {
            // 旋转
            transform.Rotate(0, input.horizontal * rotationSpeed * Sandbox.FixedDeltaTime, 0);
            
         
          //  Debug.Log(input.horizontal);
         //   Debug.Log(input.vertical);
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            agent.velocity = forward * Mathf.Max(input.vertical, 0) * agent.speed;
            
            // 更新移动状态
            IsMoving = agent.velocity != Vector3.zero;
        }

        private void HandleTurretRotation(TankInput input)
        {
            
                Ray ray = Camera.main.ScreenPointToRay(input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100))
                {
                    Vector3 lookRotation = new Vector3(hit.point.x, turret.position.y, hit.point.z);
                    turret.LookAt(lookRotation);
                    TurretRotation = turret.rotation;
                }
        }


        [OnChanged(nameof(TurretRotation))]
        public void OnTurretRotation(OnChangedData info)
        {
            turret.rotation = TurretRotation;
        }
        [OnChanged(nameof(LastHealth))]
        public void OnUpdateHealthDisplay(OnChangedData info)
        {
            UpdateHealthDisplay();
        }
        private void UpdateHealthDisplay()
        {
            // 手动同步健康值
            if (health != LastHealth)
            {
                LastHealth = health;
                healthBar.text = new string('-', health);
            }
        }
        
        private void CmdFire()
        {
       
           // GameObject projectile = Sandbox.LocalInstantiate(projectilePrefab, projectileMount.position, projectileMount.rotation);
                
            Sandbox.NetworkInstantiate(projectilePrefab, projectileMount.position, projectileMount.rotation);
            projectilePrefab.Initialize(this);
            RpcOnFire();
        }

    
        private void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }
        
        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0)
            {
            }
        }
    }
}