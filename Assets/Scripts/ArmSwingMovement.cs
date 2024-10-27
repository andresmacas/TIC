using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ArmSwingMovement : MonoBehaviour
{
    public XRNode leftHandNode = XRNode.LeftHand;
    public XRNode rightHandNode = XRNode.RightHand;

    public float movementSpeed = 1.0f;
    public float swingThreshold = 0.1f; // Sensibilidad del swing

    private Vector3 leftPreviousPos;
    private Vector3 rightPreviousPos;

    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    
    private CharacterController characterController;

    void Start()
    {
        leftHandDevice = InputDevices.GetDeviceAtXRNode(leftHandNode);
        rightHandDevice = InputDevices.GetDeviceAtXRNode(rightHandNode);

        characterController = GetComponent<CharacterController>();
        leftPreviousPos = GetHandPosition(leftHandDevice);
        rightPreviousPos = GetHandPosition(rightHandDevice);
    }

    void Update()
    {
        Vector3 leftCurrentPos = GetHandPosition(leftHandDevice);
        Vector3 rightCurrentPos = GetHandPosition(rightHandDevice);

        // Detectar el movimiento de los brazos
        Vector3 leftSwing = leftCurrentPos - leftPreviousPos;
        Vector3 rightSwing = rightCurrentPos - rightPreviousPos;

        float leftSwingMagnitude = leftSwing.magnitude;
        float rightSwingMagnitude = rightSwing.magnitude;

        // Si el movimiento es mayor que el umbral, avanzar en la dirección de la vista
        if (leftSwingMagnitude > swingThreshold || rightSwingMagnitude > swingThreshold)
        {
            Vector3 forwardDirection = Camera.main.transform.forward;
            forwardDirection.y = 0; // Evitar que el jugador se mueva hacia arriba o abajo

            Vector3 movement = forwardDirection * movementSpeed * Time.deltaTime;
            characterController.Move(movement);
        }

        // Actualizar las posiciones anteriores de las manos
        leftPreviousPos = leftCurrentPos;
        rightPreviousPos = rightCurrentPos;
    }

    // Función para obtener la posición actual de la mano en espacio local
    Vector3 GetHandPosition(InputDevice handDevice)
    {
        if (handDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
        {
            return position;
        }

        return Vector3.zero;
    }
}
