﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVoiceDashSender : MonoBehaviour
{
    public int userID;
    Workers.BaseWorker reader;
    Workers.BaseWorker codec;
    Workers.BaseWorker writer;


    // Start is called before the first frame update
    void Start() {
        codec = new Workers.VoiceEncoder();
        reader = new Workers.VoiceReader(this, ((Workers.VoiceEncoder)codec).bufferSize);

        writer = new Workers.B2DWriter(Config.Instance.Users[userID-1].PCSelfConfig.AudioBin2Dash);

        reader.AddNext(codec).AddNext(writer).AddNext(reader);
        reader.token = new Workers.Token(1);

    }

    void OnDestroy() {
        reader?.Stop();
        codec?.Stop();
        writer?.Stop();
    }
}
