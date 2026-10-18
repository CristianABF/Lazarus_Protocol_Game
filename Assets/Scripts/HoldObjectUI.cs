using UnityEngine;
using TMPro;

public class HoldObjectUI : MonoBehaviour
{
    [Header("UI de Armas")]
    [SerializeField] private GameObject panelArma;
    [SerializeField] private TextMeshProUGUI textoNombreArma;
    [SerializeField] private TextMeshProUGUI textoMunicion;   // Ej: "10 / 10"
    [SerializeField] private TextMeshProUGUI textoCargadores; // Ej: "CARGADORES: 5"

    [Header("UI de Herramientas / Construcción")]
    [SerializeField] private GameObject panelHerramienta;
    [SerializeField] private TextMeshProUGUI textoNombreEstructura; // Ej: "Torreta"
    [SerializeField] private TextMeshProUGUI textoCapacidadEstructuras; // Ej: "CONSTRUIDOS: 2 / 5"

    [Header("Referencias del Jugador")]
    [SerializeField] private HoldObject holdObjectScript;

    private void Start()
    {
        OcultarTodo();
    }

    private void Update()
    {
        if (holdObjectScript == null) return;

        // Obtenemos el objeto que el jugador sostiene actualmente
        GameObject heldObj = holdObjectScript.GetHeldObject();

        if (heldObj != null)
        {
            // Buscamos IEquipableUI en el objeto sostenido o en cualquiera de sus hijos
            IEquipableUI equipable = heldObj.GetComponent<IEquipableUI>();
            if (equipable == null)
            {
                equipable = heldObj.GetComponentInChildren<IEquipableUI>(true);
            }

            if (equipable != null)
            {
                ActualizarPantalla(equipable);
            }
            else
            {
                OcultarTodo();
            }
        }
        else
        {
            OcultarTodo();
        }
    }

    private void ActualizarPantalla(IEquipableUI equipable)
    {
        switch (equipable.GetTipoObjeto())
        {
            case TipoObjetoUI.Arma:
                if (panelHerramienta != null) panelHerramienta.SetActive(false);
                if (panelArma != null) panelArma.SetActive(true);

                if (textoNombreArma != null)
                    textoNombreArma.text = equipable.GetNombreObjeto();

                if (textoMunicion != null)
                    textoMunicion.text = $"{equipable.GetMunicionActual()}/{equipable.GetMaxMunicion()}";

                if (textoCargadores != null)
                    textoCargadores.text = $"{equipable.GetCargadores()}";
                break;

            case TipoObjetoUI.Herramienta:
                if (panelArma != null) panelArma.SetActive(false);
                if (panelHerramienta != null) panelHerramienta.SetActive(true);

                if (textoNombreEstructura != null)
                    textoNombreEstructura.text = equipable.GetNombreObjeto();

                if (textoCapacidadEstructuras != null)
                    textoCapacidadEstructuras.text = $"Disponible: {equipable.GetCantidadDisponible()}/{equipable.GetCantidadMaxima()}";
                break;

            default:
                OcultarTodo();
                break;
        }
    }

    public void OcultarTodo()
    {
        if (panelArma != null) panelArma.SetActive(false);
        if (panelHerramienta != null) panelHerramienta.SetActive(false);
    }
}