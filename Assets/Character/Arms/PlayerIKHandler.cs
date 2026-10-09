using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponIKHandler : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rig rigLayerRight; // Rig del brazo derecho
    [SerializeField] private Rig rigLayerLeft;  // Rig del brazo izquierdo

    [Header("Configuración de Animación")]
    [SerializeField] private string boolName = "HasWeapon";
    [SerializeField] private float transitionSpeed = 12f;

    private int isHoldingHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        isHoldingHash = Animator.StringToHash(boolName);
    }

    private void Update()
    {
        if (animator == null) return;

        // Evaluamos si el personaje tiene un arma en la mano
        bool isHolding = animator.GetBool(isHoldingHash);
        float targetWeight = isHolding ? 1f : 0f;

        // Actualizamos el peso de ambos Rigs suavemente
        if (rigLayerRight != null)
        {
            rigLayerRight.weight = Mathf.MoveTowards(rigLayerRight.weight, targetWeight, Time.deltaTime * transitionSpeed);
        }

        if (rigLayerLeft != null)
        {
            rigLayerLeft.weight = Mathf.MoveTowards(rigLayerLeft.weight, targetWeight, Time.deltaTime * transitionSpeed);
        }
    }
}