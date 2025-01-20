using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTest : MonoBehaviour
{
    public void ChangeScene()
    {
        if(!InstanceFinder.IsServerStarted)
        return;

        var unload = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        SceneUnloadData sud = new SceneUnloadData(unload);
        SceneLoadData sld = new SceneLoadData("TechnicalPlayground2");

        Debug.Log($"Unloading {unload}");
        InstanceFinder.SceneManager.UnloadGlobalScenes(sud);
        Debug.Log($"Unloaded {unload}");

        Debug.Log($"Loading Technical Playground 2");
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        Debug.Log($"Loaded Technical Playground 2");
    }
}
