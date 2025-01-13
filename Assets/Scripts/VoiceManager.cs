using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.Speech;
using System.Linq;

public class VoiceManager : MonoBehaviour
{
    private string url = "http://localhost:3000/"; // URL de tu API
    private AudioClip audioClip;
    private bool isRecording = false;
    private DictationRecognizer dictationRecognizer;

    private void Start()
    {
        // Inicia el reconocimiento de voz
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += OnDictationResult;
        dictationRecognizer.DictationComplete += OnDictationComplete;

        // Inicia la grabación cuando el objeto es activado
        StartCoroutine(GrabarAudio());
    }

    private void OnDestroy()
    {
        // Asegúrate de liberar recursos al destruir el objeto
        if (dictationRecognizer != null)
        {
            dictationRecognizer.Dispose();
        }
    }

    private IEnumerator GrabarAudio()
    {
        // Comienza a grabar
        isRecording = true;
        Debug.Log("Grabando audio...");

        // Inicia el reconocimiento de voz
        dictationRecognizer.Start();

        // Espera 5 segundos (ajusta este tiempo si lo deseas)
        yield return new WaitForSeconds(5.0f);

        // Detiene la grabación
        dictationRecognizer.Stop();
        isRecording = false;
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log("Texto reconocido: " + text);
        // Llama a tu API con el texto reconocido
        StartCoroutine(HacerSolicitud(text));
    }

    private void OnDictationComplete(DictationCompletionCause cause)
    {
        if (cause != DictationCompletionCause.Complete)
        {
            Debug.LogError("Dictado no completo. Causa: " + cause);
        }
    }

    private IEnumerator HacerSolicitud(string question)
    {
        // Envía la pregunta a tu API
        WWWForm form = new WWWForm();
        form.AddField("question", question); // Ajusta esto según lo que necesites enviar

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta de la API: " + jsonResponse);
            }
        }
    }
}
