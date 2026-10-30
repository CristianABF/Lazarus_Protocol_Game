using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

// Estructura para crear planos de construcción en el Inspector
[System.Serializable]
public struct BuildableItem
{
    public string id;            // Identificador único (ej: "Turret_Flamethrower", "Mine_Electric")
    public string itemName;
    public GameObject realPrefab;
    public GameObject hologramPrefab;
    public int maxAmount;
    [HideInInspector] public int currentAmount;

    [Header("Desbloqueo")]
    public bool isUnlocked;     // Determina si el jugador ya puede seleccionar y construir esto
}

public class WrenchBuilder : MonoBehaviour, IPickable, IEquipableUI
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

    [Header("Materiales / Colores del Holograma")]
    [SerializeField] private Material validHologramMaterial;
    [SerializeField] private Material invalidHologramMaterial;

    public Camera fpsCam;

    private GameObject currentHologram;
    private bool isEquipped;
    private bool isTryingToBuild;

    private Vector3 currentBuildPosition;
    private Quaternion currentBuildRotation;
    private bool isValidBuildPosition;
    private float yRotationOffset = 0f;

    private void Start()
    {
        GetNextUnlockedIndex(0);
    }
    private void Awake()
    {
        // Si no se asignó manualmente en el Inspector, busca el Animator en el personaje o sus padres
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }
        hasWrenchHash = Animator.StringToHash(hasWrenchBool);
    }
    private void SetHologramMaterial(Material mat)
    {
        if (currentHologram == null || mat == null) return;

        Renderer[] renderers = currentHologram.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material = mat;
        }
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

        if (buildableItems.Count > 0 && !!buildableItems[currentIndex].isUnlocked)
        {
            GetNextUnlockedIndex(1);
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

        BuildableItem currentItem = buildableItems[currentIndex];

        if (!currentItem.isUnlocked)
        {
            isValidBuildPosition = false;
            currentHologram.SetActive(false);
            return;
        }

        Vector3 rayOrigin = fpsCam.transform.position;
        Vector3 rayDirection = fpsCam.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, buildRange, buildableLayer))
        {
            currentHologram.SetActive(true);

            currentBuildPosition = hit.point;
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            currentBuildRotation = surfaceRotation * Quaternion.Euler(0f, yRotationOffset, 0f);

            currentHologram.transform.position = currentBuildPosition;
            currentHologram.transform.rotation = currentBuildRotation;

            // Si sobrepasa la cantidad máxima
            if (currentItem.currentAmount >= currentItem.maxAmount)
            {
                isValidBuildPosition = false;
                SetHologramMaterial(invalidHologramMaterial);
            }
            else
            {
                isValidBuildPosition = true;
                SetHologramMaterial(validHologramMaterial);
            }
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

        if (!currentItem.isUnlocked || currentItem.currentAmount >= currentItem.maxAmount)
        {
            Debug.LogWarning($"[WrenchBuilder] No se puede construir {currentItem.itemName}. Desbloqueado: {currentItem.isUnlocked}, Cantidad: {currentItem.currentAmount}/{currentItem.maxAmount}");
            return;
        }

        if (currentItem.realPrefab != null)
        {
            GameObject newObj = Instantiate(currentItem.realPrefab, currentBuildPosition, currentBuildRotation);

            // Aumentamos el contador
            currentItem.currentAmount++;
            buildableItems[currentIndex] = currentItem;

            // Si es una barricada, ejecutamos OnPlaced
            if (newObj.TryGetComponent<BarricadeObstacle>(out BarricadeObstacle barricade))
            {
                barricade.OnPlaced();
            }

            // Nos aseguramos de que el objeto tenga el componente notificador
            BuildableObject buildableObj = newObj.GetComponent<BuildableObject>();
            if (buildableObj == null)
            {
                buildableObj = newObj.AddComponent<BuildableObject>();
            }

            int itemIndex = currentIndex;

            // Escuchamos cuando el objeto se destruya
            buildableObj.OnObjectDestroyed += (obj) =>
            {
                BuildableItem item = buildableItems[itemIndex];
                if (item.currentAmount > 0)
                {
                    item.currentAmount--;
                    buildableItems[itemIndex] = item;
                }
            };

            if (currentItem.currentAmount >= currentItem.maxAmount && currentHologram != null)
            {
                // Cambia el holograma a rojo o lo oculta si se alcanzó el tope
                SetHologramMaterial(invalidHologramMaterial);
            }
        }
    }

    private void TryDestroy(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused || !isEquipped || fpsCam == null) return;

        Vector3 rayOrigin = fpsCam.transform.position;
        Vector3 rayDirection = fpsCam.transform.forward;

        bool hologramWasActive = currentHologram != null && currentHologram.activeSelf;
        if (hologramWasActive)
        {
            currentHologram.SetActive(false);
        }

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, buildRange, destroyableLayer, QueryTriggerInteraction.Ignore))
        {
            GameObject targetObject = hit.collider.transform.root.gameObject;

            // Al destruir el GameObject, saltará automáticamente el evento OnObjectDestroyed 
            // y restará 1 a la cuenta de buildableItems sin importar qué tipo de estructura sea.
            Destroy(targetObject);
        }

        if (hologramWasActive && currentHologram != null)
        {
            currentHologram.SetActive(true);
        }
    }

    public void UnlockItem(string id)
    {
        for (int i = 0; i < buildableItems.Count; i++)
        {
            if (buildableItems[i].id == id)
            {
                BuildableItem item = buildableItems[i];
                item.isUnlocked = true;
                buildableItems[i] = item;

                // Si desbloqueamos el ítem que estaba seleccionado, refrescamos el holograma
                if (i == currentIndex)
                {
                    UpdateHologram();
                }

                Debug.Log($"[WrenchBuilder] Construcción desbloqueada: {item.itemName}");
                break;
            }
        }
    }

    private void ChangeSelection(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused || !isEquipped || buildableItems.Count == 0) return;

        Vector2 scrollValue = context.ReadValue<Vector2>();

        int previousIndex = currentIndex;

        if (scrollValue.y > 0)
        {
            GetNextUnlockedIndex(1);
        }
        else if (scrollValue.y < 0)
        {
            GetNextUnlockedIndex(-1);
        }

        if (scrollValue.y != 0 && previousIndex != currentIndex)
        {
            yRotationOffset = 0f;
            UpdateHologram();
        }
    }

    // Avanza o retrocede en la lista saltando los objetos bloqueados
    private void GetNextUnlockedIndex(int direction)
    {
        int attempts = 0;
        while (attempts < buildableItems.Count)
        {
            currentIndex += direction;

            if (currentIndex >= buildableItems.Count) currentIndex = 0;
            if (currentIndex < 0) currentIndex = buildableItems.Count - 1;

            if (buildableItems[currentIndex].isUnlocked)
                return; // Encontró uno válido unlocked

            attempts++;
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

    // --- IMPLEMENTACIÓN DE LA INTERFAZ IEquipableUI ---

    public TipoObjetoUI GetTipoObjeto() => TipoObjetoUI.Herramienta;

    public string GetNombreObjeto()
    {
        if (buildableItems != null && buildableItems.Count > currentIndex)
        {
            return buildableItems[currentIndex].itemName;
        }
        return "Llave Inglesa";
    }

    public int GetCantidadDisponible()
    {
        if (buildableItems != null && buildableItems.Count > currentIndex)
        {
            return buildableItems[currentIndex].currentAmount;
        }
        return 0;
    }

    public int GetCantidadMaxima()
    {
        if (buildableItems != null && buildableItems.Count > currentIndex)
        {
            return buildableItems[currentIndex].maxAmount;
        }
        return 0;
    }

    // Métodos vacíos requeridos por la interfaz para armas
    public int GetMunicionActual() => 0;
    public int GetMaxMunicion() => 0;
    public int GetCargadores() => 0;

    public void IncreaseMaxAmount(string id, int extraAmount, int absoluteMaxLimit)
    {
        for (int i = 0; i < buildableItems.Count; i++)
        {
            if (buildableItems[i].id == id)
            {
                BuildableItem item = buildableItems[i];

                if (item.maxAmount >= absoluteMaxLimit)
                {
                    Debug.Log($"[WrenchBuilder] {item.itemName} ya alcanzó su límite máximo absoluto ({absoluteMaxLimit}).");
                    return;
                }

                item.maxAmount = Mathf.Min(item.maxAmount + extraAmount, absoluteMaxLimit);
                buildableItems[i] = item;

                Debug.Log($"[WrenchBuilder] Límite aumentado para {item.itemName}. Nuevo máximo: {item.maxAmount}/{absoluteMaxLimit}");

                if (i == currentIndex)
                {
                    UpdateHologramPosition();
                }
                break;
            }
        }
    }
}