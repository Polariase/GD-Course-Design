using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotParent;

    public List<ItemSlot> slots = new();
    private InventoryData _boundData;
    private IInventoryHandler _currentHandler;
    private int _displayStartIndex;

    public void Setup(InventoryData data, IInventoryHandler handler, int displayCount, int startIndex = 0)
    {
        Cleanup();

        _boundData = data;
        _currentHandler = handler;
        _displayStartIndex = startIndex;

        if (_boundData != null)
        {
            _boundData.OnSlotChanged += HandleSlotChanged;
            _boundData.OnCapacityExpanded += HandleCapacityExpanded;
        }

        for (int i = 0; i < displayCount; i++)
        {
            CreateSlot(startIndex + i);
        }
    }

    private void CreateSlot(int globalIndex)
    {
        GameObject go = Instantiate(slotPrefab, slotParent);
        ItemSlot slotScript = go.GetComponent<ItemSlot>();

        if (slotScript != null)
        {
            slotScript.slotIndex = globalIndex;
            slotScript.handler = _currentHandler;

            // 初始化显示内容
            InventoryItem item = _boundData?.GetItem(globalIndex);
            slotScript.Refresh(item);

            slots.Add(slotScript);
        }
    }

    private void HandleSlotChanged(int index, InventoryItem newItem)
    {
        int localIndex = index - _displayStartIndex;
        if (localIndex >= 0 && localIndex < slots.Count)
        {
            slots[localIndex].Refresh(newItem);
        }
    }

    private void HandleCapacityExpanded(int additionalAmount)
    {
        int currentTotal = _displayStartIndex + slots.Count;
        for (int i = 0; i < additionalAmount; i++)
        {
            CreateSlot(currentTotal + i);
        }
    }

    public void Cleanup()
    {
        // 取消订阅防止内存泄漏
        if (_boundData != null)
        {
            _boundData.OnSlotChanged -= HandleSlotChanged;
            _boundData.OnCapacityExpanded -= HandleCapacityExpanded;
        }

        // 销毁旧格子
        foreach (var s in slots)
        {
            if (s != null) Destroy(s.gameObject);
        }
        slots.Clear();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int globalIndex = _displayStartIndex + i;
            slots[i].Refresh(_boundData?.GetItem(globalIndex));
        }
    }
}