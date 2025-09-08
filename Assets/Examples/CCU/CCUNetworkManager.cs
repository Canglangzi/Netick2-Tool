using System;
using Netick.Unity;
using UnityEngine;
using System.Collections.Generic;

namespace Netick.Examples.CCU
{
    [AddComponentMenu("")]
    public class CCUNetworkManager : NetworkBehaviour
    {
        [Header("Spawns")]
        public int spawnAmount = 10_000;
        public float interleave = 1;
        public GameObject spawnPrefab;

        [Range(0, 1)] public float spawnPositionRatio = 0.01f;

        System.Random random = new System.Random(42);
        List<Transform> startPositions = new List<Transform>();


        public void Awake()
        {
            
        }

        void SpawnAll()
        {
            // 清除已有的生成位置
            foreach (Transform position in startPositions)
                Destroy(position.gameObject);
            startPositions.Clear();

            float sqrt = Mathf.Sqrt(spawnAmount);
            float offset = -sqrt / 2 * interleave;

            int spawned = 0;
            for (int spawnX = 0; spawnX < sqrt; ++spawnX)
            {
                for (int spawnZ = 0; spawnZ < sqrt; ++spawnZ)
                {
                    if (spawned >= spawnAmount) break;

                    // 直接计算位置
                    float x = offset + spawnX * interleave;
                    float z = offset + spawnZ * interleave;
                    Vector3 position = new Vector3(x, 0, z);

                    // 直接网络实例化（避免本地实例化）
                    Sandbox.NetworkInstantiate(spawnPrefab, position, Quaternion.identity);
                    spawned++;

                    // 添加玩家出生点
                    if (random.NextDouble() <= spawnPositionRatio)
                    {
                        GameObject spawnGO = new GameObject("Spawn");
                        spawnGO.transform.position = position;
                       // spawnGO.AddComponent<NetworkStartPosition>();
                        startPositions.Add(spawnGO.transform);
                    }
                }
            }
        }

        public override void NetworkStart()
        {
            if (IsServer)
            {
                SpawnAll();
            }
        }

        public  Transform GetStartPosition()
        {
            startPositions.RemoveAll(t => t == null);
            return startPositions.Count == 0 ? null : startPositions[random.Next(0, startPositions.Count)];
        }
        
    }
}