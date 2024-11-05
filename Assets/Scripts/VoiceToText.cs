using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class VoiceCapture : MonoBehaviour
{
    private AudioClip audioClip;
    private bool isRecording = false;
    private string witAiToken = "X5VGRZXBIXGRRJBG77TSCLI4WUYYUXX6"; // Reemplaza con tu Server Access Token de Wit.ai

    void StartRecording()
    {
        if (Microphone.devices.Length > 0 && !isRecording)
        {
            audioClip = Microphone.Start(null, false, 10, 44100);
            isRecording = true;
            Debug.Log("Grabación iniciada...");
        }
        else
        {
            Debug.LogWarning("No se detectó micrófono o ya se está grabando.");
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            Microphone.End(null);
            isRecording = false;
            Debug.Log("Grabación detenida.");
            
            // Convertir el AudioClip a formato WAV y enviar a Wit.ai
            StartCoroutine(SendWitAiRequest());
        }
    }

    IEnumerator SendWitAiRequest()
    {
        byte[] audioData = WavUtility.FromAudioClip(audioClip);

        UnityWebRequest request = new UnityWebRequest("https://api.wit.ai/speech", "POST");
        request.uploadHandler = new UploadHandlerRaw(audioData);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Agregar encabezados
        request.SetRequestHeader("Authorization", "Bearer " + witAiToken);
        request.SetRequestHeader("Content-Type", "audio/wav");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la solicitud a Wit.ai: " + request.error);
        }
        else
        {
            Debug.Log("Respuesta de Wit.ai: " + request.downloadHandler.text);
            // Procesar la respuesta JSON aquí
        }
    }

    // Método que puedes llamar para comenzar la grabación en un evento o trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡El jugador ha activado el trigger!");
            StartRecording();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopRecording();
        }
    }
}
