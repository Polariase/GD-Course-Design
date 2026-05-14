using UnityEngine;

public class InventoryPanel : BasePanel
{
    public CanvasGroup hotbarGroup;

    public override void Open()
    {
        hotbarGroup.blocksRaycasts = true;
        hotbarGroup.interactable = true;
        base.Open();
    }

    public override void Close()
    {
        base.Close();
        hotbarGroup.blocksRaycasts = false;
        hotbarGroup.interactable = false;
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ForceCancelDrag();
            InventoryManager.Instance.HideTooltip();    
        }
    }
}