﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using VRT.Core;

namespace VRT.UserRepresentation.Voice
{
    public class VoiceReader : BaseWorker
    {
        //
        // For debugging we can add a 440Hz tone to the microphone signal by setting this
        // value to true.
        //
        const bool debugReplaceByTone = false;
        const bool debugAddTone = false;
        ToneGenerator debugToneGenerator = null;

        Coroutine coroutine;
        QueueThreadSafe outQueue;

        public VoiceReader(string deviceName, int sampleRate, int fps, int minBufferSize, MonoBehaviour monoBehaviour, QueueThreadSafe _outQueue) : base()
        {
            stats = new Stats(Name());
            outQueue = _outQueue;
            device = deviceName;
            coroutine = monoBehaviour.StartCoroutine(MicroRecorder(deviceName, sampleRate, fps, minBufferSize));
            Debug.Log($"{Name()}: Started bufferLength {nSamplesPerPacket}.");
            Start();
        }

        public int getBufferSize()
        {
            return nSamplesPerPacket;
        }

        long sampleTimestamp(int nSamplesInInputBuffer)
        {
            System.TimeSpan sinceEpoch = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1);
            double timestamp = sinceEpoch.TotalMilliseconds;
            timestamp -= (1000 * nSamplesInInputBuffer / wantedSampleRate);
            return (long)timestamp;
        }
        protected override void Update()
        {
            base.Update();
        }

        public override void OnStop()
        {
            base.OnStop();
            Debug.Log($"{Name()}: Stopped microphone {device}.");
            outQueue.Close();
        }

        string device;
        int nSamplesInCircularBuffer;
        int nSamplesPerPacket;
        AudioClip recorder;

        int wantedSampleRate;
        
        public static void PrepareDSP(int _sampleRate, int _bufferSize)
        {
            var ac = AudioSettings.GetConfiguration();
            if (_sampleRate == 0) _sampleRate = ac.sampleRate;
            if (_bufferSize != 0) _bufferSize = ac.dspBufferSize;

            if (_sampleRate != ac.sampleRate || _bufferSize != ac.dspBufferSize)
            {
                ac.sampleRate = _sampleRate;
                ac.dspBufferSize = _bufferSize;
                AudioSettings.Reset(ac);
                ac = AudioSettings.GetConfiguration();
                if (ac.sampleRate != _sampleRate && _sampleRate != 0)
                {
                    Debug.LogError($"Audio output sample rate is {ac.sampleRate} in stead of {_sampleRate}. Other participants may sound funny.");
                }
                if (ac.dspBufferSize != _bufferSize && _bufferSize != 0)
                {
                    Debug.LogWarning($"PrepareDSP: audio output buffer is {ac.dspBufferSize} in stead of {_bufferSize}");
                }
              
            }

        }

