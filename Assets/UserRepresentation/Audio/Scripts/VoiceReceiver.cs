﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Workers;

public class VoiceReceiver : MonoBehaviour {
    Workers.BaseReader      reader;
    Workers.BaseWorker      codec;
    Workers.AudioPreparer   preparer;

    // xxxjack nothing is dropped here. Need to investigate what is the best idea.
    QueueThreadSafe decoderQueue;
    QueueThreadSafe preparerQueue;

    // Start is called before the first frame update
    public void Init(OrchestratorWrapping.User user, string _streamName, int _streamNumber, int _initialDelay, bool UseDash) {
        VoiceReader.PrepareDSP();
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 4f;
        audioSource.maxDistance = 100f;
        audioSource.loop = true;
        audioSource.Play();

        preparerQueue = new QueueThreadSafe("VoiceReceiverPreparer", 4, false);

        if (UseDash) {
            decoderQueue = new QueueThreadSafe("VoiceReceiverDecoder", 32, true);
            reader = new Workers.BaseSubReader(user.sfuData.url_audio, _streamName, _initialDelay, 0, decoderQueue);
        } else {
            decoderQueue = new QueueThreadSafe("VoiceReceiverDecoder", 4, true);
            reader = new Workers.SocketIOReader(user, _streamName, decoderQueue);
        }

        codec = new Workers.VoiceDecoder(decoderQueue, preparerQueue);
        preparer = new Workers.AudioPreparer(preparerQueue);//, optimalAudioBufferSize);
    }

    void OnDestroy() {
        reader?.StopAndWait();
        codec?.StopAndWait();
        preparer?.StopAndWait();
    }
    /*
    void OnAudioRead(float[] data) {
        if (preparer == null || !preparer.GetAudioBuffer(data, data.Length))
            System.Array.Clear(data, 0, data.Length);
    }
*/

    float[] tmpBuffer;
    void OnAudioFilterRead(float[] data, int channels) {
        if (tmpBuffer == null) tmpBuffer = new float[data.Length];
        if (preparer != null && preparer.GetAudioBuffer(tmpBuffer, tmpBuffer.Length)) {
            int cnt = 0;
            do {
                data[cnt] += tmpBuffer[cnt];
            } while (++cnt < data.Length);
        }
    }

    public void SetSyncInfo(SyncConfig.ClockCorrespondence _clockCorrespondence) {
        reader.SetSyncInfo(_clockCorrespondence);
    }


}
