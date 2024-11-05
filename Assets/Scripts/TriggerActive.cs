using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ApiResponse
{
    public int code;
    public string message;
}

public class TriggerActive : MonoBehaviour
{

    private readonly string url = "http://localhost:3000/";
    private AzureTextToSpeech azureTTS;

    private void Start()
    {
        // Busca el componente AzureTTS en la escena
        azureTTS = FindObjectOfType<AzureTextToSpeech>();
        if (azureTTS == null)
        {
            Debug.LogError("No se encontró el componente AzureTTS en la escena.");
        }
    }

    private IEnumerator HacerSolicitud()
    {
        // Crea la solicitud
        UnityWebRequest request = UnityWebRequest.Get(url);

        // Espera la respuesta
        yield return request.SendWebRequest();

        // Verifica si hubo algún error
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // Procesa la respuesta
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Respuesta de la API: " + jsonResponse);
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(jsonResponse);
            azureTTS.SynthesizeAndPlay(response.message);

            // Aquí puedes deserializar el JSON si es necesario
            // Por ejemplo, utilizando JsonUtility para un objeto específico:
            // MiObjeto respuesta = JsonUtility.FromJson<MiObjeto>(jsonResponse);
        }
        
    }

    //Asegúrate de que el jugador tenga la etiqueta "Player" en el Inspector de Unity
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Algo ha entrado en el trigger.");
        EjecutarScript();
    }

    private void EjecutarScript()
    {
        // Coloca aquí la lógica de tu script
        Debug.Log("¡El jugador ha activado el trigger!");
        StartCoroutine(HacerSolicitud());
    }
}
