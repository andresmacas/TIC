using System;
using UnityEngine;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] data)
    {
        int headerSize = 44; // Tama�o est�ndar del encabezado WAV
        int sampleCount = (data.Length - headerSize) / 2;
        float[] audioData = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(data, headerSize + i * 2);
            audioData[i] = sample / 32768.0f; // Normalizar los valores de audio
        }

        AudioClip audioClip = AudioClip.Create("AzureTTS", sampleCount, 1, 16000, false);
        audioClip.SetData(audioData, 0);

        return audioClip;
    }
}
