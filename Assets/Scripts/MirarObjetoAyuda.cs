using UnityEngine;

public class MirarObjetoAyuda : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float distanciaRaycast = 5f;
    [SerializeField] private GameObject burbujaAyudaUI;

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

        foreach (RaycastHit hit in hits)
        {
            // Ahora detecta las armas (Pickable) Y TAMBIÉN tus detectores (Interactuable)
            if (hit.collider.CompareTag("Pickable") || hit.collider.CompareTag("Interactuable"))
            {
                mirandoObjeto = true;
                break;
            }
        }

        if (burbujaAyudaUI != null) burbujaAyudaUI.SetActive(mirandoObjeto);
    }
}