using UnityEngine;

public class WorkbenchController : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    public KeyCode teclaInteractuar = KeyCode.H;
    public float distanciaInteraccion = 3.0f;

    [Header("Interfaz (UI)")]
    public GameObject panelMenuWorkbench; // Aquí arrastraremos el Panel de UI en el siguiente paso

    private Transform jugador;
    private bool estaAbierto = false;

    void Start()
    {
        // Busca automáticamente al Player en la escena
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            jugador = playerObj.transform;
        }
        else
        {
            // Respaldo por si el objeto se llama "Player" pero no tiene el Tag asignado
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

        // Si el jugador se aleja con el menú abierto, se cierra automáticamente
        if (estaAbierto && distancia > distanciaInteraccion + 1f)
        {
            CerrarMenu();
            return;
        }

        // Abrir o cerrar con la tecla H estando cerca
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

        // Libera el mouse para poder hacer clic en los botones del menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CerrarMenu()
    {
        estaAbierto = false;
        if (panelMenuWorkbench != null) panelMenuWorkbench.SetActive(false);

        // Vuelve a bloquear el mouse para mover la cámara en primera persona
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}