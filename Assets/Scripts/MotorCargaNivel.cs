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

            // NUEVO 1: Restaurar la chatarra del Workbench en InventarioJugador
            InventarioJugador inv = jugador.GetComponent<InventarioJugador>();
            if (inv != null)
            {
                inv.chatarraAcumulada = PlayerPrefs.GetInt("Slot" + slot + "_ChatarraWorkbench", 0);
                Debug.Log($">> MOTOR DE CARGA: Chatarra Workbench restaurada: {inv.chatarraAcumulada}");
            }
        }

        // Recupera los créditos del Protocolo Lázaro
        if (ControladorPartida.Instancia != null)
        {
            ControladorPartida.Instancia.creditosActuales = PlayerPrefs.GetInt("Slot" + slot + "_Chatarra", 0);
        }

        // NUEVO 2: Leer el tiempo guardado y restaurar el reloj y la fase en WaveManager
        int tiempoGuardado = PlayerPrefs.GetInt("Slot" + slot + "_Tiempo", 0);
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.RestaurarTiempoYFase(tiempoGuardado);
            Debug.Log($">> MOTOR DE CARGA: Tiempo restaurado a {tiempoGuardado} segundos.");
        }
        else
        {
            Debug.LogWarning(">> MOTOR DE CARGA: No se encontró WaveManager en la escena.");
        }

        // Restaurar enemigos guardados
        string jsonEnemigos = PlayerPrefs.GetString("Slot" + slot + "_Enemigos", "");
        if (!string.IsNullOrEmpty(jsonEnemigos) && waveManager != null)
        {
            ListaEnemigosGuardados lista = JsonUtility.FromJson<ListaEnemigosGuardados>(jsonEnemigos);
            GameObject[] prefabs = waveManager.GetEnemyPrefabs();

            if (lista != null && prefabs != null)
            {
                foreach (DatosEnemigo datos in lista.enemigos)
                {
                    foreach (GameObject prefab in prefabs)
                    {
                        if (prefab != null && prefab.name == datos.nombrePrefab)
                        {
                            Vector3 pos = new Vector3(datos.posX, datos.posY, datos.posZ);
                            Quaternion rot = Quaternion.Euler(0f, datos.rotY, 0f);
                            Instantiate(prefab, pos, rot);
                            break;
                        }
                    }
                }
            }
        }

        // Busca el arma guardada y la coloca en el jugador
        string objetoMano = PlayerPrefs.GetString("Slot" + slot + "_ObjetoMano", "Vacio");
        if (objetoMano != "Vacio" && scriptAgarre != null)
        {
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

        // NUEVO 3: Limpiar la bandera para que si mueres y reinicias con Protocolo Lázaro no vuelva a cargar este slot
        PlayerPrefs.SetInt("SlotAIniciar", -1);
        PlayerPrefs.Save();
    }
}