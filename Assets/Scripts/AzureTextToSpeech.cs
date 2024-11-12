using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AzureTextToSpeech : MonoBehaviour
{
    [Header("Azure TTS Configuration")]
    public string subscriptionKey = "TU_CLAVE_DE_SUSCRIPCION";
    public string region = "eastus";
    public string voiceName = "es-ES-AlvaroNeural";  // Cambia la voz según prefieras

    [Header("Unity Configuration")]
    public AudioSource audioSource;

    private string ttsEndpoint;

    void Start()
    {
        ttsEndpoint = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";
    }

    public void SynthesizeAndPlay(string text)
    {
        StartCoroutine(SendTextToSpeechRequest(text));
    }

    private IEnumerator SendTextToSpeechRequest(string text)
    {
        // Configuración de la solicitud TTS
        var request = new UnityWebRequest(ttsEndpoint, "POST");
        request.SetRequestHeader("Ocp-Apim-Subscription-Key", subscriptionKey);
        request.SetRequestHeader("Content-Type", "application/ssml+xml");
        request.SetRequestHeader("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm"); // Usar WAV

        // Generar el contenido SSML para el TTS de Azure
        string ssml = $@"
        <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
            <voice name='{voiceName}'>{text}</voice>
        </speak>";
        Debug.Log(ssml);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(ssml);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Enviar la solicitud y esperar respuesta
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error en la solicitud TTS: " + request.error);
        }
        else
        {
            Debug.LogError("Respuesta de la API: " + request.downloadHandler.text);
            // Guardar el archivo WAV en disco
            byte[] audioData = request.downloadHandler.data;
            string filePath = Path.Combine(Application.persistentDataPath, "tts_audio.wav");
            File.WriteAllBytes(filePath, audioData);

            // Cargar el archivo WAV en Unity
            yield return StartCoroutine(LoadAudioFromFile(filePath));
        }
    }

    private IEnumerator LoadAudioFromFile(string filePath)
    {
        using (UnityWebRequest audioLoader = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
        {
            yield return audioLoader.SendWebRequest();

            if (audioLoader.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioLoader);
                audioSource.clip = audioClip;
                audioSource.Play();

                // Esperar hasta que el audio termine de reproducirse
                yield return new WaitUntil(() => !audioSource.isPlaying);

                // Eliminar el archivo después de la reproducción
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.log("Archivo de audio eliminado: " + filePath);
                }
            }
            else
            {
                Debug.LogError("Error cargando el audio: " + audioLoader.error);
            }
        }
    }
}
