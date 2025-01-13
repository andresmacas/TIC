using UnityEngine;
using UnityEngine.InputSystem;

public class MetaQuest2Input: MonoBehaviour {
    public InputActionReference secondaryButtonAction; // Asigna aqu� tu SecondaryButton
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
        secondaryButtonAction.action.Enable(); // Aseg�rate de habilitar el Input Action
    }

    private void OnDisable() {
        secondaryButtonAction.action.started -= StartRecording;
        secondaryButtonAction.action.canceled -= StopRecording;
        secondaryButtonAction.action.Disable();
    }

    private void StartRecording(InputAction.CallbackContext context) {
        Debug.Log("Bot�n B presionado - Iniciando grabaci�n");
        if (TriggerActive.triggerUsed) {
            TriggerActive.isPressedButton = true;
        }
        // Aqu� puedes iniciar la grabaci�n de audio
    }

    private void StopRecording(InputAction.CallbackContext context) {
        Debug.Log("Bot�n B soltado - Deteniendo grabaci�n");
        TriggerActive.isPressedButton = false;
        TriggerActive.dictationScript.StopRecording();
        // Aqu� puedes detener la grabaci�n de audio y procesar el audio
    }
}
