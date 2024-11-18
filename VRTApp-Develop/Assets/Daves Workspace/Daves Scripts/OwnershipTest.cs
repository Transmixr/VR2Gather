using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class OwnershipTest : NetworkBehaviour
{
    public Text ownerTag;

    private NetworkObject networkObject;

    private void Start()
    {
        networkObject = gameObject.GetComponent<NetworkObject>();
    }

    [ServerRpc]
    public void ShowCurrentOwner()
    {
        ownerTag.text = networkObject.Owner.ClientId.ToString();
    }

    private void TransferOwnership(NetworkConnection newOwner)
    {
        if(IsOwner) return;
        
        networkObject.GiveOwnership(newOwner);
    }
}
