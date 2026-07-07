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

    public bool TrySwapOrMerge(int sourceIndex, InventoryData targetData, int targetIndex)
    {
        if (targetData == null) return false;
        if (sourceIndex < 0 || sourceIndex >= _items.Count) return false;
        if (targetIndex < 0 || targetIndex >= targetData._items.Count) return false;
        if (targetData == this && sourceIndex == targetIndex) return false;

        InventoryItem itemA = _items[sourceIndex];
        InventoryItem itemB = targetData._items[targetIndex];

        if (itemA == null) return false;

        if (!targetData.Acceptable(itemA)) return false;
        if (itemB != null && !Acceptable(itemB)) return false;

        if (itemB != null && itemA.data.itemID == itemB.data.itemID)
        {
            ItemData config = itemB.data;
            int maxStack = config != null ? config.maxStack : 1;

            if (maxStack > 1)
            {
                int canAdd = maxStack - itemB.count;
                if (canAdd > 0)
                {
                    int toAdd = Math.Min(canAdd, itemA.count);
                    itemB.AddCount(toAdd);
                    targetData.SetItem(targetIndex, itemB);
                    itemA.AddCount(-toAdd);
                    if (itemA.count <= 0)
                    {
                        SetItem(sourceIndex, null);
                    }
                    else
                    {
                        SetItem(sourceIndex, itemA);
                    }

                    return true;
                }
            }
        }

        _items[sourceIndex] = itemB;
        targetData._items[targetIndex] = itemA;

        OnSlotChanged?.Invoke(sourceIndex, itemB);
        targetData.OnSlotChanged?.Invoke(targetIndex, itemA);

        return true;
    }

    public InventoryItem AddItem(int itemID,int count)
    {
        return AddItem(new(DataManager.Instance.GetItemData(itemID), count));
    }

    public InventoryItem AddItem(InventoryItem incomingItem)
    {
        if (incomingItem == null || incomingItem.data.itemID <= 0) return null;
        if (!Acceptable(incomingItem)) return incomingItem;

        ItemData config = incomingItem.data;
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
            if (_items[i] == null || _items[i].data.itemID == 0)
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