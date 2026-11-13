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

            Debug.Log("Chatarra recolectada: +" + valorChatarra);

            Destroy(gameObject);
        }
    }
}