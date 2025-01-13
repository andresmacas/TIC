using UnityEngine;
using UnityEngine.InputSystem;

public class MetaQuest2Input: MonoBehaviour {
    public InputActionReference secondaryButtonAction; // Asigna aquí tu SecondaryButton
    private TriggerActive TriggerActive;

    private void Start() {
        TriggerActive = GetComponent<TriggerActive>();
        if (TriggerActive == null) {
            Debug.Log("no hay triger xd");
        }
    }

    private void OnEnable() {
        secondaryButtonAction.action.started += StartRecording;
        secondaryButtonAction.action.canceled += StopRecording;
        secondaryButtonAction.action.Enable(); // Asegúrate de habilitar el Input Action
    }

    private void OnDisable() {
        secondaryButtonAction.action.started -= StartRecording;
        secondaryButtonAction.action.canceled -= StopRecording;
        secondaryButtonAction.action.Disable();
    }

    private void StartRecording(InputAction.CallbackContext context) {
        Debug.Log("Botón B presionado - Iniciando grabación");
        if (TriggerActive.triggerUsed) {
            TriggerActive.isPressedButton = true;
        }
        // Aquí puedes iniciar la grabación de audio
    }

    private void StopRecording(InputAction.CallbackContext context) {
        Debug.Log("Botón B soltado - Deteniendo grabación");
        TriggerActive.isPressedButton = false;
        TriggerActive.dictationScript.StopRecording();
        // Aquí puedes detener la grabación de audio y procesar el audio
    }
}
