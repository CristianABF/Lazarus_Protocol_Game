using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private Ammo[] ammo;
    [SerializeField] private PlayerBehaviour playerBehaviour;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private Image playerHealthFillImage;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI clipText;
    [SerializeField] private Image gunImage;
    [SerializeField] private Sprite[] gunSprites;

    private int currentGunIndex;

    private void Awake()
    {
        InputManager inputManager = new InputManager();
        inputManager.Player.Enable();
        inputManager.Player.SwapWeapon.performed += SwapWeapon;
    }

    private void Update()
    {
        playerHealthText.text = playerBehaviour.playerHealth + "/100";
        playerHealthFillImage.fillAmount = playerBehaviour.playerHealth / 100;
        ammoText.SetText($"{ammo[currentGunIndex].GetCurrentAmmo()}/{ammo[currentGunIndex].GetMaxAmmo()}");
        clipText.SetText($"{ammo[currentGunIndex].GetCurrentClips()}");
    }

    private void SwapWeapon(InputAction.CallbackContext context)
    {
        if (context.control.displayName == "1")
        {
            currentGunIndex = 0;
            gunImage.sprite = gunSprites[currentGunIndex];
            GameManager.Instance.ActivateWeaponObject(currentGunIndex);
        }
        else if (context.control.displayName == "2")
        {
            currentGunIndex = 1;
            gunImage.sprite = gunSprites[currentGunIndex];
            GameManager.Instance.ActivateWeaponObject(currentGunIndex);
        }
    }
}
