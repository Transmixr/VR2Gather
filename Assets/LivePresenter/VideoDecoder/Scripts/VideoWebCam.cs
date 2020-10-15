﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;
using System;
using System.Threading;
using UnityEngine.Networking.NetworkSystem;

public class VideoWebCam : MonoBehaviour {
    public Renderer rendererOrg;
    public Renderer rendererDst;

    Workers.WebCamReader    recorder;
    Workers.VideoEncoder    encoder;
    Workers.BaseWorker      writer;
    Workers.BaseWorker      reader;

    Workers.VideoDecoder    decoder;
    Workers.VideoPreparer   preparer;

    QueueThreadSafe         videoDataQueue = new QueueThreadSafe();
    QueueThreadSafe         writerQueue = new QueueThreadSafe();
    QueueThreadSafe         videoCodecQueue = new QueueThreadSafe();
    QueueThreadSafe videoPreparerQueue = new QueueThreadSafe(5);

    Texture2D       texture;
    public int      width = 1280;
    public int      height = 720;
    public int      fps = 12;
    bool            ready = false;

    public bool     useDash = false;

    private IEnumerator Start() {
        ready = false;
        while (OrchestratorController.Instance==null || OrchestratorController.Instance.MySession==null) yield return null;

        Init(null);

        rendererOrg.material.mainTexture = recorder.webcamTexture;
        rendererOrg.transform.localScale = new Vector3(1, 1, recorder.webcamTexture.height / (float)recorder.webcamTexture.width);
    }

    // Start is called before the first frame update
    public void Init(string deviceName) {
        string remoteURL = OrchestratorController.Instance.SelfUser.sfuData.url_gen;
        string remoteStream = "webcam";
        try {
            recorder = new Workers.WebCamReader(deviceName, width, height, fps, this, videoDataQueue);
            encoder  = new Workers.VideoEncoder(videoDataQueue, null, writerQueue, null);
            Workers.B2DWriter.DashStreamDescription[] b2dStreams = new Workers.B2DWriter.DashStreamDescription[1] {
                new Workers.B2DWriter.DashStreamDescription() {
                    tileNumber = 0,
                    quality = 0,
                    inQueue = writerQueue
                }
            };
            if(useDash) writer = new Workers.B2DWriter(remoteURL, remoteStream, "wcss", 2000, 10000, b2dStreams);
            else writer = new Workers.SocketIOWriter(OrchestratorController.Instance.SelfUser, remoteStream, b2dStreams);

            if (useDash) reader = new Workers.BaseSubReader(remoteURL, remoteStream, 1, 0, videoCodecQueue);
            else reader = new Workers.SocketIOReader(OrchestratorController.Instance.SelfUser, remoteStream, videoCodecQueue);

            decoder = new Workers.VideoDecoder(videoCodecQueue, null, videoPreparerQueue, null);
            preparer = new Workers.VideoPreparer(videoPreparerQueue, null);
        }
        catch (System.Exception e) {
            Debug.Log($"VideoWebCam.Init: Exception: {e.Message}");
            throw e;
        }
        ready = true;
    }
    float timeToFrame = 0;
    void Update() {
        if (ready) {
            lock (preparer) {
                if (preparer.availableVideo > 0) {
                    if (texture == null) {
                        texture = new Texture2D(decoder.Width, decoder.Height, TextureFormat.RGB24, false, true);
                        rendererDst.material.mainTexture = texture;
                        rendererDst.transform.localScale = new Vector3(1, 1, decoder.Height / (float)decoder.Width);
                    }
                    texture.LoadRawTextureData(preparer.GetVideoPointer(preparer.videFrameSize), preparer.videFrameSize);
                    texture.Apply();
                }
            }
        }
    }

    void OnDestroy() {
        Debug.Log("VideoDashReceiver: OnDestroy");
        encoder?.StopAndWait();
        recorder?.StopAndWait();
        decoder?.StopAndWait();
        preparer?.StopAndWait();

        Debug.Log($"VideoDashReceiver: Queues references counting: videoCodecQueue {videoCodecQueue._Count} videoPreparerQueue {videoPreparerQueue._Count} ");
        BaseMemoryChunkReferences.ShowTotalRefCount();
    }
}
