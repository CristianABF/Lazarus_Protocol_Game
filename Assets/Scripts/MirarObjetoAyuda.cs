using UnityEngine;

public class MirarObjetoAyuda : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float distanciaRaycast = 3f;
    [SerializeField] private GameObject burbujaAyudaUI;

    private Camera camaraPrincipal;

    void Start()
    {
        camaraPrincipal = Camera.main;
        if (burbujaAyudaUI != null)
        {
            burbujaAyudaUI.SetActive(false);
        }
    }

    void Update()
    {
        Ray ray = camaraPrincipal.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distanciaRaycast))
        {
            if (hit.collider.CompareTag("Interactuable"))
            {
                if (burbujaAyudaUI != null) burbujaAyudaUI.SetActive(true);
                return;
            }
        }

        if (burbujaAyudaUI != null)
        {
            burbujaAyudaUI.SetActive(false);
        }
    }
}