using Netick;
using Netick.Unity;
using UnityEngine;

namespace Netick.Examples.TanksHybrid
{
    public class Projectile : NetworkBehaviour
    {
        [Header("Settings")]
        public float destroyAfter = 2f;
        public float force = 1000f;
        public int damage = 1;
        
        [Header("References")]
        public Rigidbody rigidBody;
        
        // 记录发射者，用于忽略碰撞
        private Tank _ownerTank;
        private int _ownerPlayerId = -1;

         [SerializeReference] private NetworkObject NetworkObject;
        public void Initialize(Tank owner)
        {
            _ownerTank = owner;
            _ownerPlayerId = owner.Object.InputSource.PlayerId;
            NetworkObject = GetComponent<NetworkObject>();
        }

        public override void NetworkStart()
        {
            // 应用初始力
            rigidBody.AddForce(transform.forward * force);
            
            // 仅服务器设置销毁计时器
            if (IsServer)
            {
                Invoke(nameof(DestroySelf), destroyAfter); 
            }
        }
        
        // 销毁子弹（无参版本）
        void DestroySelf()
        {
            Sandbox.Destroy(NetworkObject);
        }
        
        void OnTriggerEnter(Collider other)
        {
            // 只在服务器处理碰撞
            if (!IsServer) return;
            
            // 忽略与自身的碰撞
            if (other.transform == _ownerTank?.transform) 
                return;
            
            // 尝试获取坦克组件
            Tank tank = null;
            
            // 检查碰撞对象自身是否有Tank组件
            if (other.TryGetComponent(out tank))
            {
                ApplyDamage(tank);
            }
            // 检查父对象是否有Tank组件
            else if (other.transform.parent != null && 
                     other.transform.parent.TryGetComponent(out tank))
            {
                ApplyDamage(tank);
            }
        }
        
        void ApplyDamage(Tank tank)
        {
            Debug.Log($"Hit tank: {tank.name}");
            
            // 应用伤害
            tank.health -= damage;
            
            DestroySelf();
            
            // 检查坦克是否被摧毁
            if (tank.health <= 0)
            {
                Sandbox.Kick(tank.InputSource);
                Sandbox.DestroyPool(tank.gameObject);
            }
        }
    }
}