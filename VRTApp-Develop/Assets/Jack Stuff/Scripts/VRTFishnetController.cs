using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting;
using VRT.Orchestrator.Wrapping;
using VRT.Pilots.Common;
using System;
using JetBrains.Annotations;
using System.Net;

public class VRTFishnetController : NetworkIdBehaviour
{
    public class FishnetStartupData : BaseMessage
    {
        public string serverHost;
        public ushort serverPort;
    };

    [SerializeField]
    private string hostName;

    [SerializeField]
    private NetworkManager _networkManager;
    [SerializeField]
    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
   
    [SerializeField]
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;
    protected override void Awake()
    {
        base.Awake();
        OrchestratorController.Instance.RegisterEventType(MessageTypeID.TID_FishnetStartupData, typeof(FishnetStartupData));
        hostName = Dns.GetHostName();
        // xxxjack should check that it's valid, and resolves.
        // xxxjack otherwise we should use some other means.
        if (_networkManager == null) {
            _networkManager = FindObjectOfType<NetworkManager>();
        }
        if (_networkManager == null)
        {
            Debug.LogError($"{Name()}: Fishnet NetworkManager not found");
        }
        _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
    
    }

    void OnDestroy()
    {
        if (_networkManager == null)
            return;

        _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        base.OnDestroy();
    }
    public virtual void OnEnable()
    {
        OrchestratorController.Instance.Subscribe<FishnetStartupData>(StartFishnetClient);
    }


    public virtual void OnDisable()
    {
        OrchestratorController.Instance.Unsubscribe<FishnetStartupData>(StartFishnetClient);
        // xxxjack we need Fishnet teardown as well (on end-of-scene, or maybe end-of-application)
        if (_serverState != LocalConnectionState.Stopped) {
            _networkManager.ServerManager.StopConnection(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (OrchestratorController.Instance.UserIsMaster) {
            StartFishnetServer();
            BroadcastFishnetServerAddress();
        }
    }

    
    public string Name() {
        return "VRTFishnetController";
    }

    void StartFishnetServer() {
        Debug.Log($"{Name()}: Starting Fishnet server on VR2Gather master. host={hostName}");
        if (_serverState != LocalConnectionState.Started) {
            _networkManager.ServerManager.StartConnection();
        }
       
    }

    void BroadcastFishnetServerAddress() {
        // xxxjack This will only be run on the master. Use a VR2Gather Orchestrator message to have all session participants call StartFishnetClient.
        // xxxjack maybe we ourselves (the master) have to call it also, need to check.
        FishnetStartupData serverData = new() {
            serverHost=hostName,
            serverPort=7770
        };
        OrchestratorController.Instance.SendTypeEventToAll(serverData);
        StartFishnetClient(serverData);
    }

    void StartFishnetClient(FishnetStartupData server) {
        // xxxjack this is going to need at least one argument (the address of the Fishnet server)
        Debug.Log($"{Name()}: Starting Fishnet client, host={server.serverHost}, port={server.serverPort}");
        if (_clientState == LocalConnectionState.Stopped) {
            _networkManager.ClientManager.StartConnection(server.serverHost, server.serverPort);
        }
        // xxxjack this should create the connection to the server.
    }
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        Debug.Log($"{Name()}: xxxjack ClientManager_OnClientConnectionState: state={obj.ConnectionState}");
        _clientState = obj.ConnectionState;
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        Debug.Log($"{Name()}: xxxjack ServerManager_OnServerConnectionState: state={obj.ConnectionState}");
        _serverState = obj.ConnectionState;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
