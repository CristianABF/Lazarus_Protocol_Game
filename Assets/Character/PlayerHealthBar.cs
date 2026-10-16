using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private PlayerBehaviour player;
    [SerializeField] private Image healthImage;

    [Header("Sprites de la barra (de 0% a 100%)")]
    [SerializeField] private Sprite[] healthSprites;

    void Update()
    {
        if (player != null && healthImage != null && healthSprites.Length > 0 && player.maxHealth > 0)
        {
            // Calcula el porcentaje actual
            float healthRatio = Mathf.Clamp01(player.playerHealth / player.maxHealth);

            // Mapea el porcentaje al índice del array de sprites
            int index = Mathf.RoundToInt(healthRatio * (healthSprites.Length - 1));

            // Actualiza la imagen en pantalla
            healthImage.sprite = healthSprites[index];
        }
    }
}