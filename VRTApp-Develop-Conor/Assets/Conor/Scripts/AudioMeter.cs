using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Modded from: https://www.youtube.com/watch?v=nSvSXpc_kU0
public class AudioMeter : MonoBehaviour
{
    public float updateStep = 0.01f;
    public int sampleDataLength = 1024;
    private float currentUpdateTime = 0f;
    public float clipLoudness;
    private float[] clipSampleData;
    public float sizeFactor = 1;
	public RectTransform uiImageSize;

    private void Awake()
    {
        // Create the array to hold sample data
        clipSampleData = new float[sampleDataLength];
    }

    private void Update()
    {
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep)
        {
            currentUpdateTime = 0f;

            // Get the output from the AudioListener
            AudioListener.GetOutputData(clipSampleData, 0);  // Get the mixed audio signal (channel 0)
            
            // Calculate the amplitude (loudness) by averaging the absolute values of the samples
            clipLoudness = 0f;
            foreach (var sample in clipSampleData)
            {
                clipLoudness += Mathf.Abs(sample);
            }
            clipLoudness /= sampleDataLength;  // Average the amplitude across all samples

            // Scale the amplitude by the sizeFactor
            clipLoudness *= sizeFactor;

			uiImageSize.sizeDelta = new Vector2(uiImageSize.sizeDelta.x, clipLoudness);
        }
    }
}
