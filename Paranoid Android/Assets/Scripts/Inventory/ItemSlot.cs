using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IDropHandler,IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public int slotIndex;
    public Image iconImage;
    public Image borderImage;
    public TextMeshProUGUI countText;
    public Color normalColor;
    public Color highlightColor;

    public IInventoryHandler handler;

    public void Refresh(InventoryItem item)
    {
        if (item == null || item.data.itemID <= 0)
        {
            iconImage.enabled = false;
            countText.text = "";
            return;
        }
        ItemData data = item.data;
        iconImage.sprite = DataManager.Instance.GetIcon(data.iconAddress);
        iconImage.enabled = true;
        countText.text = item.count > 1 ? item.count.ToString() : "";
    }

    public void SetHighlight(bool highlight)
    {
        borderImage.color = highlight ? highlightColor : normalColor;
    }

    // Ã°ÅÝ¸ø Handler
    public void OnBeginDrag(PointerEventData eventData) => handler?.OnSlotBeginDrag(this, eventData);
    public void OnDrag(PointerEventData eventData) => handler?.OnSlotDrag(this, eventData);
    public void OnEndDrag(PointerEventData eventData) => handler?.OnSlotEndDrag(this, eventData);
    public void OnPointerClick(PointerEventData eventData) => handler?.OnSlotClick(this, eventData);
    public void OnPointerEnter(PointerEventData eventData) => handler?.OnSlotEnter(this, eventData);
    public void OnPointerExit(PointerEventData eventData) => handler?.OnSlotExit(this, eventData);
    public void OnPointerMove(PointerEventData eventData) => handler?.OnSlotPointerMove(this, eventData);
    public void OnDrop(PointerEventData eventData) { }
}