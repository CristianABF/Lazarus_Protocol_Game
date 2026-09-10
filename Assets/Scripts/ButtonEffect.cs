using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float normalScale = 1f; // normal
    [SerializeField] private float hoverScale = 1.1f; // al pasar por encima
    [SerializeField] private float clickScale = 0.95f; // al hacer click

    [SerializeField] private float speed = 8f; // velocidad de animación

    private RectTransform rectTransform;
    private Vector3 targetScale;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetScale = Vector3.one * normalScale;
    }

    void Update()
    {
        // se interpola suavemente
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.deltaTime * speed
            );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one * normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = Vector3.one * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = Vector3.one * (eventData.pointerEnter == gameObject ? hoverScale : normalScale);
    }
}