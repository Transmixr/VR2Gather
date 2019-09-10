﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrchestrationController : PilotController {

    public override void Start() {
        base.Start();

        test.controller = this;
    }

    public override void MessageActivation(string msg) {
        if (msg == MessageType.START) {
            if (test.isMaster && !test.isDebug) SceneManager.LoadScene("Sample Scenario 2");
            else SceneManager.LoadScene(test.scenarioIdText.text);
        }
        else if (msg == MessageType.READY) {
            // Do something to check if all the users are ready (future implementation)
        }
    }
}