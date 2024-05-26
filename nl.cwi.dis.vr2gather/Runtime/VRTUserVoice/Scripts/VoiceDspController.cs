using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRT.UserRepresentation.Voice
{
    public class VoiceDspController : MonoBehaviour
    {

        public static bool PrepareDSP(int _sampleRate, int _bufferSize)
        {
            var ac = AudioSettings.GetConfiguration();
            if (_sampleRate == 0) _sampleRate = ac.sampleRate;
            if (_bufferSize == 0) _bufferSize = ac.dspBufferSize;

            if (_sampleRate == ac.sampleRate && _bufferSize == ac.dspBufferSize)
            {
                Debug.Log($"VoiceDspController: unchanged: sampleRate={ac.sampleRate}, dspBufferSize={ac.dspBufferSize}, speakerMode={ac.speakerMode}");
            }
            else
            {
                ac.sampleRate = _sampleRate;
                ac.dspBufferSize = _bufferSize;
                bool ok = AudioSettings.Reset(ac);
                ac = AudioSettings.GetConfiguration();
                Debug.Log($"VoiceDspController: changed to sampleRate={ac.sampleRate}, dspBufferSize={ac.dspBufferSize}, speakerMode={ac.speakerMode}");
                if (ac.sampleRate != _sampleRate && _sampleRate != 0)
                {
                    Debug.LogError($"VoiceDspController: Audio output sample rate is {ac.sampleRate} in stead of {_sampleRate}. Other participants may sound funny.");
                    return false;
                }
                if (ac.dspBufferSize != _bufferSize && _bufferSize != 0)
                {
                    Debug.LogWarning($"VoiceDspController: audio output buffer is {ac.dspBufferSize} in stead of {_bufferSize}");
                    return false;
                }
            }
            return true;
        }
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}