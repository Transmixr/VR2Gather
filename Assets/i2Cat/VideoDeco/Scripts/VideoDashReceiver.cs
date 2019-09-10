﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoDashReceiver : MonoBehaviour
{
    Workers.BaseWorker reader;
    Workers.BaseWorker codec;
    Workers.BaseWorker preparer;
    public string url;

    private void Start() {
        Init(url);
    }

    // Start is called before the first frame update
    public void Init(string url) {
        Debug.Log($"Config.Instance.memoryDamping {Config.Instance.memoryDamping}");
        try {
            reader = new Workers.SUBReader(url);
            codec = new Workers.VideoDecoder();
            preparer = new Workers.VideoPreparer();
            reader.AddNext(codec).AddNext(preparer).AddNext(reader);
            reader.token = new Workers.Token();
        }
        catch (System.Exception e) {
            Debug.Log($">>ERROR {e}");
        }
    }

    void OnDestroy() {
        reader?.Stop();
        codec?.Stop();
        preparer?.Stop();
    }
}