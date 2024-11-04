using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishNetMudballSpawn : NetworkBehaviour
{
    [SerializeField]
    private GameObject _prefab;
    [SerializeField]
    private Transform _spawnLocation;

    [ServerRpc(RequireOwnership = false)]
    private void OnTriggerSpawn()
    {
        GameObject go = Instantiate(_prefab);
        go.transform.position = _spawnLocation.position;
        base.Spawn(go);
    }
}
