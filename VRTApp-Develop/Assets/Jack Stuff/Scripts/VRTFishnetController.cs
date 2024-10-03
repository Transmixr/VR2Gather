using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRT.Orchestrator.Wrapping;
using VRT.Pilots.Common;

public class VRTFishnetController : NetworkIdBehaviour
{
    public class FishnetStartupData : BaseMessage
    {
        public string serverHost;
        public int serverPort;
    }

    protected override void Awake()
    {
        base.Awake();
        OrchestratorController.Instance.RegisterEventType(MessageTypeID.TID_FishnetStartupData, typeof(FishnetStartupData));

    }
    public virtual void OnEnable()
    {
        OrchestratorController.Instance.Subscribe<FishnetStartupData>(StartFishnetClient);
    }


    public virtual void OnDisable()
    {
        OrchestratorController.Instance.Unsubscribe<FishnetStartupData>(StartFishnetClient);
        // xxxjack we need Fishnet teardown as well (on end-of-scene, or maybe end-of-application)
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
        Debug.Log($"{Name()}: Starting Fishnet server on VR2Gather master");
        Debug.Log($"{Name()}: xxxjack StartFishnetServer() not implemented yet.");
        
    }

    void BroadcastFishnetServerAddress() {
        // xxxjack This will only be run on the master. Use a VR2Gather Orchestrator message to have all session participants call StartFishnetClient.
        // xxxjack maybe we ourselves (the master) have to call it also, need to check.
        FishnetStartupData serverData = new() {
            serverHost="localhost",
            serverPort=7770
        };
        OrchestratorController.Instance.SendTypeEventToAll(serverData);
        StartFishnetClient(serverData);
    }

    void StartFishnetClient(FishnetStartupData server) {
        // xxxjack this is going to need at least one argument (the address of the Fishnet server)
        Debug.Log($"{Name()}: Starting Fishnet client");
        Debug.Log($"{Name()}: xxxjack StartFishnetClient() not implemented yet.");
        // xxxjack this should create the connection to the server.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
