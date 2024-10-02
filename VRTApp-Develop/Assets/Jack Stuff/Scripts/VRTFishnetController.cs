using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRT.Orchestrator.Wrapping;

public class VRTFishnetController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (OrchestratorController.Instance.UserIsMaster) {
            StartFishnetServer();
            BroadcastFishnetServerAddress();
        }
    }

    // xxxjack we need Fishnet teardown as well (on end-of-scene, or maybe end-of-application)

    public string Name() {
        return "VRTFishnetController";
    }

    void StartFishnetServer() {
        Debug.Log($"{Name()}: Starting Fishnet server on VR2Gather master");
        Debug.Log($"{Name()}: xxxjack StartFishnetServer() not implemented yet.");
    }

    void BroadcastFishnetServerAddress() {
        Debug.Log($"{Name()}: xxxjack BroadcastFishnetServerAddress() not implemented yet.");
        // xxxjack This will only be run on the master. Use a VR2Gather Orchestrator message to have all session participants call StartFishnetClient.
        // xxxjack maybe we ourselves (the master) have to call it also, need to check.
        StartFishnetClient();
    }

    void StartFishnetClient() {
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
