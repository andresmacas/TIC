using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class APIClient : MonoBehaviour
{
    // URL de ejemplo
    private string url = "http://localhost:3000/";

    // Método que inicia la corutina
    public void LlamarAPI()
    {
        StartCoroutine(HacerSolicitud());
    }

    // Método que realiza la solicitud
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

            // Aquí puedes deserializar el JSON si es necesario
            // Por ejemplo, utilizando JsonUtility para un objeto específico:
            // MiObjeto respuesta = JsonUtility.FromJson<MiObjeto>(jsonResponse);
        }
    }
}
