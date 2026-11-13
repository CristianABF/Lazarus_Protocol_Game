using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkbenchUI : MonoBehaviour
{
    [Header("Lista de Recetas")]
    public List<RecetaCrafteo> recetas;

    [Header("Textos e Imágenes de Pantalla")]
    public TextMeshProUGUI textoChatarraTotal;
    public TextMeshProUGUI textoNombreItem;
    public TextMeshProUGUI textoCosto;
    public Image imagenItemCentral;
    public Button botonCrear;

    [Header("Conexión con la Llave Inglesa")]
    public WrenchBuilder llaveInglesa;

    private int indiceActual = 0;
    private InventarioJugador inventario;

    private void OnEnable()
    {
        if (inventario == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
            if (player != null) inventario = player.GetComponent<InventarioJugador>();
        }

        if (llaveInglesa == null)
        {
            llaveInglesa = FindFirstObjectByType<WrenchBuilder>();
        }

        ActualizarPantalla();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) SiguienteReceta();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) AnteriorReceta();
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) FabricarItem();
    }

    public void ActualizarPantalla()
    {
        if (recetas == null || recetas.Count == 0) return;

        RecetaCrafteo actual = recetas[indiceActual];
        int chatarra = inventario != null ? inventario.chatarraAcumulada : 0;

        if (textoChatarraTotal != null)
            textoChatarraTotal.text = $"TOTAL CHATARRA: {chatarra} PTS";

        if (textoNombreItem != null)
            textoNombreItem.text = actual.nombreItem.ToUpper();

        if (imagenItemCentral != null)
        {
            imagenItemCentral.sprite = actual.imagenPlano;
            imagenItemCentral.enabled = actual.imagenPlano != null;
        }

        bool alcanza = chatarra >= actual.costoChatarra;
        bool hayCupo = llaveInglesa == null || llaveInglesa.PuedeFabricarDesdeWorkbench(actual.idEnLlaveInglesa);

        if (textoCosto != null)
        {
            if (!hayCupo)
                textoCosto.text = $"COSTE: [{chatarra} / {actual.costoChatarra}] [MAX]";
            else
                textoCosto.text = $"COSTE: [{chatarra} / {actual.costoChatarra}] {(alcanza ? "[OK]" : "[FALTA]")}";
        }

        if (botonCrear != null)
            botonCrear.interactable = alcanza && hayCupo;
    }

    public void SiguienteReceta()
    {
        if (recetas.Count == 0) return;
        indiceActual = (indiceActual + 1) % recetas.Count;
        ActualizarPantalla();
    }

    public void AnteriorReceta()
    {
        if (recetas.Count == 0) return;
        indiceActual = (indiceActual - 1 + recetas.Count) % recetas.Count;
        ActualizarPantalla();
    }

    public void FabricarItem()
    {
        if (recetas.Count == 0 || inventario == null) return;

        RecetaCrafteo actual = recetas[indiceActual];

        if (llaveInglesa != null && !llaveInglesa.PuedeFabricarDesdeWorkbench(actual.idEnLlaveInglesa))
        {
            ActualizarPantalla();
            return;
        }

        if (inventario.GastarChatarra(actual.costoChatarra))
        {
            if (llaveInglesa != null)
            {
                llaveInglesa.DesbloquearOAumentarDesdeWorkbench(actual.idEnLlaveInglesa, 1);
            }
            else
            {
                Debug.LogWarning("[WorkbenchUI] No se encontró el objeto con WrenchBuilder en la escena.");
            }

            ActualizarPantalla();
        }
    }
}