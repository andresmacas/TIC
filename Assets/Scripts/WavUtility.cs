using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            WriteWavHeader(stream, clip);
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            foreach (var sample in samples)
            {
                stream.Write(BitConverter.GetBytes((short)(sample * short.MaxValue)), 0, 2);
            }
            return stream.ToArray();
        }
    }

    private static void WriteWavHeader(Stream stream, AudioClip clip)
    {
        byte[] header = new byte[44];
        int sampleRate = clip.frequency;
        int samples = clip.samples;
        int channels = clip.channels;

        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(header, 0, header.Length);
        }
    }
}
