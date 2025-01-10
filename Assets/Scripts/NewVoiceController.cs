using UnityEngine;
using UnityEngine.Windows.Speech;

public class DictationScript : MonoBehaviour
{

    public DictationRecognizer m_DictationRecognizer;
    private TriggerActive triggerActive; // Referencia al script TriggerActive
    private string dictationResult = ""; // Almacena el texto generado por el dictado

    public void StartTextRecognition()
    {
        Debug.Log("El jugador ya puede hablar");
        m_DictationRecognizer = new DictationRecognizer();
        triggerActive = FindObjectOfType<TriggerActive>(); // Encuentra el script TriggerActive

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            dictationResult = text; // Guarda el texto del dictado
            Debug.LogFormat("Dictation result: {0}", text);
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            Debug.LogFormat("Dictation hypothesis: {0}", text);
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);

            // Aquí, indicamos que el jugador terminó de hablar
            if (triggerActive != null && !string.IsNullOrEmpty(dictationResult))
            {
                Debug.Log("El jugador ha dejado de hablar: " + dictationResult);
                triggerActive.SetPlayerFinishedTalking(dictationResult); // Pasa el texto al script TriggerActive
            }
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

        m_DictationRecognizer.Start();
    }
}
