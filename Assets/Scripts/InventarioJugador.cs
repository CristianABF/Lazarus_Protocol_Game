using UnityEngine;

public class InventarioJugador : MonoBehaviour
{
    // Esta variable guarda cuánta chatarra tienes acumulada
    public int chatarraAcumulada = 0;

    // Esta función suma la chatarra cuando la recolectas
    public void SumarChatarra(int cantidad)
    {
        chatarraAcumulada += cantidad;
        Debug.Log("Chatarra actual: " + chatarraAcumulada); // Se muestra en la consola de Unity
    }
}