using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public TextMeshProUGUI textoDelBoton;

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
        if (ControladorPartida.Instancia != null && ControladorPartida.Instancia.mejorasCompradas.Contains(tituloMejora))
        {
            MarcarComoComprado();
        }
        else if (estaDesbloqueado)
        {
            MarcarComoComprado();
        }
    }

    private void EnviarDatosAlVisor()
    {
        if (visor != null) visor.MostrarDetalles(this);
    }

    //Cambia el color del texto
    public void MarcarComoComprado()
    {
        estaDesbloqueado = true;
        if (textoDelBoton != null)
        {
            textoDelBoton.color = Color.green;
        }
    }
}