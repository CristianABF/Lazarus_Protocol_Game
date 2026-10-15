using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerIKHandler : MonoBehaviour
{
    [Header("Referencias de Rigs")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rig rigLayerRight;
    [SerializeField] private Rig rigLayerLeft;

    [Header("Constraint de la Mano Derecha")]
    [SerializeField] private TwoBoneIKConstraint rightHandConstraint;

    [Header("Puntos de Agarre (Grips)")]
    [SerializeField] private Transform weaponRightGrip; // RightHandGrip del arma
    [SerializeField] private Transform wrenchRightGrip; // Wrench_RightHandGrip de la llave inglesa

    [Header("Parámetros del Animator")]
    [SerializeField] private string hasWeaponBool = "HasWeapon";
    [SerializeField] private string hasWrenchBool = "HasWrench";
    [SerializeField] private float transitionSpeed = 12f;

    private int hasWeaponHash;
    private int hasWrenchHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        hasWeaponHash = Animator.StringToHash(hasWeaponBool.Trim());
        hasWrenchHash = Animator.StringToHash(hasWrenchBool.Trim());
    }

    private void LateUpdate()
    {
        if (animator == null) return;

        bool hasWeapon = animator.GetBool(hasWeaponHash);
        bool hasWrench = animator.GetBool(hasWrenchHash);

        // 1. Asignar el Target de la mano derecha dinámicamente
        UpdateIKTargets(hasWeapon, hasWrench);

        // 2. Calcular los pesos de los Rigs de forma independiente

        // Brazo Derecho: Activo si tiene un arma O la llave inglesa
        float rightTargetWeight = (hasWeapon || hasWrench) ? 1f : 0f;

        // Brazo Izquierdo: Activo SOLO si tiene un arma (si HasWrench es true, pasa a 0)
        float leftTargetWeight = (hasWeapon && !hasWrench) ? 1f : 0f;

        // 3. Aplicar las transiciones de peso suavemente
        if (rigLayerRight != null)
        {
            rigLayerRight.weight = Mathf.MoveTowards(rigLayerRight.weight, rightTargetWeight, Time.deltaTime * transitionSpeed);
        }

        if (rigLayerLeft != null)
        {
            rigLayerLeft.weight = Mathf.MoveTowards(rigLayerLeft.weight, leftTargetWeight, Time.deltaTime * transitionSpeed);
        }
    }

    private void UpdateIKTargets(bool hasWeapon, bool hasWrench)
    {
        if (rightHandConstraint == null) return;

        Transform currentTarget = null;

        if (hasWeapon)
        {
            currentTarget = weaponRightGrip;
        }
        else if (hasWrench)
        {
            currentTarget = wrenchRightGrip;
        }

        // Reasignar el target únicamente cuando cambie la herramienta
        if (rightHandConstraint.data.target != currentTarget)
        {
            rightHandConstraint.data.target = currentTarget;
        }
    }
}