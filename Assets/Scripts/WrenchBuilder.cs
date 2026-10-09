using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Estructura para crear planos de construcción en el Inspector
[System.Serializable]
public struct BuildableItem
{
    public string itemName;
    public GameObject realPrefab;
    public GameObject hologramPrefab;
}

public class WrenchBuilder : MonoBehaviour, IPickable
{
    [Header("Referencias de Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hasWrenchBool = "HasWrench";
    private int hasWrenchHash;

    [Header("Lista de Construcción")]
    public List<BuildableItem> buildableItems;
    private int currentIndex = 0;

    [Header("Configuración de Construcción")]
    [SerializeField] private float buildRange = 10f;
    [SerializeField] private LayerMask buildableLayer;     // Capa del Suelo
    [SerializeField] private LayerMask destroyableLayer;   // Capa de las Torretas construidas
    public Camera fpsCam;

    private GameObject currentHologram;
    private bool isEquipped;
    private bool isTryingToBuild;

    private Vector3 currentBuildPosition;
    private Quaternion currentBuildRotation;
    private bool isValidBuildPosition;

    private void Awake()
    {
        // Si no se asignó manualmente en el Inspector, busca el Animator en el personaje o sus padres
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }
        hasWrenchHash = Animator.StringToHash(hasWrenchBool);
    }

    // --- MÉTODOS DE LA INTERFAZ IPICKABLE ---

    public void OnPickedUp()
    {
        this.enabled = true;
        isEquipped = true;

        // Activamos la animación en el Animator
        if (animator != null)
        {
            animator.SetBool(hasWrenchHash, true);
        }

        UpdateHologram(); // Instancia el holograma del objeto seleccionado actualmente
    }

    public void OnDropped()
    {
        this.enabled = false;
        isEquipped = false;
        isTryingToBuild = false;

        // Desactivamos la animación en el Animator
        if (animator != null)
        {
            animator.SetBool(hasWrenchHash, false);
        }

        // Destruimos el holograma al soltar la herramienta para liberar memoria
        if (currentHologram != null)
        {
            Destroy(currentHologram);
        }
    }

    // --- SUSCRIPCIÓN AL SISTEMA DE INPUTS ---

    private void OnEnable()
    {
        if (InputController.Input != null)
        {
            InputController.Input.Player.Shoot.performed += TryBuild;
            InputController.Input.Player.Shoot.canceled += CancelBuild;

            // Nuevos inputs
            InputController.Input.Player.ChangeObject.performed += ChangeSelection;
            InputController.Input.Player.Destroy.performed += TryDestroy;
        }
    }

    private void OnDisable()
    {
        if (InputController.Input != null)
        {
            InputController.Input.Player.Shoot.performed -= TryBuild;
            InputController.Input.Player.Shoot.canceled -= CancelBuild;

            InputController.Input.Player.ChangeObject.performed -= ChangeSelection;
            InputController.Input.Player.Destroy.performed -= TryDestroy;
        }
    }

    // --- LÓGICA PRINCIPAL ---

    private void Update()
    {
        if (PauseControl.isPaused || !isEquipped || fpsCam == null || buildableItems.Count == 0) return;

        UpdateHologramPosition();

        if (isTryingToBuild && isValidBuildPosition)
        {
            BuildObject();
            isTryingToBuild = false; // Evita construir múltiples objetos con un solo clic
        }
    }

    private void UpdateHologramPosition()
    {
        if (currentHologram == null) return;

        Vector3 rayOrigin = fpsCam.transform.position;
        Vector3 rayDirection = fpsCam.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, buildRange, buildableLayer))
        {
            isValidBuildPosition = true;

            currentBuildPosition = hit.point;
            currentBuildRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            currentHologram.SetActive(true);
            currentHologram.transform.position = currentBuildPosition;
            currentHologram.transform.rotation = currentBuildRotation;
        }
        else
        {
            isValidBuildPosition = false;
            currentHologram.SetActive(false);
        }
    }

    // --- ACCIONES DE LOS CONTROLES ---

    private void TryBuild(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused || buildableItems.Count == 0) return;
        isTryingToBuild = true;
    }

    private void CancelBuild(InputAction.CallbackContext context)
    {
        isTryingToBuild = false;
    }

    private void BuildObject()
    {
        GameObject prefabToBuild = buildableItems[currentIndex].realPrefab;
        if (prefabToBuild != null)
        {
            Instantiate(prefabToBuild, currentBuildPosition, currentBuildRotation);
        }
    }

    private void TryDestroy(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused || !isEquipped) return;

        Vector3 rayOrigin = fpsCam.transform.position;
        Vector3 rayDirection = fpsCam.transform.forward;

        // Trazamos el rayo buscando solo objetos en la capa de torretas
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, buildRange, destroyableLayer))
        {
            // Buscamos el script TurretBase en el objeto o sus padres para destruir la torreta completa
            TurretBase turret = hit.collider.GetComponentInParent<TurretBase>();
            if (turret != null)
            {
                Destroy(turret.gameObject);
            }
        }
    }

    private void ChangeSelection(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused || !isEquipped || buildableItems.Count == 0) return;

        Vector2 scrollValue = context.ReadValue<Vector2>();

        if (scrollValue.y > 0)
        {
            currentIndex++;
            if (currentIndex >= buildableItems.Count) currentIndex = 0;
        }
        else if (scrollValue.y < 0)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = buildableItems.Count - 1;
        }

        if (scrollValue.y != 0)
        {
            UpdateHologram();
        }
    }

    private void UpdateHologram()
    {
        if (currentHologram != null)
        {
            Destroy(currentHologram);
        }

        if (buildableItems.Count > 0 && buildableItems[currentIndex].hologramPrefab != null)
        {
            currentHologram = Instantiate(buildableItems[currentIndex].hologramPrefab);
            currentHologram.SetActive(false);
        }
    }
}