        IEnumerator MicroRecorder(string deviceName, int _sampleRate, int _fps, int _minBufferSize)
        {
            if (debugAddTone || debugReplaceByTone)
            {
                debugToneGenerator = new ToneGenerator();
                Debug.LogWarning($"{Name()}: Adding 440Hz tone to microphone signal");
            }
            wantedSampleRate = _sampleRate;
            nSamplesPerPacket = wantedSampleRate / _fps;
            if (_minBufferSize > 0 && nSamplesPerPacket % _minBufferSize != 0)
            {
                // Round up to a multiple of _minBufferSize
                nSamplesPerPacket = ((nSamplesPerPacket + _minBufferSize - 1) / _minBufferSize) * _minBufferSize;
                float actualFps = (float)wantedSampleRate / nSamplesPerPacket;
                Debug.LogWarning($"{Name()}: adapted bufferSize={nSamplesPerPacket}, fps={actualFps}");
            }
            if (wantedSampleRate % nSamplesPerPacket != 0)
            {
                Debug.LogWarning($"{Name()}: non-integral number of buffers per second. This may not work.");
            }
            PrepareDSP(wantedSampleRate, nSamplesPerPacket);
            if (Microphone.devices.Length > 0)
            {
                if (deviceName == null || deviceName == "") deviceName = Microphone.devices[0];
                int currentMinFreq;
                int currentMaxFreq;
                Microphone.GetDeviceCaps(deviceName, out currentMinFreq, out currentMaxFreq);
                // We record a looping clip of 1 second.
                const int recorderBufferDuration = 1; 
                recorder = Microphone.Start(deviceName, true, recorderBufferDuration, currentMaxFreq);
                nSamplesInCircularBuffer = recorder.samples * recorder.channels;
                // We expect the recorder clip to contain an integral number of
                // buffers, because we are going to use it as a circular buffer.
                if (nSamplesInCircularBuffer % nSamplesPerPacket != 0)
                {
                    Debug.LogError($"VoiceReader: Incorrect clip size {nSamplesInCircularBuffer} for buffer size {nSamplesPerPacket}");
                }
                if (recorder.channels != 1)
                {
                    Debug.LogWarning("{Name()}: Microphone has {recorder.channels} channels, not supported");
                }
#if WITH_SAMPLERATE_ADAPTER
                float inc = 1; // was: recorderBufferSize / 16000f;
                int nInputSamplesNeededPerPacket = (int)(nSamplesPerPacket * inc);
#else
                int nInputSamplesNeededPerPacket = nSamplesPerPacket;
#endif
                float[] readBuffer = new float[nInputSamplesNeededPerPacket];
                Debug.Log($"{Name()}: Using {deviceName}  Channels {recorder.channels} Frequency {nSamplesInCircularBuffer} bufferLength {nSamplesPerPacket} IsRecording {Microphone.IsRecording(deviceName)}");

                int readPosition = 0;

                while (true)
                {
                    if (Microphone.IsRecording(deviceName))
                    {
                        int writePosition = Microphone.GetPosition(deviceName);
                        int available;
                        if (writePosition < readPosition) available = nSamplesInCircularBuffer - readPosition + writePosition;
                        else available = writePosition - readPosition;
                        while (available >= nInputSamplesNeededPerPacket)
                        {
                            if (!recorder.GetData(readBuffer, readPosition))
                            {
                                Debug.Log($"{Name()}: ERROR!!! IsRecording {Microphone.IsRecording(deviceName)}");
                                Debug.LogError("Error while getting audio from microphone");
                            }
                            // Write all data from microphone.
                            lock (outQueue)
                            {
                                FloatMemoryChunk mc = new FloatMemoryChunk(nSamplesPerPacket);
                                _copyTo(readBuffer, mc.buffer);
                                if (debugAddTone || debugReplaceByTone)
                                {
                                    if (debugReplaceByTone)
                                    {
                                        for (int i = 0; i < mc.buffer.Length; i++) mc.buffer[i] = 0;
                                    }
                                    debugToneGenerator.addTone(mc.buffer);
                                }
                                readPosition = (readPosition + nSamplesPerPacket) % nSamplesInCircularBuffer;
                                available -= nSamplesPerPacket;
                                mc.info = new FrameInfo();
                                // We need to compute timestamp of this audio frame
                                // by using system clock and adjusting with "available".
                                mc.info.timestamp = sampleTimestamp(available);
                                double timeRemainingInBuffer = (double)available / wantedSampleRate;
                                ToneGenerator.checkToneBuffer("VoiceReader.outQueue.mc", mc.buffer);
                                bool ok = outQueue.Enqueue(mc);
                                stats.statsUpdate(timeRemainingInBuffer, nSamplesPerPacket, !ok, outQueue.QueuedDuration());
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"{Name()}: microphone {deviceName} stopped recording, starting again.");
                        recorder = Microphone.Start(deviceName, true, recorderBufferDuration, currentMaxFreq);
                        readPosition = 0;
                    }
                    yield return null;
                }

                void _copyTo(float[] inBuffer, float[] outBuffer)
                {
#if WITH_SAMPLERATE_ADAPTER
                    float idx = 0;
                    for (int i = 0; i < bufferLength; i++)
                    {
                        outBuffer[i] = inBuffer[(int)idx];
                        idx += inc;
                    }
#else
                    for (int i = 0; i < nSamplesPerPacket; i++)
                    {
                        outBuffer[i] = inBuffer[i];
                    }
#endif
                }
            }
            else
                Debug.LogError("{Name()}: No Microphones detected.");
        }

        protected class Stats : VRT.Core.BaseStats
        {
            public Stats(string name) : base(name) { }

            double statsTotalUpdates;
            double statsTotalSamples;
            double statsTotalTimeInInputBuffer;
            double statsTotalQueuedDuration;
            double statsDrops;

            public void statsUpdate(double timeInInputBuffer, int sampleCount, bool dropped, ulong queuedDuration)
            {

                statsTotalUpdates += 1;
                statsTotalSamples += sampleCount;
                statsTotalTimeInInputBuffer += timeInInputBuffer;
                statsTotalQueuedDuration += queuedDuration;
                if (dropped) statsDrops++;

                if (ShouldOutput())
                {
                    Output($"fps={statsTotalUpdates / Interval():F3}, record_latency_ms={(int)(statsTotalTimeInInputBuffer * 1000 / statsTotalUpdates)}, output_queue_ms={(int)(statsTotalQueuedDuration / statsTotalUpdates)}, fps_dropped={statsDrops / Interval()}, samples_per_frame={(int)(statsTotalSamples/statsTotalUpdates)}");
                }
                if (ShouldClear())
                {
                    Clear();
                    statsTotalUpdates = 0;
                    statsTotalSamples = 0;
                    statsTotalTimeInInputBuffer = 0;
                    statsTotalQueuedDuration = 0;
                    statsDrops = 0;
                }
            }
        }

        protected Stats stats;
    }

    public class ToneGenerator
    {
        float position = 0;
        const float factor = 0.2f;
        const float sampleFrequency = 48000f;
        const float toneFrequency = 440f;

        public ToneGenerator() { }

        public void addTone(float[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] += Mathf.Sin(position) * factor;
                position += 2 * Mathf.PI / (sampleFrequency / toneFrequency);
            }
        }

        public static void checkToneBuffer(string name, float[] buffer)
        {
            float maxValue = Math.Abs(buffer[0]);
            int maxIndex = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (Math.Abs(buffer[i]) > maxValue)
                {
                    maxValue = Math.Abs(buffer[i]);
                    maxIndex = i;
                }
            }
            Debug.Log($"xxxjack checkToneBuffer: {name}[{maxIndex}] = {maxValue}");
            if (maxValue > factor)
            {
                Debug.LogWarning($"xxxjack checkToneBuffer: too large");
            }
        }
    }
}