using System.IO;
using UnityEngine;
using HuggingFace.API;



public class DictationScript : MonoBehaviour
{
    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private TriggerActive triggerActive; // Referencia al script TriggerActive

    //public void StartTextRecognition()
    //{
    //    Debug.Log("El jugador ya puede hablar");
    //    m_DictationRecognizer = new DictationRecognizer();
    //    triggerActive = FindObjectOfType<TriggerActive>(); // Encuentra el script TriggerActive

    //    m_DictationRecognizer.DictationResult += (text, confidence) =>
    //    {
    //        dictationResult = text; // Guarda el texto del dictado
    //        Debug.LogFormat("Dictation result: {0}", text);
    //    };

    //    m_DictationRecognizer.DictationHypothesis += (text) =>
    //    {
    //        triggerActive.ChangeText("Hablando");
    //        Debug.LogFormat("Dictation hypothesis: {0}", text);
    //    };

    //    m_DictationRecognizer.DictationComplete += (completionCause) =>
    //    {
    //        if (completionCause != DictationCompletionCause.Complete)
    //            Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);

    //        // Aquí, indicamos que el jugador terminó de hablar
    //        if (triggerActive != null && !string.IsNullOrEmpty(dictationResult))
    //        {
    //            triggerActive.ChangeText("Haz terminado de hablar");
    //            Debug.Log("El jugador ha dejado de hablar: " + dictationResult);
    //            triggerActive.SetPlayerFinishedTalking(dictationResult); // Pasa el texto al script TriggerActive
    //        }
    //    };

    //    m_DictationRecognizer.DictationError += (error, hresult) =>
    //    {
    //        Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
    //    };

    //    m_DictationRecognizer.Start();
    //}

    private void Start()
    {
        triggerActive = FindObjectOfType<TriggerActive>();
    }

    public void StartRecording()
    {   

        triggerActive.ChangeText("Hablando...., suelta el boton para dejar de hablar");
        clip = Microphone.Start(null, false, 30, 44100);
        recording = true;
    }

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    public void StopRecording()
    {

        triggerActive.ChangeText("Terminaste de hablar");
        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;
        SendRecording();
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    private void SendRecording()
    {
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response =>
        {
            triggerActive.ChangeText("Pensando la respuesta");
            Debug.Log("El jugador ha dejado de hablar: " + response);
            triggerActive.SetPlayerFinishedTalking(response);
        }, error =>
        {
            triggerActive.ChangeText("Error");
        });
    }

}
