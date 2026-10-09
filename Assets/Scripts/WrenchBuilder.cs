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
    public int maxAmount; // Cantidad maxima permitida
    [HideInInspector]
    public int currentAmount; // Cantidad construida en tiempo de ejecucion
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
    private float yRotationOffset = 0f;

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
            InputController.Input.Player.ChangeObject.performed += ChangeSelection;
            InputController.Input.Player.Destroy.performed += TryDestroy;
            InputController.Input.Player.Rotate.performed += RotateObject;
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
            InputController.Input.Player.Rotate.performed -= RotateObject;
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

        // Si ya alcanzamos el límite de construcción del objeto seleccionado, no mostramos el holograma
        BuildableItem currentItem = buildableItems[currentIndex];
        if (currentItem.currentAmount >= currentItem.maxAmount)
        {
            isValidBuildPosition = false;
            currentHologram.SetActive(false);
            return;
        }

        Vector3 rayOrigin = fpsCam.transform.position;
        Vector3 rayDirection = fpsCam.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, buildRange, buildableLayer))
        {
            isValidBuildPosition = true;

            currentBuildPosition = hit.point;

            // Calculamos la rotacion alineada a la superficie + la rotacion offset en x
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            currentBuildRotation = surfaceRotation * Quaternion.Euler(0f, yRotationOffset, 0f);

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

    private void RotateObject(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused || !isEquipped) return;

        // Sumamos 90 grados en el eje Y
        yRotationOffset = (yRotationOffset + 90f) % 360f;
    }

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
        BuildableItem currentItem = buildableItems[currentIndex];

        // Verificamos si aún se pueden construir más unidades
        if (currentItem.currentAmount < currentItem.maxAmount && currentItem.realPrefab != null)
        {
            Instantiate(currentItem.realPrefab, currentBuildPosition, currentBuildRotation);

            // Modificamos el struct en la lista incrementando la cuenta actual
            currentItem.currentAmount++;
            buildableItems[currentIndex] = currentItem;

            // Ocultamos el holograma si se alcanzó el límite máximo tras instanciar
            if (currentItem.currentAmount >= currentItem.maxAmount && currentHologram != null)
            {
                currentHologram.SetActive(false);
            }
        }
    }

    private void TryDestroy(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused || !isEquipped || fpsCam == null) return;

        Vector3 rayOrigin = fpsCam.transform.position;
        Vector3 rayDirection = fpsCam.transform.forward;

        // Ocultamos temporalmente el holograma si está activo para que no interfiera con el Raycast
        bool hologramWasActive = currentHologram != null && currentHologram.activeSelf;
        if (hologramWasActive)
        {
            currentHologram.SetActive(false);
        }

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, buildRange, destroyableLayer, QueryTriggerInteraction.Ignore))
        {
            // Obtenemos la raíz del objeto impactado (por si el Raycast choca contra un Collider de un objeto hijo)
            GameObject targetObject = hit.collider.transform.root.gameObject;

            // Recorremos la lista de objetos construibles para encontrar la coincidencia de nombre y liberar la cuota
            for (int i = 0; i < buildableItems.Count; i++)
            {
                BuildableItem item = buildableItems[i];

                if (item.realPrefab != null && targetObject.name.StartsWith(item.realPrefab.name))
                {
                    if (item.currentAmount > 0)
                    {
                        item.currentAmount--;
                        buildableItems[i] = item;
                    }
                    break;
                }
            }

            // Destruimos el objeto raíz (Barricada, Torreta, etc.)
            Destroy(targetObject);
        }

        // Reactivamos el holograma si estaba visible previamente
        if (hologramWasActive && currentHologram != null)
        {
            currentHologram.SetActive(true);
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
            yRotationOffset = 0f; // Resetea la rotacion al cambiar el objeto
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