using UnityEngine;
using UnityEngine.InputSystem;

public class HoldObject : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform holdPoint;       
    [SerializeField] private Animator animator;
    private Transform cameraTransform;

    [Header("Configuración")]
    [SerializeField] private float pickUpRange = 3.0f;    // Distancia máxima para alcanzar el objeto
    [SerializeField] private float throwForce = 1.0f;

    private Rigidbody heldObjRb; 
    private GameObject heldObj; 
    private static readonly int HasWeaponHash = Animator.StringToHash("HasWeapon");

    private void Start()
    {
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
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickUpRange))
        {
            if (hit.transform.CompareTag("Pickable") && hit.transform.GetComponent<Rigidbody>() != null)
            {
                heldObj = hit.transform.gameObject;
                heldObjRb = heldObj.GetComponent<Rigidbody>();

                heldObjRb.isKinematic = true;

                Collider objCollider = heldObj.GetComponent<Collider>();
                if (objCollider != null) objCollider.enabled = false;

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
            // null para reactivar físicas
            heldObj.transform.SetParent(null);

            // Reactiva colisiones
            Collider objCollider = heldObj.GetComponent<Collider>();
            if (objCollider != null) objCollider.enabled = true;

            heldObjRb.isKinematic = false;
            heldObjRb.useGravity = true;

            heldObjRb.linearVelocity = Vector3.zero;
            heldObjRb.angularVelocity = Vector3.zero;

            heldObjRb.WakeUp();

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

    public GameObject GetHeldObject()
    {
        return heldObj;
    }

    // Método público para que el Motor de Carga asigne un arma directamente
    public void ForzarAgarre(GameObject nuevoObjeto)
    {
        if (heldObj != null) DropObject();

        heldObj = nuevoObjeto;
        heldObjRb = heldObj.GetComponent<Rigidbody>();

        if (heldObjRb != null) heldObjRb.isKinematic = true;

        Collider objCollider = heldObj.GetComponent<Collider>();
        if (objCollider != null) objCollider.enabled = false;

        heldObj.transform.SetParent(holdPoint);
        heldObj.transform.localPosition = Vector3.zero;
        heldObj.transform.localRotation = Quaternion.identity;

        IPickable pickableItem = heldObj.GetComponent<IPickable>();
        if (pickableItem != null) pickableItem.OnPickedUp();

        if (animator != null) animator.SetBool(HasWeaponHash, true);
    }
}