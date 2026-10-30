using UnityEngine;

public class ChatarraRecoleccion : MonoBehaviour
{
    [Header("Configuración de Chatarra")]
    public int valorChatarra = 1;
    public float velocidadRotacion = 50f;

    void Update()
    {
        // Hace rotar el objeto de chatarra en el aire
        transform.Rotate(0, velocidadRotacion * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el jugador entra en contacto con la chatarra
        if (other.CompareTag("Player"))
        {
            // AQUÍ SUMAS LA CHATARRA AL JUGADOR O GAMEMANAGER
            // Ejemplo: GameManager.Instance.SumarChatarra(valorChatarra);

            Debug.Log("Chatarra recolectada: +" + valorChatarra);

            // Destruye el objeto de chatarra en escena
            Destroy(gameObject);
        }
    }
}