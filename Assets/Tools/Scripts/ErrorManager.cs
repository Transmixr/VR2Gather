﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour {

    public GameObject myPrefab;

    List<string[]> queue = new List<string[]>();
    private object thisLock = new object();

    // Start is called before the first frame update
    void Start() {
        Application.logMessageReceived += Application_logMessageReceived;
    }

    private void Update() {
        lock (thisLock) {
            if (queue.Count > 0) {
                foreach (string[] error in queue) {
                    bool instantiate = true;
                    ErrorPopup[] prevErrors = gameObject.GetComponentsInChildren<ErrorPopup>();
                    foreach (ErrorPopup item in prevErrors) {
                        if (item.ErrorMessage == error[1])
                            instantiate = false;
                    }
                    if (instantiate) {
                        GameObject popup = Instantiate(myPrefab, gameObject.transform);
                        popup.GetComponent<ErrorPopup>().FillError(error[0], error[1]);
                    }
                }
                queue.Clear();
            }
        }
    }

    private void OnDestroy() {
        Application.logMessageReceived -= Application_logMessageReceived;
    }

    private void Application_logMessageReceived(string condition, string stackTrace, LogType type) {
        string[] error = { "", ""};
        error[1] = condition;
        if (type == LogType.Exception) {
            error[0] = "Exception";
            lock (thisLock) {
                queue.Add(error);
            }
        }
        else if (type == LogType.Error) {
            error[0] = "Error";
            lock (thisLock) {
                queue.Add(error);
            }
        }
    }
}
