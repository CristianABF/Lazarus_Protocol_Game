using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("Referencias de Salud")]
    [SerializeField] private PlayerBehaviour player;
    [SerializeField] private Image foreground;
    [SerializeField] private CanvasGroup canvasGroup; // Referencia para ocultar/mostrar el HUD

    [Header("Efecto Barra de Daño (Blanca)")]
    [SerializeField] private Image damageBar;
    [SerializeField] private float damageDelay = 0.5f;
    [SerializeField] private float damageShrinkSpeed = 2f;

    [Header("Efecto de Temblor (Shake)")]
    [SerializeField] private List<RectTransform> elementsToShake = new List<RectTransform>();
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeAmount = 8f;

    private float currentDamageFill = 1f;
    private Coroutine damageCoroutine;
    private Coroutine shakeCoroutine;
    private float lastPlayerHealth;

    void Awake()
    {
        // Si no se asigna desde el inspector, intenta obtenerlo automáticamente
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        if (player != null)
        {
            lastPlayerHealth = player.playerHealth;
            float initialRatio = Mathf.Clamp01(player.playerHealth / player.maxHealth);

            if (foreground != null) foreground.fillAmount = initialRatio;
            if (damageBar != null) damageBar.fillAmount = initialRatio;

            currentDamageFill = initialRatio;
        }
    }

    void Update()
    {
        if (player == null) return;

        float currentHealthRatio = Mathf.Clamp01(player.playerHealth / player.maxHealth);

        // Si el jugador recibe daño
        if (player.playerHealth < lastPlayerHealth)
        {
            TriggerShake();

            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            damageCoroutine = StartCoroutine(ProcessDamageBar(currentHealthRatio));
        }
        // Si se cura
        else if (player.playerHealth > lastPlayerHealth)
        {
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            currentDamageFill = currentHealthRatio;
            if (damageBar != null) damageBar.fillAmount = currentDamageFill;
        }

        lastPlayerHealth = player.playerHealth;

        if (foreground != null)
        {
            foreground.fillAmount = currentHealthRatio;
        }
    }

    // Método público para alternar la visibilidad de la vida
    public void SetHUDVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }

    private IEnumerator ProcessDamageBar(float targetRatio)
    {
        yield return new WaitForSeconds(damageDelay);

        while (Mathf.Abs(currentDamageFill - targetRatio) > 0.001f)
        {
            // Usamos unscaledDeltaTime por si el juego entra en pausa mientras la barra blanca reduce su tamaño
            currentDamageFill = Mathf.MoveTowards(currentDamageFill, targetRatio, damageShrinkSpeed * Time.unscaledDeltaTime);
            if (damageBar != null)
            {
                damageBar.fillAmount = currentDamageFill;
            }
            yield return null;
        }

        currentDamageFill = targetRatio;
        if (damageBar != null) damageBar.fillAmount = targetRatio;
    }

    public void TriggerShake()
    {
        if (elementsToShake == null || elementsToShake.Count == 0) return;

        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake());
    }

    private IEnumerator DoShake()
    {
        List<Vector2> startPositions = new List<Vector2>();
        for (int i = 0; i < elementsToShake.Count; i++)
        {
            if (elementsToShake[i] != null)
                startPositions.Add(elementsToShake[i].anchoredPosition);
            else
                startPositions.Add(Vector2.zero);
        }

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float randomX = Random.Range(-1f, 1f) * shakeAmount;
            float randomY = Random.Range(-1f, 1f) * shakeAmount;
            Vector2 offset = new Vector2(randomX, randomY);

            for (int i = 0; i < elementsToShake.Count; i++)
            {
                if (elementsToShake[i] != null)
                {
                    elementsToShake[i].anchoredPosition = startPositions[i] + offset;
                }
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        for (int i = 0; i < elementsToShake.Count; i++)
        {
            if (elementsToShake[i] != null)
            {
                elementsToShake[i].anchoredPosition = startPositions[i];
            }
        }
    }

}