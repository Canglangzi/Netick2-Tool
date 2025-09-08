using CocKleBurs.GameFrameWork.LagCompensation;
using Netick;
using Netick.Unity;
using UnityEngine;

namespace Netick.Examples.WeaponSystem
{
    public class WeaponSystem : NetworkBehaviour
    {
        [Header("Weapon Settings")]
        public Weapon[] weapons;
        public Transform weaponMountPoint;
        public float switchWeaponCooldown = 0.5f;

        // 网络变量
        [Networked] public int CurrentWeaponIndex { get; set; }
        [Networked] public float NextWeaponSwitchTime { get; set; }
        [Networked] public bool IsReloading { get; set; }

        // 输入结构
        public struct WeaponInput : INetworkInput
        {
            public bool fire;
            public bool reload;
            public int weaponSwitch;
        }

        // 私有变量
       [SerializeReference] private Weapon _currentWeapon;
        private float _nextFireTime;
        private GameObject[] _weaponModels; // 存储所有武器模型

        public override void NetworkStart()
        {
            // 初始化所有武器模型
            InitializeWeaponModels();
            
            // 初始化武器
            if (Sandbox.IsServer)
            {
                CurrentWeaponIndex = 0;
                EquipWeapon(CurrentWeaponIndex);
            }
        }
       
