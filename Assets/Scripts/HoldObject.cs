using UnityEngine;
using UnityEngine.InputSystem;

public class HoldObject : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform holdPoint;       // Objeto vacío hijo de la cámara donde se sostendrá el objeto
    [SerializeField] private Animator animator;
    private Transform cameraTransform;

    [Header("Configuración")]
    [SerializeField] private float pickUpRange = 3.0f;    // Distancia máxima para alcanzar el objeto
    [SerializeField] private float throwForce = 1.0f;

    private Rigidbody heldObjRb; // Rigidbody del objeto agarrado
    private GameObject heldObj;  // Hace referencia al objeto agarrado
    private static readonly int HasWeaponHash = Animator.StringToHash("HasWeapon");

    private void Start()
    {
        // Busca automáticamente la cámara principal en la escena
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró ninguna 'Main Camera' en la escena.");
        }

        if (animator == null) animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // Se asume que en InputController la acción de interactuar/agarrar se llama "Interact"
        InputController.Input.Player.Pick.performed += OnPick;
    }

    private void OnDisable()
    {
        if (InputController.Input != null)
        {
            InputController.Input.Player.Pick.performed -= OnPick;
        }
    }

    private void OnPick(InputAction.CallbackContext context)
    {
        if (heldObj == null)
        {
            TryPickUpObject();
        }
        else
        {
            DropObject();
        }
    }

    private void TryPickUpObject()
    {
        if (cameraTransform == null) return;

        RaycastHit hit;
        // Lanzamos un rayo desde el centro de la cámara hacia adelante
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickUpRange))
        {
            // Verificamos que el objeto tenga la etiqueta "Pickable" y un Rigidbody
            if (hit.transform.CompareTag("Pickable") && hit.transform.GetComponent<Rigidbody>() != null)
            {
                heldObj = hit.transform.gameObject;
                heldObjRb = heldObj.GetComponent<Rigidbody>();

                // Convertimos a cinemático para fijarlo al holdPoint
                heldObjRb.isKinematic = true;

                // Desactivamos colisiones para evitar colisionar con el jugador
                Collider objCollider = heldObj.GetComponent<Collider>();
                if (objCollider != null) objCollider.enabled = false;

                // Emparentar y fijar posición y rotación
                heldObj.transform.SetParent(holdPoint);
                heldObj.transform.localPosition = Vector3.zero;
                heldObj.transform.localRotation = Quaternion.identity;

                // Busca si el objeto tiene algún script que implemente la interfaz IPickable
                IPickable pickableItem = heldObj.GetComponent<IPickable>();
                if (pickableItem != null) pickableItem.OnPickedUp();

                if (animator != null) animator.SetBool(HasWeaponHash, true);
            }
        }
    }

    private void DropObject()
    {
        if (heldObjRb != null)
        {
            // Desvinculamos del HoldPoint antes de reactivar físicas
            heldObj.transform.SetParent(null);

            // Reactivamos las colisiones primero
            Collider objCollider = heldObj.GetComponent<Collider>();
            if (objCollider != null) objCollider.enabled = true;

            // Restauramos el Rigidbody y la gravedad
            heldObjRb.isKinematic = false;
            heldObjRb.useGravity = true;

            // Limpiamos inercias o velocidades residuales
            heldObjRb.linearVelocity = Vector3.zero;
            heldObjRb.angularVelocity = Vector3.zero;

            // Forzamos al motor de física a procesarlo de inmediato
            heldObjRb.WakeUp();

            // Leve impulso hacia adelante al soltarlo
            if (throwForce > 0f && cameraTransform != null)
            {
                heldObjRb.AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
            }

            IPickable pickableItem = heldObj.GetComponent<IPickable>();
            if (pickableItem != null) pickableItem.OnDropped();

            if (animator != null) animator.SetBool(HasWeaponHash, false);

            heldObj = null;
            heldObjRb = null;
        }
    }
}