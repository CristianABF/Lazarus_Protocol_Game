using UnityEngine;

public class CheatManager : MonoBehaviour
{
    public static bool municionInfinitaActiva = false;

    [Header("Sonidos de Cheats")]
    public AudioClip sonidoHesoyam;

    // Almacena las últimas letras presionadas
    private string inputBuffer = "";

    void Update()
    {
        RegistrarTeclado();
        VerificarTrucos();
    }

    private void RegistrarTeclado()
    {
        // Atrapa cualquier letra presionada y pasa a mayúsculas
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                inputBuffer += char.ToUpper(c);
            }
        }

        // Limitamos el historial a 15 caracteres
        if (inputBuffer.Length > 15)
        {
            inputBuffer = inputBuffer.Substring(inputBuffer.Length - 15);
        }
    }

    private void VerificarTrucos()
    {
        if (inputBuffer.EndsWith("PLATA"))
        {
            AdelantarTiempo();
            LimpiarBuffer();
        }

        if (inputBuffer.EndsWith("DIOS"))
        {
            ActivarModoDios();
            LimpiarBuffer();
        }

        if (inputBuffer.EndsWith("BALAS"))
        {
            AlternarMunicionInfinita();
            LimpiarBuffer();
        }

        if (inputBuffer.EndsWith("HESOYAM"))
        {
            AdelantarTiempo();
            ActivarModoDios();
            if (!municionInfinitaActiva) AlternarMunicionInfinita();
            ReproducirAudioJugador();
            LimpiarBuffer();
        }
    }

    private void ReproducirAudioJugador()
    {
        if (sonidoHesoyam == null) return;

        PlayerBehaviour jugador = Object.FindFirstObjectByType<PlayerBehaviour>();
        if (jugador != null)
        {

            GameObject audioObj = new GameObject("TempAudio_Hesoyam");
            audioObj.transform.SetParent(jugador.transform);
            audioObj.transform.localPosition = Vector3.zero;

            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = sonidoHesoyam;
            source.spatialBlend = 1.0f;
            source.minDistance = 2f;
            source.maxDistance = 25f;
            source.Play();

            Destroy(audioObj, sonidoHesoyam.length);
        }
    }

    private void LimpiarBuffer()
    {
        // Borra el historial para que no se active el truco dos veces seguidas
        inputBuffer = "";
    }

    private void AdelantarTiempo()
    {
        if (ControladorPartida.Instancia != null)
        {
            ControladorPartida.Instancia.creditosActuales += 5000;
            Debug.LogWarning("[CHEAT] +5000 Créditos inyectados.");
        }
    }

    private void ActivarModoDios()
    {
        PlayerBehaviour jugador = Object.FindFirstObjectByType<PlayerBehaviour>();
        if (jugador != null)
        {
            jugador.maxHealth = 99999f;
            jugador.RecibirCuracion(99999f);
        }

        CoreBehaviour nucleo = Object.FindFirstObjectByType<CoreBehaviour>();
        if (nucleo != null) nucleo.currentHealth = 99999f;

        Debug.LogWarning("[CHEAT] Modo Dios activado.");
    }

    private void AlternarMunicionInfinita()
    {
        municionInfinitaActiva = !municionInfinitaActiva;
        Debug.LogWarning($"[CHEAT] Munición infinita: {municionInfinitaActiva}");
    }
}