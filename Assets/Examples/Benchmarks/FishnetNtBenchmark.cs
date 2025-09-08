
using Netick.Unity;
using UnityEngine;

public class FishnetNtBenchmark : NetworkBehaviour
{
    [SerializeField] private GameObject _fishPrefab;
    [SerializeField] private int _fishCount = 100;

    public override void NetworkStart()
    {
        if (IsServer)
        {
            for (int i = 0; i < _fishCount; i++)
            {
                Sandbox.NetworkInstantiate(_fishPrefab, Random.insideUnitSphere * 10f, Random.rotation);
            }
        }
    }
}
