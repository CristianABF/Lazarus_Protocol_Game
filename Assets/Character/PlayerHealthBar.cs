using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private PlayerBehaviour player;
    [SerializeField] private Image foreground;

    // Update is called once per frame
    void Update()
    {
        if (player != null && foreground != null)
        {
            foreground.fillAmount = player.playerHealth / player.maxHealth;
        }
    }
}
