using UnityEngine;
using TMPro;

public class VisorTienda : MonoBehaviour
{
    [Header("Textos del Panel Derecho")]
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoDescripcion;

    // NUEVO: Necesita conocer el texto superior para actualizar el saldo en vivo
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

    // Esta función se dispara sola en el instante en que el Panel_Mejoras se vuelve visible
    private void OnEnable()
    {
        // Interroga a la bóveda y actualiza el texto de inmediato
        if (textoSaldoBilletera != null && ControladorPartida.Instancia != null)
        {
            textoSaldoBilletera.text = $"CRÉDITOS: {ControladorPartida.Instancia.creditosActuales} PTS";
        }
    }

    // NUEVA FUNCIÓN: Conectar esta función al evento On Click() del Btn_Comprar
    public void IntentarCompra()
    {
        if (nodoSeleccionado == null || nodoSeleccionado.estaDesbloqueado) return; // No hay nada seleccionado o ya se compró

        // Validar si cumple el requisito previo
        if (nodoSeleccionado.nodoRequisito != null && !nodoSeleccionado.nodoRequisito.estaDesbloqueado)
        {
            Debug.Log("Compra rechazada: Faltan requisitos.");
            return;
        }

        // Validar billetera contra el cerebro inmortal
        if (ControladorPartida.Instancia.creditosActuales >= nodoSeleccionado.costoPuntos)
        {
            // 1. Cobrar el dinero
            ControladorPartida.Instancia.creditosActuales -= nodoSeleccionado.costoPuntos;

            // 2. Pintar el nodo de verde
            nodoSeleccionado.MarcarComoComprado();

            // 3. Informarle al cerebro global qué mejora matemática debe guardar
            ControladorPartida.Instancia.RegistrarMejoraAdquirida(nodoSeleccionado.tituloMejora);

            // 4. Refrescar la pantalla y el saldo
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