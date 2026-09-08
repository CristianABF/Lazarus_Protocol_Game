using UnityEngine;


public class HoldObject : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform holdPoint;       // Objeto vac�o hijo de la c�mara donde se sostendr� el objeto
    private Transform cameraTransform;

    [Header("Configuracion")]
    [SerializeField] private float pickUpRange = 3f;    // Distancia m�xima para alcanzar el objeto
    [SerializeField] private float moveForce = 250f;    // Fuerza con la que el objeto sigue el punto de agarre

    private Rigidbody heldObjRb;
    private GameObject heldObj;

    void Start()
    {
        // busca automáticamente la cámara principal en la escena
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró ninguna 'Main Camera' en la escena.");
        }
    }
    void Update()
    {
        // Al presionar la tecla 'E' (o el bot�n que prefieras)
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

        // Si tenemos un objeto en la mano, aplicamos fisica para moverlo suavemente hacia el punto de agarre
        if (heldObj != null)
        {
            MoveObject();
        }
    }

    void TryPickUpObject()
    {
        RaycastHit hit;
        // Lanzamos un rayo desde el centro de la c�mara hacia adelante
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickUpRange))
        {
            // Verificamos que el objeto tenga la etiqueta "Pickable" y un Rigidbody
            if (hit.transform.CompareTag("Pickable") && hit.transform.GetComponent<Rigidbody>() != null)
            {
                heldObj = hit.transform.gameObject;
                heldObjRb = heldObj.GetComponent<Rigidbody>();

                // Desactivamos la gravedad y las colisiones con el jugador si es necesario para evitar bugs
                heldObjRb.useGravity = false;
                heldObjRb.linearDamping = 10; // A�ade resistencia para que no oscile salvajemente
                heldObjRb.constraints = RigidbodyConstraints.FreezeRotation; // Evita que ruede solo en el aire

                // Hacemos que el objeto sea hijo del punto de agarre (opcional, o moverlo por f�sicas)
                heldObj.transform.parent = holdPoint;
            }
        }
    }

    void MoveObject()
    {
        // Movemos el objeto suavemente hacia la posici�n del holdPoint usando fuerzas de f�sicas
        if (Vector3.Distance(heldObj.transform.position, holdPoint.position) > 0.1f)
        {
            Vector3 moveDirection = (holdPoint.position - heldObj.transform.position);
            heldObjRb.linearVelocity = moveDirection * moveForce * Time.deltaTime;
        }
        else
        {
            heldObjRb.linearVelocity = Vector3.zero;
            heldObj.transform.position = holdPoint.position;
            heldObj.transform.rotation = holdPoint.rotation;
        }
    }

    void DropObject()
    {
        if (heldObjRb != null)
        {
            // Restauramos las propiedades f�sicas originales
            heldObjRb.useGravity = true;
            heldObjRb.linearDamping = 1;
            heldObjRb.constraints = RigidbodyConstraints.None;

            // Quitamos la jerarqu�a
            heldObj.transform.parent = null;
        }

        heldObj = null;
        heldObjRb = null;
    }
}