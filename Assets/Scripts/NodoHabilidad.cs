using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para acceder al texto del botón

public class NodoHabilidad : MonoBehaviour
{
    [Header("Datos de la Mejora")]
    public string tituloMejora;
    [TextArea(3, 5)]
    public string descripcionTecnica;
    public int costoPuntos;

    [Header("Lógica de Progresión")]
    public bool estaDesbloqueado = false;
    public NodoHabilidad nodoRequisito;

    [Header("Interfaz Visual")]
    public TextMeshProUGUI textoDelBoton; // NUEVO: Arrastra el Text(TMP) hijo de este botón aquí

    private VisorTienda visor;

    private void Start()
    {
        visor = Object.FindFirstObjectByType<VisorTienda>();
        Button miBoton = GetComponent<Button>();
        if (miBoton != null)
        {
            miBoton.onClick.AddListener(EnviarDatosAlVisor);
        }

        // INTERROGATORIO A LA BÓVEDA INMORTAL
        // ¿Mi nombre exacto está en la lista de compras del ControladorPartida?
        if (ControladorPartida.Instancia != null && ControladorPartida.Instancia.mejorasCompradas.Contains(tituloMejora))
        {
            MarcarComoComprado(); // Me pinto de verde instantáneamente
        }
        else if (estaDesbloqueado)
        {
            // Respaldo por si marcaste uno como desbloqueado manualmente en el editor
            MarcarComoComprado();
        }
    }

    private void EnviarDatosAlVisor()
    {
        if (visor != null) visor.MostrarDetalles(this);
    }

    // NUEVA FUNCIÓN: Cambia el color del texto a verde neón
    public void MarcarComoComprado()
    {
        estaDesbloqueado = true;
        if (textoDelBoton != null)
        {
            textoDelBoton.color = Color.green; // Asignación matemática directa y sin margen de error
        }
    }
}