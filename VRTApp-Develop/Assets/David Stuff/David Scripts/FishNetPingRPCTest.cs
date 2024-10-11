using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEditor;

public class FishNetPingRPCTest : NetworkBehaviour
{
    [SerializeField]private AudioSource m_AudioSource;

    string Name() {
        return "FishNetPingRPCTest";
    }

    void Awake() {
        Debug.Log($"{Name()}: Awake");
    }
    void Start() {
        Debug.Log($"{Name()}: Start");
    }
    void OnEnable() {
        Debug.Log($"{Name()}: OnEnable");
    }

    void OnDisable() {
        Debug.Log($"{Name()}: OnDisable");
    }

    public void LocalEventFire()
    {
        Debug.Log($"{Name()}: Firing Event");

        RPCPlaySound();
    }


    [ObserversRpc]
    public void RPCPlaySound()
    {
        /*Sometimes Fishnet needs RPC events to be checked if the caller is the server so I had this here initially accoriding to normal setup but moved the Play() method out during testing*/
        if (IsServerInitialized)
        {
            Debug.Log($"{Name()}: Button clicked on Fish Net Object");

            m_AudioSource.Play();
        }
    }
}
