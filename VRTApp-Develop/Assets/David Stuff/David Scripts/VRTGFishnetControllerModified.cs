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

public class VRTGFishnetControllerModified : NetworkIdBehaviour
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

    private FishnetStartupData _localStartupData;

    [SerializeField] private float startUpTimeDelayInSeconds = 3.0f;
    protected override void Awake()
    {
        base.Awake();
        OrchestratorController.Instance.RegisterEventType(MessageTypeID.TID_FishnetStartupData, typeof(FishnetStartupData));
        if (_networkManager == null)
        {
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
        OrchestratorController.Instance.Subscribe<FishnetStartupData>(RecieveServerData);
    }


    public virtual void OnDisable()
    {
        OrchestratorController.Instance.Unsubscribe<FishnetStartupData>(RecieveServerData);
        if (_clientState != LocalConnectionState.Stopped)
        {
            Debug.Log($"{Name()}: Stopping client");
            _networkManager.ClientManager.StopConnection();
        }
        if (_serverState != LocalConnectionState.Stopped)
        {
            Debug.Log($"{Name()}: Stopping server");
            _networkManager.ServerManager.StopConnection(true);
        }
    }

    //// Start is called before the first frame update
    //void Start()
    //{
    //    Debug.Log($"{Name()}: Starting VRTFishnetController");

    //    if (OrchestratorController.Instance.UserIsMaster)
    //    {
    //        Debug.Log($"{Name()}: Firing Startup Coroutine");

    //        StartCoroutine("FishnetStartup");
    //    }
    //}


    public string Name()
    {
        return "VRTFishnetController";
    }

    //private IEnumerator FishnetStartup()
    //{
    //    Debug.Log($"{Name()}: Coroutine started, T-minus 5 seconds");
    //    yield return new WaitForSecondsRealtime(startUpTimeDelayInSeconds);

    //    StartFishnetServer();

    //    yield return new WaitForSecondsRealtime(2.0f);

    //    BroadcastFishnetServerAddress();
    //}

    public void StartFishnetServer()
    {
        if (_serverState != LocalConnectionState.Started)
        {
            hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                Debug.LogWarning($"{Name()}: No IP address for hostName {hostName}");
            }
            else
            {
                if (addresses.Length > 1)
                {
                    Debug.LogWarning($"{Name()}: Multiple IP addresses ({addresses.Length}) for {hostName}, using first one");
                }
                hostName = addresses[0].ToString();
                Debug.Log($"{Name()}: Using IP address {hostName}");
            }
            Debug.Log($"{Name()}: Starting Fishnet server on VR2Gather master. host={hostName}");
            _networkManager.ServerManager.StartConnection();
        }

        BroadcastFishnetServerAddress();
    }

    public void BroadcastFishnetServerAddress()
    {
        // xxxjack This will only be run on the master. Use a VR2Gather Orchestrator message to have all session participants call RecieveServerData.
        // xxxjack maybe we ourselves (the master) have to call it also, need to check.
        FishnetStartupData serverData = new()
        {
            serverHost = hostName,
            serverPort = 7770
        };
        OrchestratorController.Instance.SendTypeEventToAll(serverData);
        RecieveServerData(serverData);
        StartClient();
    }

    void RecieveServerData(FishnetStartupData server)
    {
        _localStartupData = server;
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        Debug.Log($"{Name()}: xxxjack ClientManager_OnClientConnectionState: state={obj.ConnectionState}");
        _clientState = obj.ConnectionState;
    }

    public void StartClient()
    {
        // xxxjack this is going to need at least one argument (the address of the Fishnet server)
        Debug.Log($"{Name()}: Starting Fishnet client, host={_localStartupData.serverHost}, port={_localStartupData.serverPort}");
        if (_clientState == LocalConnectionState.Stopped)
        {
            _networkManager.ClientManager.StartConnection(_localStartupData.serverHost, _localStartupData.serverPort);
        }
        // xxxjack this should create the connection to the server.
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        Debug.Log($"{Name()}: xxxjack ServerManager_OnServerConnectionState: state={obj.ConnectionState}");
        _serverState = obj.ConnectionState;
    }

}
