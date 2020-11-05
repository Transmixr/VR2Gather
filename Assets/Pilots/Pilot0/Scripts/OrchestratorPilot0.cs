﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OrchestratorWrapping;

public class OrchestratorPilot0 : MonoBehaviour {
    public static OrchestratorPilot0 Instance { get; private set; }

    #region GUI components

    [SerializeField] private Button exitButton = null;

    #endregion

    #region Unity

    // Start is called before the first frame update
    void Start() {
        if (Instance == null) {
            Instance = this;
        }
        // Buttons listeners
        exitButton.onClick.AddListener(delegate { LeaveButton(); });

        InitialiseControllerEvents();
    }

    private void OnDestroy() {
        TerminateControllerEvents();
    }

    #endregion

    #region Buttons

    public void LeaveButton() {
        LeaveSession();
    }

    #endregion

    #region Events listeners

    // Subscribe to Orchestrator Wrapper Events
    private void InitialiseControllerEvents() {
        OrchestratorController.Instance.OnGetSessionInfoEvent += OnGetSessionInfoHandler;
        OrchestratorController.Instance.OnLeaveSessionEvent += OnLeaveSessionHandler;
        OrchestratorController.Instance.OnUserJoinSessionEvent += OnUserJoinedSessionHandler;
        OrchestratorController.Instance.OnUserLeaveSessionEvent += OnUserLeftSessionHandler;
        OrchestratorController.Instance.OnErrorEvent += OnErrorHandler;

        OrchestratorController.Instance.RegisterMessageForwarder();
    }

    // Un-Subscribe to Orchestrator Wrapper Events
    private void TerminateControllerEvents() {
        OrchestratorController.Instance.OnGetSessionInfoEvent -= OnGetSessionInfoHandler;
        OrchestratorController.Instance.OnLeaveSessionEvent -= OnLeaveSessionHandler;
        OrchestratorController.Instance.OnUserJoinSessionEvent -= OnUserJoinedSessionHandler;
        OrchestratorController.Instance.OnUserLeaveSessionEvent -= OnUserLeftSessionHandler;
        OrchestratorController.Instance.OnErrorEvent -= OnErrorHandler;

        OrchestratorController.Instance.UnregisterMessageForwarder();
    }

    #endregion

    #region Commands

    #region Sessions
    
    private void OnGetSessionInfoHandler(Session session) {
        if (session != null) {
            Debug.Log(  "Now the master is " + OrchestratorController.Instance.GetMasterUser(session.sessionMaster).userName + 
                        " and the session has " + session.sessionUsers.Length  +" players.");
        } else {
        }
    }

    private void LeaveSession() {
        OrchestratorController.Instance.LeaveSession();
    }

    private void OnLeaveSessionHandler() {
        Debug.Log("[OrchestratorPilot0][OnLeaveSessionHandler] Session Leaved");
        SceneManager.LoadScene("LoginManager");
    }

    private void OnUserJoinedSessionHandler(string userID) {
        if (!string.IsNullOrEmpty(userID)) {
            Debug.Log("[OrchestratorPilot0][OnUserJoinedSessionHandler] User joined: " + userID);
        }
    }

    private void OnUserLeftSessionHandler(string userID) {
        if (!string.IsNullOrEmpty(userID)) {
            Debug.Log("[OrchestratorPilot0][OnUserLeftSessionHandler] User left: " + userID);
        }
    }

    #endregion

    #region Errors

    private void OnErrorHandler(ResponseStatus status) {
        Debug.Log("[OrchestratorPilot0][OnError]::Error code: " + status.Error + "::Error message: " + status.Message);
        ErrorManager.Instance.EnqueueOrchestratorError(status.Error, status.Message);
    }

    #endregion

    #endregion

#if UNITY_STANDALONE_WIN
    void OnGUI() {
        if (GUI.Button(new Rect(Screen.width / 2, 5, 70, 20), "Open Log")) {
            var log_path = System.IO.Path.Combine(System.IO.Directory.GetParent(Environment.GetEnvironmentVariable("AppData")).ToString(), "LocalLow", Application.companyName, Application.productName, "Player.log");
            Debug.Log(log_path);
            Application.OpenURL(log_path);
        }
    }
#endif
}
