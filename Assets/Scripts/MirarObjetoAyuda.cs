using UnityEngine;
using TMPro;

public class MirarObjetoAyuda : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float distanciaRaycast = 5f;
    [SerializeField] private GameObject burbujaAyudaUI;
    [SerializeField] private TextMeshProUGUI textoTeclaUI;

    private Camera camaraPrincipal;

    void Start()
    {
        camaraPrincipal = Camera.main;
        if (burbujaAyudaUI != null) burbujaAyudaUI.SetActive(false);
    }

    void Update()
    {
        if (camaraPrincipal == null) return;

        Ray ray = camaraPrincipal.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, distanciaRaycast, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        bool mirandoObjeto = false;
        string teclaAMostrar = "";

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Interactuable"))
            {
                mirandoObjeto = true;
                teclaAMostrar = "Presiona 'F' para interactuar";
                break;
            }
            else if (hit.collider.CompareTag("Pickable"))
            {
                mirandoObjeto = true;
                teclaAMostrar = "Presiona 'E' para interactuar";
                break;
            }
        }

        if (burbujaAyudaUI != null)
        { 
            burbujaAyudaUI.SetActive(mirandoObjeto);

            if (mirandoObjeto && textoTeclaUI != null) textoTeclaUI.text = teclaAMostrar;
        }
    }
}