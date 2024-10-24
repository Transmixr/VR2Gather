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
using UnityEditor.VersionControl;

public class VRTFishnetController : NetworkIdBehaviour
{
    public class FishnetStartupData : BaseMessage
    {
        public string serverHost;
        public ushort serverPort;
    };

    public class FishnetMessage : BaseMessage
    {
        public bool toServer;
        public byte channelId;
        public byte[] fishnetPayload;
    };

    [SerializeField]
    private string hostName;

    [SerializeField]
    private NetworkManager _networkManager;
    [SerializeField]
    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
   
    [SerializeField]
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;

    [SerializeField] private float startUpTimeDelayInSeconds = 3.0f;

    Queue<FishnetMessage> incomingMessages = new();

    public bool debug;

    protected override void Awake()
    {
        base.Awake();
        OrchestratorController.Instance.RegisterEventType(MessageTypeID.TID_FishnetStartupData, typeof(FishnetStartupData));
        OrchestratorController.Instance.RegisterEventType(MessageTypeID.TID_FishnetMessage, typeof(FishnetMessage));
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

    new void OnDestroy()
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
        OrchestratorController.Instance.Subscribe<FishnetMessage>(FishnetMessageReceived);
    }


    public virtual void OnDisable()
    {
        OrchestratorController.Instance.Unsubscribe<FishnetStartupData>(StartFishnetClient);
        if (_clientState != LocalConnectionState.Stopped) {
            if (debug) Debug.Log($"{Name()}: Stopping client");
            _networkManager.ClientManager.StopConnection();
        }
        if (_serverState != LocalConnectionState.Stopped) {
            if (debug) Debug.Log($"{Name()}: Stopping server");
            _networkManager.ServerManager.StopConnection(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (debug) Debug.Log($"{Name()}: Starting VRTFishnetController");

        if (OrchestratorController.Instance.UserIsMaster) {
            if (debug) Debug.Log($"{Name()}: Firing Startup Coroutine");

            StartCoroutine("FishnetStartup");
        }
    }

    
    public string Name() {
        return "VRTFishnetController";
    }

    private IEnumerator FishnetStartup()
    {
        if (debug) Debug.Log($"{Name()}: Coroutine started, T-minus 5 seconds");
        yield return new WaitForSecondsRealtime(startUpTimeDelayInSeconds);

        StartFishnetServer();

        yield return new WaitForSecondsRealtime(2.0f);

        BroadcastFishnetServerAddress();  
    }

    void StartFishnetServer() {
        if (_serverState != LocalConnectionState.Started) {
            hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            //
            //TODO: Search for an IPv4 adress instead of an IPv6
            //
            //if (addresses.Length == 0) {
            //    Debug.LogWarning($"{Name()}: No IP address for hostName {hostName}");
            //} else {
            //    if (addresses.Length > 1) {
            //        Debug.LogWarning($"{Name()}: Multiple IP addresses ({addresses.Length}) for {hostName}, using first one");
            //    }
            //    hostName = addresses[0].ToString();
            //    Debug.Log($"{Name()}: Using IP address {hostName}");
            //}
            hostName = "192.168.219.229";
            if (debug) Debug.Log($"{Name()}: Starting Fishnet server on VR2Gather master. host={hostName}");
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
        if (debug) Debug.Log($"{Name()}: Starting Fishnet client, host={server.serverHost}, port={server.serverPort}");
        if (_clientState == LocalConnectionState.Stopped) {
            _networkManager.ClientManager.StartConnection(server.serverHost, server.serverPort);
        }
        // xxxjack this should create the connection to the server.
    }
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        if (debug) Debug.Log($"{Name()}: xxxjack ClientManager_OnClientConnectionState: state={obj.ConnectionState}");
        _clientState = obj.ConnectionState;
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        if(obj.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.LogError($"{Name()}: ServerManager_OnServerConnectionState: state={obj.ConnectionState}");
        }
        else
        {
            if (debug) Debug.Log($"{Name()}: ServerManager_OnServerConnectionState: state={obj.ConnectionState}");
        }
        
        _serverState = obj.ConnectionState;
    }
    
    void FishnetMessageReceived(FishnetMessage message) 
    {
        // xxxjack add to queue
        if (debug) Debug.Log($"{Name()}: FishnetMessageReceived({message.channelId}, {message.fishnetPayload.Length} bytes)");
        incomingMessages.Enqueue(message);
    }

    public void SendToServer(byte channelId, ArraySegment<byte> segment)
    {
        FishnetMessage message = new() {
            toServer = true,
            channelId = channelId,
            fishnetPayload = segment.ToArray()
        };
        if (debug) Debug.Log($"{Name()}: SendToServer({channelId}, {message.fishnetPayload.Length} bytes)");
        OrchestratorController.Instance.SendTypeEventToMaster<FishnetMessage>(message);
    }
       
    public void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
    {
        FishnetMessage message = new() {
            toServer = false,
            channelId = channelId,
            fishnetPayload = segment.ToArray()
        };
        if (debug) Debug.Log($"{Name()}: SendToClient({channelId}, {message.fishnetPayload.Length} bytes, {connectionId})");
        string userId = "blabla"; // xxxjack need to map connectionId to userId
        OrchestratorController.Instance.SendTypeEventToUser<FishnetMessage>(userId, message);
    }

    public bool IterateIncoming(VRTFishnetTransport transport) {
        // xxxjack process all messages in the queue
        FishnetMessage message;
        while (incomingMessages.TryDequeue(out message)) {
            if (debug) Debug.Log($"{Name()}: IterateIncoming: forward message to {transport.Name()}");
            transport.HandleDataReceivedViaOrchestrator(message.toServer, message.channelId, message.fishnetPayload);     
        }
        return true;
    }
}
