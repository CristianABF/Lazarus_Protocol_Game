using UnityEngine;

public class ChatarraRecoleccion : MonoBehaviour
{
    public int valorChatarra = 1; // Cuánto suma esta pieza al agarrarla

    private void OnTriggerEnter(Collider other)
    {
        // Revisamos si el objeto que nos tocó tiene el Tag "Player"
        if (other.CompareTag("Player"))
        {
            // Buscamos el componente InventarioJugador en el personaje
            InventarioJugador inventario = other.GetComponent<InventarioJugador>();

            if (inventario != null)
            {
                inventario.SumarChatarra(valorChatarra); // Le sumamos la chatarra
                Destroy(gameObject); // Destruimos la chatarra del piso
            }
        }
    }
}