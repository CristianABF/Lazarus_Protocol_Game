using UnityEngine;

public class InventarioJugador : MonoBehaviour
{
    public int chatarraAcumulada = 0;

    public void SumarChatarra(int cantidad)
    {
        chatarraAcumulada += cantidad;
        Debug.Log("Chatarra recolectada. Total interno: " + chatarraAcumulada);
    }

    public bool GastarChatarra(int cantidad)
    {
        if (chatarraAcumulada >= cantidad)
        {
            chatarraAcumulada -= cantidad;
            Debug.Log("Chatarra gastada: " + cantidad + ". Restante: " + chatarraAcumulada);
            return true;
        }
        return false;
    }
}