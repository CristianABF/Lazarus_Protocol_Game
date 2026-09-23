using UnityEngine;


public class HoldObject : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform holdPoint;       // Objeto vacio hijo de la camara donde se sostendra el objeto
    [SerializeField] private Animator animator;
    private Transform cameraTransform;

    [Header("Configuracion")]
    [SerializeField] private float pickUpRange = 3.0f;    // Distancia maxima para alcanzar el objeto
    [SerializeField] private float throwForce = 1.0f;

    private Rigidbody heldObjRb; // rigidbody del objeto agarrado
    private GameObject heldObj; // hace referencia al objeto agarrado
    private static readonly int HasWeaponHash = Animator.StringToHash("HasWeapon");

    void Start()
    {
        // busca automaticamente la camara principal en la escena
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
    void Update()
    {
        // Al presionar la tecla 'E' (o el boton que prefieras)
        if (Input.GetKeyDown(KeyCode.E))
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
    }

    void TryPickUpObject()
    {
        RaycastHit hit;
        // Lanzamos un rayo desde el centro de la camara hacia adelante
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickUpRange))
        {
            // Verificamos que el objeto tenga la etiqueta "Pickable" y un Rigidbody
            if (hit.transform.CompareTag("Pickable") && hit.transform.GetComponent<Rigidbody>() != null)
            {
                heldObj = hit.transform.gameObject;
                heldObjRb = heldObj.GetComponent<Rigidbody>();

                // convertimos a cinematico para fijarlo al holdpoint
                heldObjRb.isKinematic = true;

                // desactivamos colisiones para evitar empujones con el jugador
                Collider objCollider = heldObj.GetComponent<Collider>();
                if (objCollider != null) objCollider.enabled = false;

                // emparentar y fijar posicion y rotacion
                heldObj.transform.SetParent(holdPoint);
                heldObj.transform.localPosition = Vector3.zero;
                heldObj.transform.localRotation = Quaternion.identity;

                // busca si el objeto tiene algún script que implemente la interfaz
                IPickable pickableItem = heldObj.GetComponent<IPickable>();
                if (pickableItem != null) pickableItem.OnPickedUp(); //avisa al objeto que fue agarrado

                if (animator != null) animator.SetBool(HasWeaponHash, true);
            }
        }
    }
    void DropObject()
    {
        if (heldObjRb != null)
        {
            // desvinculamos del HoldPoint antes de reactivar fisicas
            heldObj.transform.SetParent(null);

            // reactivamos las colisiones primero
            Collider objCollider = heldObj.GetComponent<Collider>();
            if (objCollider != null) objCollider.enabled = true;

            // restauramos el Rigidbody y la gravedad
            if (heldObjRb != null)
            {
                heldObjRb.isKinematic = false;
                heldObjRb.useGravity = true;

                // limpiamos inercias o velocidades residuales
                heldObjRb.linearVelocity = Vector3.zero;
                heldObjRb.angularVelocity = Vector3.zero;

                // forzamos al motor de fisica a procesarlo de inmediato
                heldObjRb.WakeUp();

                // (Opcional) leve impulso hacia adelante al soltarlo
                if (throwForce > 0f) heldObjRb.AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
            }

            IPickable pickableItem = heldObj.GetComponent<IPickable>();
            if (pickableItem != null) pickableItem.OnDropped(); // avisa al objeto que fue soltado

            if (animator != null) animator.SetBool (HasWeaponHash, false);
            heldObj = null;
            heldObjRb = null;
        }
    }
}