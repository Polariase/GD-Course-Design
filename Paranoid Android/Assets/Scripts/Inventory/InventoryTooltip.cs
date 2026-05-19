using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class InventoryTooltip : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI infoText;
    public int xOffset = 15;
    public int yOffset = -15;
    private RectTransform _rectTransform;


    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Display(InventoryItem item, Vector2 position)
    {
        ItemData config = item.data;
        titleText.text = config.itemName;
        descriptionText.text = config.description;
        infoText.text = $"type: {config.itemType.ToDisplayName()} | occupation: {config.weight * item.count}";
        gameObject.SetActive(true);

        UpdatePosition(position);
    }

    private void UpdatePosition(Vector2 mousePosition)
    {
        Vector2 finalPos = mousePosition + new Vector2(xOffset, -yOffset);

        float width = _rectTransform.rect.width;
        float height = _rectTransform.rect.height;

        float screenW = Screen.width;

        if (finalPos.x + width > screenW)
        {
            finalPos.x = mousePosition.x - width - xOffset; // 翻转到鼠标左侧
        }

        if (finalPos.y - height < 0)
        {
            finalPos.y = mousePosition.y + height - yOffset; // 翻转到鼠标上方
        }

        transform.position = finalPos;
    }

    public void Hide() => gameObject.SetActive(false);
}