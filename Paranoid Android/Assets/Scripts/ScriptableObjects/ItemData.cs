using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "SOs/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("基础信息")]
    public int itemID;
    public string itemName;
    [TextArea]
    public string description;

    [Header("堆叠设置")]
    public int maxStack = 99;

    [Header("游戏逻辑属性")]
    public ItemType itemType;
    public float weight;
    public int value;

    [Header("Addressable Keys")]
    public string iconAddress;
    public string prefabAddress;   // 丢到地上或拿在手里的模型预制体
}

public enum ItemType
{
    Weapon,
    Ammo,
    Consumable,
    Loot
}


public static class ItemTypeExtensions
{
    public static string ToDisplayName(this ItemType type)
    {
        return type switch
        {
            ItemType.Weapon => "武器",
            ItemType.Ammo => "弹药",
            ItemType.Consumable => "可用道具",
            ItemType.Loot => "战利品",
            _ => "Unknown"
        };
    }
}