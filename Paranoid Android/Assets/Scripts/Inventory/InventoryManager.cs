using UnityEngine;
using UnityEngine.UI;

public class DragPayload
{
    public IInventoryHandler SourceHandler;
    public int SourceIndex;    
    public ItemSlot SourceSlot;
    public InventoryItem Item;  
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public BaseInventoryController backpack;
    public BaseInventoryController hotbar;
    public BaseInventoryController container;

    [SerializeField] private InventoryTooltip _tooltip;
    [SerializeField] private RectTransform _dragVisualRect;
    [SerializeField] private Image _dragVisualImage;

    public DragPayload ActiveSession { get; set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitDragVisual();
    }

    public InventoryItem Collect(InventoryItem item)
    {
        return backpack.GetData().AddItem(item);
    }

    private void InitDragVisual()
    {
        _dragVisualRect.gameObject.SetActive(false);
    }


    public void ShowDragVisual(Sprite icon, Vector2 position)
    {
        _dragVisualImage.sprite = icon;
        _dragVisualRect.position = position;
        _dragVisualRect.gameObject.SetActive(true);
        // 隐藏 Tooltip
        HideTooltip();
    }

    public void UpdateDragVisual(Vector2 position)
    {
        if (_dragVisualRect.gameObject.activeSelf)
        {
            _dragVisualRect.position = position;
        }
    }

    public void HideDragVisual()
    {
        if (_dragVisualRect != null)
            _dragVisualRect.gameObject.SetActive(false);
    }

    public void ShowTooltip(InventoryItem item, Vector2 position)
    {
        if (item == null || ActiveSession != null) return; // 正在拖拽时不显示
        _tooltip.Display(item, position);
    }

    public void HideTooltip()
    {
        if (_tooltip != null) _tooltip.Hide();
    }

    public void ForceCancelDrag()
    {
        if (ActiveSession != null)
        {
            HideDragVisual();
            ActiveSession.SourceSlot.iconImage.color = Color.white; // 恢复原色
            ActiveSession = null;
        }
    }
}