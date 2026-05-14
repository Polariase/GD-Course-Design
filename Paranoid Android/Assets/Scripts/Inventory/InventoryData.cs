using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryData
{
    [SerializeField] private List<InventoryItem> _items;
    public event Action<int, InventoryItem> OnSlotChanged;
    public event Action<int> OnCapacityExpanded;
    public Func<InventoryItem, bool> filter;

    public int CurrentCapacity => _items.Count;

    public InventoryData(int initialCapacity)
    {
        _items = new List<InventoryItem>(new InventoryItem[initialCapacity]);
    }

    public bool Acceptable(InventoryItem item)
    {
        return filter == null || filter(item);
    }

    public InventoryItem GetItem(int index)
    {
        if (index < 0 || index >= _items.Count) return null;
        return _items[index];
    }

    public void SetItem(int index, InventoryItem item)
    {
        if (index < 0 || index >= _items.Count) return;
        if (item != null && !Acceptable(item))
            return;
        _items[index] = item;
        OnSlotChanged?.Invoke(index, item);
    }

    public bool SwapItems(int indexA, int indexB)
    {
        if (indexA == indexB) return false;
        if (indexA < 0 || indexA >= _items.Count || indexB < 0 || indexB >= _items.Count) return false;

        (_items[indexA], _items[indexB]) = (_items[indexB], _items[indexA]);

        OnSlotChanged?.Invoke(indexA, _items[indexA]);
        OnSlotChanged?.Invoke(indexB, _items[indexB]);

        return true;
    }

    public InventoryItem AddItem(int itemID,int count)
    {
        return AddItem(new(itemID, count));
    }

    public InventoryItem AddItem(InventoryItem incomingItem)
    {
        if (incomingItem == null || incomingItem.itemID <= 0) return null;
        if (!Acceptable(incomingItem)) return incomingItem;

        ItemData config = DataManager.Instance.GetItem(incomingItem.itemID);
        int maxStack = config != null ? config.maxStack : 1;

        // 如果可以堆叠，尝试堆叠
        if (maxStack > 1)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                InventoryItem existing = _items[i];
                if (existing != null && existing == incomingItem)
                {
                    int canAdd = maxStack - existing.count;
                    if (canAdd > 0)
                    {
                        int toAdd = Math.Min(canAdd, incomingItem.count);
                        existing.AddCount(toAdd);
                        incomingItem.AddCount(-toAdd);

                        OnSlotChanged?.Invoke(i, existing);

                        if (incomingItem.count <= 0) return null;
                    }
                }
            }
        }

        // 尝试放入空格子
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] == null || _items[i].itemID == 0)
            {
                if (incomingItem.count > maxStack)
                {
                    _items[i] = incomingItem.Split(maxStack);
                    OnSlotChanged?.Invoke(i, _items[i]);
                }
                else
                {
                    _items[i] = incomingItem;
                    OnSlotChanged?.Invoke(i, _items[i]);
                    return null;
                }
            }
        }

        // 依然有剩余
        return incomingItem;
    }

    public void RemoveItem(int index, int amount)
    {
        if (index < 0 || index >= _items.Count || _items[index] == null) return;

        _items[index].count -= amount;
        if (_items[index].count <= 0)
        {
            _items[index] = null;
        }
        OnSlotChanged?.Invoke(index, _items[index]);
    }

    public void Expand(int additionalAmount)
    {
        for (int i = 0; i < additionalAmount; i++)
        {
            _items.Add(null);
        }
        OnCapacityExpanded?.Invoke(additionalAmount);
    }
}