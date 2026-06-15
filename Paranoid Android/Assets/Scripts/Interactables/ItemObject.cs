using UnityEngine;

public class ItemObject : MonoBehaviour,IInteractable
{
    public InventoryItem item;

    public void Interact()
    {
        InventoryItem remainder = InventoryManager.Instance.Collect(item);

        if (remainder == null)
        {
            PoolManager.Instance.item.ReleaseItem(gameObject);
        }
        else
        {
            item = remainder;
        }
    }

    public void Setup(InventoryItem newItem)
    {
        item = newItem;
    }
}