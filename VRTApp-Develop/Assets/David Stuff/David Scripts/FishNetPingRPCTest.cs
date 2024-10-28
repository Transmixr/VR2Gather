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
        Debug.Log($"{Name()}: Local Event Fired on source client");

        ServerPlaySound();
    }


    [ServerRpc(RequireOwnership = false)]
    public void ServerPlaySound()
    {
        Debug.Log($"{Name()}: Server Event Fired");
        ClientPlaySound();

    }

    [ObserversRpc]
    public void ClientPlaySound()
    {

        Debug.Log($"{Name()}: Client Event Fired");
        m_AudioSource.Play();

    }
}
