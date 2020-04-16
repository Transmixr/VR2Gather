﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Workers {
    public class B2DWriter : BaseWorker {
        bin2dash.connection uploader;
        //BinaryWriter bw;
        string url;
        QueueThreadSafe inQueue;

        public B2DWriter(Config._User._PCSelfConfig._Bin2Dash cfg, string _url, QueueThreadSafe _inQueue) : base(WorkerType.End) {
            try {
                inQueue = _inQueue;
                //if (cfg.fileMirroring) bw = new BinaryWriter(new FileStream($"{Application.dataPath}/../{cfg.streamName}.dashdump", FileMode.Create));
                if ( string.IsNullOrEmpty(_url))
                    url = cfg.url;
                else
                    url = _url;
                uploader = bin2dash.create(cfg.streamName, bin2dash.VRT_4CC('c', 'w', 'i', '1'), url, cfg.segmentSize, cfg.segmentLife);
                if (uploader != null) {
//                    Debug.Log($"Bin2Dash vrt_create(url={url + cfg.streamName}.mpd)");
                    Start();
                }
                else
                    throw new System.Exception($"B2DWriter: vrt_create: failed to create uploader {url + cfg.streamName}.mpd");
            }
            catch (System.Exception e) {
                Debug.LogError($"B2DWriter({url}:{e.Message}");
                throw e;
            }
        }

        public override void OnStop() {
            uploader.free();
            uploader = null;
            //bw?.Close();
            base.OnStop();
            Debug.Log($"B2DWriter {url} Stopped");
        }

        protected override void Update() {
            base.Update();
            if (inQueue.Count>0 ) {
                NativeMemoryChunk mc = (NativeMemoryChunk)inQueue.Dequeue();
                statsUpdate((int)mc.length);
                if (!uploader.push_buffer(mc.pointer, (uint)mc.length))
                    Debug.Log($"B2DWriter {url}: ERROR sending data");
                mc.free();
            }
        }

        System.DateTime statsLastTime;
        double statsTotalBytes;
        double statsTotalPackets;

        public void statsUpdate(int nBytes) {
            if (statsLastTime == null) {
                statsLastTime = System.DateTime.Now;
                statsTotalBytes = 0;
                statsTotalPackets = 0;
            }
            if (System.DateTime.Now > statsLastTime + System.TimeSpan.FromSeconds(10)) {
                Debug.Log($"stats: ts={(int)System.DateTime.Now.TimeOfDay.TotalSeconds}: B2DWriter: {statsTotalPackets / 10} fps, {(int)(statsTotalBytes / statsTotalPackets)} bytes per packet");
                statsTotalBytes = 0;
                statsTotalPackets = 0;
                statsLastTime = System.DateTime.Now;
            }
            statsTotalBytes += nBytes;
            statsTotalPackets += 1;
        }
    }
}
