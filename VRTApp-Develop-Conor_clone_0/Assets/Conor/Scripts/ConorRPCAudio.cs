using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FishNet.Connection;
using FishNet.Object;

public class ConorRPCAudio : NetworkBehaviour
{
    public GameObject audioPlayer;

    public override void OnStartClient()
    {
        base.OnStartClient();
    
    }


    public void TestScript()
    {
        // Call the server to notify it of the event
        PlayAudioSampleServerRpc();
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void PlayAudioSampleServerRpc()
    {
        // Call ObserversRpc on all clients except the owner
        PlayAudioSampleForOthers();
    }

    
    [ObserversRpc]
    public void PlayAudioSampleForOthers()
    {
        
        if (!IsOwner && !IsServer)
        {
            
            audioPlayer.GetComponent<AudioSource>().Play();
        }
    }
}
