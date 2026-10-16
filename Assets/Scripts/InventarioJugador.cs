using UnityEngine;

public class InventarioJugador : MonoBehaviour
{
    public int chatarraAcumulada = 0;

    public void SumarChatarra(int cantidad)
    {
        chatarraAcumulada += cantidad;
        Debug.Log("Chatarra recolectada. Total interno: " + chatarraAcumulada);
    }
}