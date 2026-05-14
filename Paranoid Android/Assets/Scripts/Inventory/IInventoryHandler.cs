using UnityEngine.EventSystems;

public interface IInventoryHandler
{
    // ÊÂ¼þÍÆËÍ
    void OnSlotBeginDrag(ItemSlot slot, PointerEventData data);
    void OnSlotDrag(ItemSlot slot, PointerEventData data);
    void OnSlotEndDrag(ItemSlot slot, PointerEventData data);
    void OnSlotClick(ItemSlot slot, PointerEventData data);
    void OnSlotEnter(ItemSlot slot, PointerEventData data);
    void OnSlotPointerMove(ItemSlot slot, PointerEventData data);
    void OnSlotExit(ItemSlot slot, PointerEventData data);

    InventoryData GetData();

    bool CanAcceptItem(InventoryItem item);

    void RequestTransfer(DragPayload session, int targetIndex);

    void RequestDrop(DragPayload session);
}