        private void InitializeWeaponModels()
        {
            _weaponModels = new GameObject[weapons.Length];
            
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i].weaponModel != null)
                {
                    // 实例化武器模型并隐藏
                    _weaponModels[i] = Instantiate(
                        weapons[i].weaponModel,
                        weaponMountPoint.position,
                        weaponMountPoint.rotation,
                        weaponMountPoint
                    );
                    
                    // 网络生成武器模型
                    Sandbox.LocalInstantiate(_weaponModels[i]);
                    
                    // 初始隐藏所有武器
                    _weaponModels[i].SetActive(false);
                }
            }
        }

        public override void NetworkUpdate()
        {
            if (IsServer)
            {
                var networkPlayerId = Object.InputSourcePlayerId;
                var networkPlayer  =Sandbox.GetPlayerById(networkPlayerId);
                var conn = networkPlayer as ServerConnection;
           
                 
                 if (conn == null)
                 {
                     rtt = 0;
                 }
                 else
                 {
                     rtt = conn.RTT.Average* 1000f;
                 }
            }
            // 只有输入源处理输入
            if (!IsInputSource) return;

            // 获取输入
            WeaponInput input = Sandbox.GetInput<WeaponInput>();

            // 设置射击输入
            input.fire = Input.GetButton("Fire1");

            // 设置装弹输入
            input.reload |= Input.GetKeyDown(KeyCode.R);

            // 设置武器切换输入
            input.weaponSwitch = GetWeaponSwitchInput();

            // 提交输入
            Sandbox.SetInput(input);
        }

        public override void NetworkFixedUpdate()
        {
            // 获取输入
            if (!FetchInput(out WeaponInput input)) return;

            // 处理武器切换
            HandleWeaponSwitching(input);

            // 处理装弹
            if (input.reload)
            {
                StartReload();
            }

            // 处理射击
            if (input.fire)
            {
                HandleFiring();
            }
        }

        #region 输入处理
        private int GetWeaponSwitchInput()
        {
            // 数字键切换武器
            for (int i = 0; i < weapons.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    return i;
                }
            }

            // 鼠标滚轮切换武器
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0)
            {
                return (CurrentWeaponIndex + 1) % weapons.Length;
            }
            else if (scroll < 0)
            {
                return (CurrentWeaponIndex - 1 + weapons.Length) % weapons.Length;
            }

            return -1;
        }
        #endregion

        #region 武器操作
        private void HandleWeaponSwitching(WeaponInput input)
        {
            // 检查冷却时间
            if (Sandbox.FixedDeltaTime < NextWeaponSwitchTime) return;

            // 处理武器切换
            if (input.weaponSwitch >= 0 && input.weaponSwitch < weapons.Length && input.weaponSwitch != CurrentWeaponIndex)
            {
                SwitchWeapon(input.weaponSwitch);
                NextWeaponSwitchTime = Sandbox.DeltaTime + switchWeaponCooldown;
            }
        }

        private void HandleFiring()
        {
            if (_currentWeapon == null) return;
            if (IsReloading) return;

          
                // 检查弹药
                if (_currentWeapon.currentAmmo <= 0)
                {
                    // 自动装弹
                    StartReload();
                    return;
                }

                // 射击
                FireWeapon();
                _nextFireTime = Sandbox.DeltaTime + _currentWeapon.fireRate;

                // 减少弹药
                _currentWeapon.currentAmmo--;

                // 如果弹药耗尽，自动装弹
                if (_currentWeapon.currentAmmo <= 0)
                {
                    StartReload();
                }
        }
        
        
      [Networked]  public double rtt { get; set; }
        private void FireWeapon()
        {
            // 服务器端射击逻辑
            if (_currentWeapon == null) return;
            Vector3 direction = transform.forward;
    
                LagCompensationManager.Simulate(rtt, () =>
                {
                    // 执行射线检测
                    if (Sandbox.Physics.Raycast(weaponMountPoint.position, direction, out RaycastHit hit, _currentWeapon.range))
                    {
                        // 处理命中
                        HandleHit(hit.collider, hit.point, direction);
                    }
                });
        
  
            // 播放射击效果（本地执行）
            PlayFireEffects();
        }

        private void HandleHit(Collider hitCollider, Vector3 hitPoint, Vector3 direction)
        {
            // 处理命中逻辑
            Health health = hitCollider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(_currentWeapon.damage);
            }
        }

        private void StartReload()
        {
            if (_currentWeapon == null) return;
            if (_currentWeapon.currentAmmo >= _currentWeapon.magazineSize) return;
            if (_currentWeapon.reserveAmmo <= 0) return;

            // 计算需要装填的弹药数量
            int ammoNeeded = _currentWeapon.magazineSize - _currentWeapon.currentAmmo;
            int ammoToReload = Mathf.Min(ammoNeeded, _currentWeapon.reserveAmmo);

            // 装填弹药
            _currentWeapon.currentAmmo += ammoToReload;
            _currentWeapon.reserveAmmo -= ammoToReload;
        }
        
        private void SwitchWeapon(int newWeaponIndex)
        {
            // 切换武器
            CurrentWeaponIndex = newWeaponIndex;
            EquipWeapon(newWeaponIndex);
        }

        private void EquipWeapon(int weaponIndex)
        {
            // 隐藏当前武器模型
            if (_weaponModels != null && _weaponModels.Length > CurrentWeaponIndex)
            {
                if (_weaponModels[CurrentWeaponIndex] != null)
                {
                    _weaponModels[CurrentWeaponIndex].SetActive(false);
                }
            }

            // 装备新武器
            if (weaponIndex >= 0 && weaponIndex < weapons.Length)
            {
                _currentWeapon = weapons[weaponIndex];
                
                // 显示新武器模型
                if (_weaponModels != null && _weaponModels.Length > weaponIndex)
                {
                    if (_weaponModels[weaponIndex] != null)
                    {
                        _weaponModels[weaponIndex].SetActive(true);
                    }
                }
            }
            else
            {
                _currentWeapon = null;
            }
        }
        
        private void PlayFireEffects()
        {
            // 播放枪口闪光效果
            if (_currentWeapon.muzzleFlash != null)
            {
                GameObject muzzleFlash = Instantiate(
                    _currentWeapon.muzzleFlash,
                    weaponMountPoint.position,
                    weaponMountPoint.rotation
                );
                
                Destroy(muzzleFlash, 0.1f);
            }
        }
        #endregion
    }

    #region 武器相关类
    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public GameObject weaponModel;
        public GameObject muzzleFlash;
        
        [Header("战斗属性")]
        public float damage = 10f;
        public float fireRate = 0.5f;
        public float range = 100f;
        public int magazineSize = 30;
        
        [Header("弹药管理")]
        public int currentAmmo = 30;
        public int reserveAmmo = 90;
    }
    
    #endregion
}