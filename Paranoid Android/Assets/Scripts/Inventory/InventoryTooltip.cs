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
        string baseInfo = $"\n物品类型：{config.itemType.ToDisplayName()}" + (config.value > 0 ?
                      $" | 数据价值：{config.value * item.count}" : "");
        if (config is WeaponData weapon)
        {
            baseInfo += "\n\n" +
                        $"基础伤害: {weapon.damage}\n" +
                        $"射速: {weapon.fireRate}\n" +
                        $"弹速: {weapon.bulletSpeed}\n" +
                        $"射程: {weapon.distance}\n" +
                        $"负载量: {weapon.loadPerShot}\n" +
                        $"枪械散布: {weapon.baseSpread}\n" +
                        $"瞄准速度: {weapon.aimSpeed}\n" +
                        $"瞄准散布系数: {Mathf.RoundToInt(weapon.aimSpreadMult * 100f)}%";
        }
        infoText.text = baseInfo;
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