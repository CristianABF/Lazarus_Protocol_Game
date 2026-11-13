using UnityEngine;
using TMPro;

public class VisorTienda : MonoBehaviour
{
    [Header("Textos del Panel Derecho")]
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoDescripcion;

    public TextMeshProUGUI textoSaldoBilletera;

    private NodoHabilidad nodoSeleccionado;

    public void MostrarDetalles(NodoHabilidad nodo)
    {
        nodoSeleccionado = nodo;
        ActualizarPantalla();
    }

    private void ActualizarPantalla()
    {
        if (nodoSeleccionado == null) return;

        textoTitulo.text = $"[ {nodoSeleccionado.tituloMejora} ]";

        string estado = nodoSeleccionado.estaDesbloqueado ? "<color=#00FF00>COMPRADO</color>" : "<color=#FF0000>BLOQUEADO</color>";
        string requisitoText = (nodoSeleccionado.nodoRequisito != null && !nodoSeleccionado.nodoRequisito.estaDesbloqueado)
            ? $"REQUIERE: {nodoSeleccionado.nodoRequisito.tituloMejora}"
            : "REQUISITOS CUMPLIDOS";

        textoDescripcion.text =
            $"{nodoSeleccionado.descripcionTecnica}\n\n" +
            $"COSTO: {nodoSeleccionado.costoPuntos} PTS\n" +
            $"{requisitoText}\n" +
            $"ESTADO: {estado}";
    }

    // Funciona cuando solo es visible el panel
    private void OnEnable()
    {
        if (textoSaldoBilletera != null && ControladorPartida.Instancia != null)
        {
            textoSaldoBilletera.text = $"CRÉDITOS: {ControladorPartida.Instancia.creditosActuales} PTS";
        }
    }

    public void IntentarCompra()
    {
        if (nodoSeleccionado == null || nodoSeleccionado.estaDesbloqueado) return;

        if (nodoSeleccionado.nodoRequisito != null && !nodoSeleccionado.nodoRequisito.estaDesbloqueado)
        {
            Debug.Log("Compra rechazada: Faltan requisitos.");
            return;
        }

        if (ControladorPartida.Instancia.creditosActuales >= nodoSeleccionado.costoPuntos)
        {
            ControladorPartida.Instancia.creditosActuales -= nodoSeleccionado.costoPuntos;

            nodoSeleccionado.MarcarComoComprado();

            ControladorPartida.Instancia.RegistrarMejoraAdquirida(nodoSeleccionado.tituloMejora);

            // Refresca la pantalla y el saldo
            ActualizarPantalla();
            if (textoSaldoBilletera != null)
            {
                textoSaldoBilletera.text = $"CRÉDITOS: {ControladorPartida.Instancia.creditosActuales} PTS";
            }
        }
        else
        {
            Debug.Log("Compra rechazada: Créditos insuficientes.");
        }
    }
}