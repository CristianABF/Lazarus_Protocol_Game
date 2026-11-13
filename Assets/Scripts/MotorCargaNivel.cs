using UnityEngine;

public class MotorCargaNivel : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    public GameObject jugador;
    public HoldObject scriptAgarre;

    void Start()
    {
        int slotAIniciar = PlayerPrefs.GetInt("SlotAIniciar", -1);

        if (slotAIniciar != -1)
        {
            Debug.Log($">> MOTOR DE CARGA: Restaurando Slot {slotAIniciar}");
            CargarDatosFisicos(slotAIniciar);
        }
        else
        {
            Debug.Log(">> NUEVA PARTIDA: Coordenadas por defecto.");
        }
    }

    private void CargarDatosFisicos(int slot)
    {
        if (jugador != null)
        {
            // Apaga las físicas temporalmente
            CharacterController cc = jugador.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            float x = PlayerPrefs.GetFloat("Slot" + slot + "_PosX");
            float y = PlayerPrefs.GetFloat("Slot" + slot + "_PosY");
            float z = PlayerPrefs.GetFloat("Slot" + slot + "_PosZ");
            float rotY = PlayerPrefs.GetFloat("Slot" + slot + "_RotY");

            jugador.transform.position = new Vector3(x, y, z);
            jugador.transform.rotation = Quaternion.Euler(0, rotY, 0);

            if (cc != null) cc.enabled = true;
        }

        // Recupera la chatarra a la bóveda central
        if (ControladorPartida.Instancia != null)
        {
            ControladorPartida.Instancia.creditosActuales = PlayerPrefs.GetInt("Slot" + slot + "_Chatarra", 0);
        }

        // Busca el arma guardada y la coloca en el jugador
        string objetoMano = PlayerPrefs.GetString("Slot" + slot + "_ObjetoMano", "Vacio");
        if (objetoMano != "Vacio" && scriptAgarre != null)
        {
            // Busca físicamente el objeto
            GameObject itemEnMapa = GameObject.Find(objetoMano);

            if (itemEnMapa != null)
            {
                scriptAgarre.ForzarAgarre(itemEnMapa);
            }
            else
            {
                Debug.LogWarning($"El arma '{objetoMano}' no se encontró tirada en el mapa para equiparla.");
            }
        }
    }
}