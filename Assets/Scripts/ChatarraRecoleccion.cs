using UnityEngine;

public class ChatarraRecoleccion : MonoBehaviour
{
    public int valorChatarra = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventarioJugador inventario = other.GetComponent<InventarioJugador>();

            if (inventario != null)
            {
                inventario.SumarChatarra(valorChatarra);
                Destroy(gameObject);
            }
        }
    }
}