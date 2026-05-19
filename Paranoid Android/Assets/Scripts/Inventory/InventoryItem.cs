using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public int count;
    public ItemData data;

    public InventoryItem(ItemData itemData, int amount)
    {
        data = itemData;
        count = amount;
    }

    public void AddCount(int amount) => count += amount;

    public InventoryItem Clone(int newCount)
    {
        InventoryItem newItem = new(data, newCount);
        return newItem;
    }

    public InventoryItem Split(int splitCount)
    {
        if (splitCount <= 0 || splitCount > count) return null;

        count -= splitCount;
        return Clone(splitCount);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is InventoryItem other)
        {
            if (data == other.data)
                return true;
        }
        return false;
    }

    public static bool operator ==(InventoryItem left, InventoryItem right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(InventoryItem left, InventoryItem right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(data.itemID);
    }
}
