using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEditor;

public class FishNetPingRPCTest : NetworkBehaviour
{
    [SerializeField]private AudioSource m_AudioSource;

    public void LocalEventFire()
    {
        Debug.Log("Firing Event");

        RPCPlaySound();
    }


    [ObserversRpc]
    public void RPCPlaySound()
    {
        /*Sometimes Fishnet needs RPC events to be checked if the caller is the server so I had this here initially accoriding to normal setup but moved the Play() method out during testing*/
        if (IsServerInitialized)
        {
            Debug.Log("Button clicked on Fish Net Object");

            m_AudioSource.Play();
        }
    }
}
