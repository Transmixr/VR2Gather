using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting;
using System;

public class Fish2Gather : Transport
{
    public override event Action<ClientConnectionStateArgs> OnClientConnectionState;
    public override event Action<ServerConnectionStateArgs> OnServerConnectionState;
    public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState;

    public override event Action<ClientReceivedDataArgs> OnClientReceivedData;
    public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

    public override string GetConnectionAddress(int connectionId)
    {
        throw new NotImplementedException();
    }

    public override LocalConnectionState GetConnectionState(bool server)
    {
        throw new NotImplementedException();
    }

    public override RemoteConnectionState GetConnectionState(int connectionId)
    {
        throw new NotImplementedException();
    }

    public override int GetMTU(byte channel)
    {
        throw new NotImplementedException();
    }

    public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
    {
        throw new NotImplementedException();
    }

    public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs)
    {
        throw new NotImplementedException();
    }

    public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
    {
        throw new NotImplementedException();
    }

    public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
    {
        throw new NotImplementedException();
    }

    public override void HandleServerReceivedDataArgs(ServerReceivedDataArgs receivedDataArgs)
    {
        throw new NotImplementedException();
    }

    public override void IterateIncoming(bool server)
    {
        throw new NotImplementedException();
    }

    public override void IterateOutgoing(bool server)
    {
        throw new NotImplementedException();
    }

    public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
    {
        throw new NotImplementedException();
    }

    public override void SendToServer(byte channelId, ArraySegment<byte> segment)
    {
        throw new NotImplementedException();
    }

    public override void Shutdown()
    {
        throw new NotImplementedException();
    }

    public override bool StartConnection(bool server)
    {
        throw new NotImplementedException();
    }

    public override bool StopConnection(bool server)
    {
        throw new NotImplementedException();
    }

    public override bool StopConnection(int connectionId, bool immediately)
    {
        throw new NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
