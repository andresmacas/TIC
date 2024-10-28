using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActive : MonoBehaviour
{
    //Asegúrate de que el jugador tenga la etiqueta "Player" en el Inspector de Unity
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("¡El jugador ha activado el trigger!");
        // Verifica si el objeto que entró es el jugador
        if (other.CompareTag("Player"))
        {
            // Ejecuta el script o la función que desees
            EjecutarScript();
        }
    }

    private void EjecutarScript()
    {
        // Coloca aquí la lógica de tu script
        Debug.Log("¡El jugador ha activado el trigger!");
    }
}
