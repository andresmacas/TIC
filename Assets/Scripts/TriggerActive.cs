using System.Collections;
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
    private Animator animator;


    private void Start()
    {
        // Busca el componente AzureTTS en la escena
        azureTTS = FindObjectOfType<AzureTextToSpeech>();
        if (azureTTS == null)
        {
            Debug.LogError("No se encontró el componente AzureTTS en la escena.");
        }
        // Obtiene el componente Animator del personaje
        animator = GetComponent<Animator>();

        // Reproduce la animación de Idle al iniciar
        animator.Play("Idle");
    }

    private IEnumerator HacerSolicitud()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Respuesta de la API: " + jsonResponse);
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(jsonResponse);
            azureTTS.SynthesizeAndPlay(response.message);

            // Aquí puedes deserializar el JSON si es necesario
            // Por ejemplo, utilizando JsonUtility para un objeto específico:
            // MiObjeto respuesta = JsonUtility.FromJson<MiObjeto>(jsonResponse);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Asegúrate de que el objeto tenga la etiqueta "Player"
        {
            Debug.Log("¡El jugador ha activado el trigger!");
            StartCoroutine(ActivarTalking());
        }
    }

    private IEnumerator ActivarTalking()
    {
        // Activa la animación de Talking
        animator.SetTrigger("StartTalking");

        // Llama a la solicitud
        StartCoroutine(HacerSolicitud());

        // Espera un momento antes de volver a Idle (ajusta el tiempo según la duración de la animación Talking)
        yield return new WaitForSeconds(2.0f);

        // Regresa a la animación Idle
        animator.Play("Idle");
    }
}
