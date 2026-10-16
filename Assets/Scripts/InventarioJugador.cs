using UnityEngine;

public class InventarioJugador : MonoBehaviour
{
    // Esta variable guardará cuánta chatarra tienes acumulada
    public int chatarraAcumulada = 0;

    // Esta función la llamará la chatarra cuando la toquemos
    public void SumarChatarra(int cantidad)
    {
        chatarraAcumulada += cantidad;
        Debug.Log("Chatarra actual: " + chatarraAcumulada); // Para verlo en la consola de Unity
    }
}