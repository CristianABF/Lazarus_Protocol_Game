using UnityEngine;

public class ChatarraRecoleccion : MonoBehaviour
{
    [Header("Configuración de Chatarra")]
    public int valorChatarra = 1;
    public float velocidadRotacion = 50f;

    void Update()
    {
        transform.Rotate(0, velocidadRotacion * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventarioJugador inventario = other.GetComponent<InventarioJugador>();
            if (inventario == null)
            {
                inventario = other.GetComponentInParent<InventarioJugador>();
            }

            if (inventario != null)
            {
                inventario.SumarChatarra(valorChatarra);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[ChatarraRecoleccion] El objeto con tag Player no tiene el componente InventarioJugador.");
            }
        }
    }
}