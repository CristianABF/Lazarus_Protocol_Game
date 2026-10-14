using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInteraction : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    [Header("Parámetros del Animator")]
    [SerializeField] private string interactTriggerName = "Interact";

    private int interactHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        interactHash = Animator.StringToHash(interactTriggerName);
    }

    // Este método se asigna en el PlayerInput (Events) o se suscribe por código
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Se ejecuta solo en el frame que se presiona la tecla
        if (context.performed)
        {
            TriggerInteraction();
        }
    }

    private void TriggerInteraction()
    {
        if (animator != null)
        {
            animator.SetTrigger(interactHash);
        }
    }
}