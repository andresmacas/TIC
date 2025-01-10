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
    private AudioSource audioSource;
    private bool firstTriggerActivated = false; // Controla si es la primera vez que se activa el trigger
    public bool triggerUsed = false; // Evita que el trigger se active múltiples veces
    private bool npcFinishedTalking = false; // Controla cuando el NPC termina de hablar
    private bool playerFinishedTalking = false; // Controla cuando el jugador termina de hablar
    public bool isPressedButton = false;
    private bool isHearingAudio = false;

    public DictationScript dictationScript; // Referencia al script de reconocimiento de voz
    public string playerText = ""; // Almacena el texto del jugador

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
        animator.Play("Idle"); // Reproduce la animación de Idle al iniciar

        // Configura el AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("Se agregó automáticamente un AudioSource al objeto.");
        }

        // Obtén el componente DictationScript
        dictationScript = FindObjectOfType<DictationScript>();
        if (dictationScript == null)
        {
            Debug.LogError("No se encontró el componente DictationScript en la escena.");
        }
    }

    private void OnTriggerExit(Collider other) {
        Debug.Log("El jugador ha salido del trigger");
        triggerUsed = false;
        StopAllCoroutines();
        audioSource.Stop();
        playerFinishedTalking = false;
        isHearingAudio = false;
        firstTriggerActivated = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggerUsed) // Solo activa si es el jugador y no se ha usado el trigger
        {
            Debug.Log("¡El jugador ha activado el trigger!");
            triggerUsed = true; // Marca el trigger como usado
            StartCoroutine(ActivarTalking());
        }
    }

    private IEnumerator ActivarTalking()
    {
        // Activa la animación de Talking
        animator.SetTrigger("StartTalking");

        // Selecciona el archivo de audio correcto
        string audioFileName = firstTriggerActivated ? "tts_extra.wav" : "tts_audio.wav";
        firstTriggerActivated = true; // Marca que ya fue activado una vez
        yield return ReproducirAudio(audioFileName);
        yield return new WaitForSeconds(1.0f);
        // Inicia el reconocimiento de voz del jugador después de que el NPC termine de hablar

        // Regresa a la animación Idle
        animator.Play("Idle");

        // Permite que el trigger se reactive si es necesario
    }

    private IEnumerator ReproducirAudio(string audioFileName)
    {
        string filePath = System.IO.Path.Combine(Application.dataPath, audioFileName);
        if (System.IO.File.Exists(filePath))
        {
            using (UnityWebRequest audioLoader = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
            {
                yield return audioLoader.SendWebRequest();

                if (audioLoader.result == UnityWebRequest.Result.Success)
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioLoader);
                    audioSource.clip = audioClip;
                    audioSource.Play();

                    // Espera hasta que el audio termine de reproducirse
                    yield return new WaitUntil(() => !audioSource.isPlaying);
                }
                else
                {
                    Debug.LogError("Error cargando el audio: " + audioLoader.error);
                }
            }
        }
        else
        {
            Debug.LogError("El archivo de audio no existe: " + filePath);
        }
    }

    private IEnumerator HacerSolicitud()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + playerText);
        Debug.Log(request.url);
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
            StopCoroutine(HacerSolicitud());
            playerText = "";
        }
        
    }

    private void Update() {
        if (triggerUsed && isPressedButton && !isHearingAudio) {
            StartCoroutine(IniciarReconocimientoDeVoz());
            isHearingAudio = true;
        } else { 

            StopCoroutine(IniciarReconocimientoDeVoz());
        }

        if (!triggerUsed || !isPressedButton) {
            isHearingAudio = false;
        }
    }

    // Llama al método para iniciar el reconocimiento de voz del jugador
    private IEnumerator IniciarReconocimientoDeVoz()
    {
        playerText = string.Empty;
        playerFinishedTalking = false;
        Debug.Log("Se va a comprobar si el boton esta presionado");
        if (isPressedButton) {

            // Inicia el reconocimiento de voz después de un breve retraso
            dictationScript.StartTextRecognition();

            // Espera hasta que el jugador haya terminado de hablar
            yield return new WaitUntil(() => playerFinishedTalking);

            // Ahora, después de que el jugador termine de hablar, realiza la solicitud POST con el texto del reconocimiento
            //StartCoroutine(EnviarTextoAlApi());
            StartCoroutine(HacerSolicitud());
        }
    }

    // Este método se llamará cuando el jugador termine de hablar
    public void SetPlayerFinishedTalking(string finishedText)
    {
        Debug.Log("Mensaje generado" + finishedText);
        playerText = finishedText; // Captura el texto de la dictación
        playerFinishedTalking = true;
    }

    private class TextData
    {
        public string text;
    }

    // Enviar el texto de la dictación al API
    private IEnumerator EnviarTextoAlApi()
    {
        // Crear el objeto a enviar
        TextData data = new TextData();
        data.text = playerText;

        // Convertir el objeto a JSON
        string jsonData = JsonUtility.ToJson(data);

        // Configurar la solicitud PUT
        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Enviar la solicitud y esperar la respuesta
        yield return request.SendWebRequest();

        // Verificar el estado de la solicitud
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la solicitud PUT: " + request.error);
        }
        else
        {
            // Leer la respuesta de la API
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Respuesta de la API: " + jsonResponse);

            // Convertir la respuesta JSON a un objeto ApiResponse
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(jsonResponse);
            Debug.Log(response.message);
            // Llamar a un método (por ejemplo, para reproducir audio)
            azureTTS.SynthesizeAndPlay(response.message);
        }
    }
}
