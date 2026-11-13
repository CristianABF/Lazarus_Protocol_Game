using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Estructura para crear planos de construcción en el Inspector
[System.Serializable]
public struct BuildableItem
{
    public string id;
    public string itemName;
    public GameObject realPrefab;
    public GameObject hologramPrefab;
    public int maxAmount;
    [HideInInspector] public int stockDisponible;
    [HideInInspector] public int placedCount;

    [Header("Desbloqueo")]
    public bool isUnlocked;
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
    [SerializeField] private LayerMask buildableLayer;
    [SerializeField] private LayerMask destroyableLayer;

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

        if (animator != null)
        {
            animator.SetBool(hasWrenchHash, true);
        }

        if (buildableItems.Count > 0 && !buildableItems[currentIndex].isUnlocked)
        {
            GetNextUnlockedIndex(1);
        }

        UpdateHologram();
    }

    public void OnDropped()
    {
        this.enabled = false;
        isEquipped = false;
        isTryingToBuild = false;

        if (animator != null)
        {
            animator.SetBool(hasWrenchHash, false);
        }

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
            isTryingToBuild = false;
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

            if (currentItem.stockDisponible <= 0 || currentItem.placedCount >= currentItem.maxAmount)
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

        if (!currentItem.isUnlocked || currentItem.stockDisponible <= 0 || currentItem.placedCount >= currentItem.maxAmount)
        {
            Debug.LogWarning($"[WrenchBuilder] No se puede construir {currentItem.itemName}. Stock: {currentItem.stockDisponible}, Colocados: {currentItem.placedCount}/{currentItem.maxAmount}");
            return;
        }

        if (currentItem.realPrefab != null)
        {
            GameObject newObj = Instantiate(currentItem.realPrefab, currentBuildPosition, currentBuildRotation);

            currentItem.stockDisponible--;
            currentItem.placedCount++;
            buildableItems[currentIndex] = currentItem;

            if (newObj.TryGetComponent<BarricadeObstacle>(out BarricadeObstacle barricade))
            {
                barricade.OnPlaced();
            }

            BuildableObject buildableObj = newObj.GetComponent<BuildableObject>();
            if (buildableObj == null)
            {
                buildableObj = newObj.AddComponent<BuildableObject>();
            }

            int itemIndex = currentIndex;

            buildableObj.OnObjectDestroyed += (obj) =>
            {
                BuildableItem item = buildableItems[itemIndex];
                if (item.placedCount > 0)
                {
                    item.placedCount--;
                    buildableItems[itemIndex] = item;
                }
            };

            if ((currentItem.stockDisponible <= 0 || currentItem.placedCount >= currentItem.maxAmount) && currentHologram != null)
            {
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

    private void GetNextUnlockedIndex(int direction)
    {
        int attempts = 0;
        while (attempts < buildableItems.Count)
        {
            currentIndex += direction;

            if (currentIndex >= buildableItems.Count) currentIndex = 0;
            if (currentIndex < 0) currentIndex = buildableItems.Count - 1;

            if (buildableItems[currentIndex].isUnlocked)
                return;

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

    // Muestra el stock disponible fabricado
    public int GetCantidadDisponible()
    {
        if (buildableItems != null && buildableItems.Count > currentIndex)
        {
            return buildableItems[currentIndex].stockDisponible;
        }
        return 0;
    }

    // Muestra el límite máximo configurado en el Inspector
    public int GetCantidadMaxima()
    {
        if (buildableItems != null && buildableItems.Count > currentIndex)
        {
            return buildableItems[currentIndex].maxAmount;
        }
        return 0;
    }

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
                if (item.maxAmount >= absoluteMaxLimit) return;

                item.maxAmount = Mathf.Min(item.maxAmount + extraAmount, absoluteMaxLimit);
                buildableItems[i] = item;

                if (i == currentIndex)
                {
                    UpdateHologramPosition();
                }
                break;
            }
        }
    }

    // --- CONEXIÓN CON EL WORKBENCH ---

    public bool PuedeFabricarDesdeWorkbench(string id)
    {
        for (int i = 0; i < buildableItems.Count; i++)
        {
            if (buildableItems[i].id == id)
            {
                BuildableItem item = buildableItems[i];
                return (item.stockDisponible + item.placedCount) < item.maxAmount;
            }
        }
        return false;
    }

    public bool DesbloquearOAumentarDesdeWorkbench(string id, int cantidadExtra = 1)
    {
        for (int i = 0; i < buildableItems.Count; i++)
        {
            if (buildableItems[i].id == id)
            {
                BuildableItem item = buildableItems[i];

                if ((item.stockDisponible + item.placedCount) >= item.maxAmount)
                {
                    Debug.LogWarning($"[WrenchBuilder] '{item.itemName}' ya alcanzó su límite máximo ({item.maxAmount}).");
                    return false;
                }

                item.isUnlocked = true;
                item.stockDisponible = Mathf.Min(item.stockDisponible + cantidadExtra, item.maxAmount - item.placedCount);
                buildableItems[i] = item;

                if (i == currentIndex)
                {
                    UpdateHologram();
                    UpdateHologramPosition();
                }

                Debug.Log($"[WrenchBuilder] '{item.itemName}' fabricada. Disponibles: {item.stockDisponible}/{item.maxAmount}");
                return true;
            }
        }

        Debug.LogWarning($"[WrenchBuilder] No se encontró ningún item con id '{id}' en la llave inglesa.");
        return false;
    }
}