using UnityEngine;

public class ChatarraRecoleccion : MonoBehaviour
{
    public int valorChatarra = 1; // Cuánta chatarra suma esta pieza

    private void OnTriggerEnter(Collider other)
    {
        // Revisamos si lo que nos tocó fue el jugador
        if (other.CompareTag("Player"))
        {
            // Buscamos el inventario en el jugador
            InventarioJugador inventario = other.GetComponent<InventarioJugador>();

            if (inventario != null)
            {
                inventario.SumarChatarra(valorChatarra); // Le sumamos la chatarra
                Destroy(gameObject); // Destruimos el objeto del piso
            }
        }
    }
}