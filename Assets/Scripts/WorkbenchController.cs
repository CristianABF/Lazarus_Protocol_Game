using UnityEngine;

public class WorkbenchController : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    public KeyCode teclaInteractuar = KeyCode.H;
    public float distanciaInteraccion = 3.0f;

    [Header("Interfaz (UI)")]
    public GameObject panelMenuWorkbench;

    private Transform jugador;
    private bool estaAbierto = false;

    void Start()
    {
        // Busca al player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            jugador = playerObj.transform;
        }
        else
        {
            GameObject playerPorNombre = GameObject.Find("Player");
            if (playerPorNombre != null) jugador = playerPorNombre.transform;
        }

        // Asegura que el menú empiece cerrado
        if (panelMenuWorkbench != null)
        {
            panelMenuWorkbench.SetActive(false);
        }
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (estaAbierto && distancia > distanciaInteraccion + 1f)
        {
            CerrarMenu();
            return;
        }

        // Abrir o cerrar con la tecla H
        if (distancia <= distanciaInteraccion && Input.GetKeyDown(teclaInteractuar))
        {
            if (estaAbierto)
            {
                CerrarMenu();
            }
            else
            {
                AbrirMenu();
            }
        }
    }

    public void AbrirMenu()
    {
        estaAbierto = true;
        if (panelMenuWorkbench != null) panelMenuWorkbench.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CerrarMenu()
    {
        estaAbierto = false;
        if (panelMenuWorkbench != null) panelMenuWorkbench.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}