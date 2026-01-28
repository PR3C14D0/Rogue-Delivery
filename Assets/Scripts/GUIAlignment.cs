using UnityEngine;

[ExecuteAlways] // Para que funcione también en el editor
public class FixedBottomRight : MonoBehaviour
{
    public Vector2 offset = new Vector2(20, 20); // píxeles desde la esquina
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Fijar ancla a la esquina inferior derecha
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);
        rectTransform.pivot = new Vector2(1, 0);

        // Aplicar offset
        rectTransform.anchoredPosition = offset;
    }
}
