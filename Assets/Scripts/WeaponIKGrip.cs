using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WeaponIKGrip : MonoBehaviour
{
    [Header("Objetivos IK (Mano Izquierda)")]
    [SerializeField] private Transform leftHandTarget;

    [Header("Alineación de Cuerpo y Cámara")]
    [Tooltip("Arrastra aquí la cámara principal del jugador")]
    [SerializeField] private Transform playerCamera;
    [Tooltip("Arrastra el hueso del Pecho (Chest) o Espina Alta (Spine2)")]
    [SerializeField] private Transform chestBone;
    [SerializeField] private Vector3 chestOffsetRotation = new Vector3(0f, -45f, 0f);
    [SerializeField] private bool invertVertical = true;

    [Header("Seguimiento Físico")]
    [Tooltip("Arrastra aquí el objeto vacío CameraHolder que creaste")]
    [SerializeField] private Transform cameraHolder;
    [Tooltip("Arrastra aquí el hueso del cuello (Neck) o cabeza (Head)")]
    [SerializeField] private Transform headPositionBone;

    private Animator animator;
    private float aimWeight = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetLeftHandTarget(Transform target)
    {
        leftHandTarget = target;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        bool hasWeapon = animator.GetBool("HasWeapon");

        if (hasWeapon && leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        }
    }

    private void LateUpdate()
    {
        if (animator == null || chestBone == null || playerCamera == null) return;

        bool isAiming = animator.GetBool("IsAiming");
        bool hasWeapon = animator.GetBool("HasWeapon");

        float targetWeight = (hasWeapon && isAiming) ? 1f : 0f;
        aimWeight = Mathf.Lerp(aimWeight, targetWeight, Time.deltaTime * 10f);

        if (aimWeight > 0.01f)
        {
            Quaternion baseRotation = chestBone.rotation;
            float pitch = playerCamera.eulerAngles.x;

            if (pitch > 180f) pitch -= 360f;
            if (invertVertical) pitch = -pitch;

            Quaternion verticalBend = Quaternion.AngleAxis(pitch, playerCamera.right);
            Quaternion targetRotation = verticalBend * baseRotation;
            targetRotation *= Quaternion.Euler(chestOffsetRotation);

            chestBone.rotation = Quaternion.Slerp(baseRotation, targetRotation, aimWeight);
        }

        // Sincroniza el contenedor de la cámara con la posición física del hueso, 
        // arrastrando a la Main Camera de forma segura.
        if (cameraHolder != null && headPositionBone != null)
        {
            cameraHolder.position = headPositionBone.position;
        }
    }
}