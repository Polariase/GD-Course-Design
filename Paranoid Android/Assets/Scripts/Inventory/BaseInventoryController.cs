using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseInventoryController : MonoBehaviour, IInventoryHandler
{
    protected InventoryData inventoryData;
    public InventoryData GetData() => inventoryData;

    public virtual bool CanAcceptItem(InventoryItem item)
    {
        if (inventoryData == null || item == null) return false;
        return inventoryData.Acceptable(item);
    }

    public virtual void OnSlotBeginDrag(ItemSlot slot, PointerEventData eventData)
    {
        var item = inventoryData.GetItem(slot.slotIndex);
        if (item == null) return;

        InventoryManager.Instance.ActiveSession = new DragPayload
        {
            SourceHandler = this,
            SourceIndex = slot.slotIndex,
            SourceSlot = slot,
            Item = item
        };

        InventoryManager.Instance.ShowDragVisual(slot.iconImage.sprite, eventData.position);

        slot.iconImage.color = new Color(1, 1, 1, 0.3f);
    }

    public virtual void OnSlotDrag(ItemSlot slot, PointerEventData eventData)
    {
        InventoryManager.Instance.UpdateDragVisual(eventData.position);
    }

    public virtual void OnSlotEndDrag(ItemSlot slot, PointerEventData eventData)
    {
        var session = InventoryManager.Instance.ActiveSession;
        if (session == null) return;

        InventoryManager.Instance.HideDragVisual();
        slot.iconImage.color = Color.white;

        GameObject overObj = eventData.pointerEnter;
        ItemSlot targetSlot = overObj != null ? overObj.GetComponentInParent<ItemSlot>() : null;

        if (targetSlot != null && targetSlot.handler != null)
        {
            targetSlot.handler.RequestTransfer(session, targetSlot.slotIndex);
        }
        else
        {
            session.SourceHandler.RequestDrop(session);
        }

        InventoryManager.Instance.ActiveSession = null;
    }

    public virtual void OnSlotDrop(ItemSlot slot, PointerEventData eventData)
    {

    }

    public virtual void OnSlotClick(ItemSlot slot, PointerEventData eventData)
    {
        // 默认点击逻辑，如使用物品或拆分
    }

    public virtual void OnSlotEnter(ItemSlot slot, PointerEventData eventData)
    {
        var item = inventoryData.GetItem(slot.slotIndex);

        // 显示 Tooltip
        if (InventoryManager.Instance.ActiveSession == null && item != null)
        {
            InventoryManager.Instance.ShowTooltip(item, eventData.position);
        }
    }

    public void OnSlotPointerMove(ItemSlot slot, PointerEventData eventData)
    {
        var item = inventoryData.GetItem(slot.slotIndex);

        if (InventoryManager.Instance.ActiveSession == null && item != null)
        {
            InventoryManager.Instance.ShowTooltip(item, eventData.position);
        }
    }

    public virtual void OnSlotExit(ItemSlot slot, PointerEventData eventData)
    {
        InventoryManager.Instance.HideTooltip();
    }


    public void RequestTransfer(DragPayload session, int targetIndex)
    {
        InventoryData sourceData = session.SourceHandler.GetData();
        if (sourceData == null || inventoryData == null) return;
        sourceData.TrySwapOrMerge(session.SourceIndex, inventoryData, targetIndex);
    }

    public virtual void RequestDrop(DragPayload session)
    {
        if (session.SourceHandler != (IInventoryHandler)this) return;
        InventoryItem itemToDrop = session.Item;
        inventoryData.SetItem(session.SourceIndex, null);

        Transform player = PlayerController.Instance.transform;
        float distance = 1f;
        float height = 0.5f;

        Vector3 spawnPos = player.position + (player.forward * distance) + (Vector3.up * height);

        _ = PoolManager.Instance.item.SpawnItemAsync(itemToDrop, spawnPos);
    }
}