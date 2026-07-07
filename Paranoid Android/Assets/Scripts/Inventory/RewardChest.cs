using UnityEngine;

public class RewardChest : BaseContainer
{
    public int capacity = 5;
    private InventoryData _chestData;

    private void Awake()
    {
        _chestData = new InventoryData(capacity);
        _chestData.filter = (incomingItem) =>
        {
            if (incomingItem == null || incomingItem.data == null) return false;
            return incomingItem.data.value > 0;
        };
        GenerateRandomChip();
    }

    protected override void OpenContainer()
    {
        isOpen = true;
        var containerCtrl = InventoryManager.Instance.container as ContainerController;
        if (containerCtrl != null)
        {
            containerCtrl.OpenContainer(_chestData, this);
        }
    }

    private void GenerateRandomChip()
    {
        int targetItemID;
        int roll = Random.Range(0, 100);

        if (roll < 75)
        {
            targetItemID = 2001;
        }
        else if (roll < 95)
        {
            targetItemID = 2002;
        }
        else
        {
            targetItemID = 2003;
        }
        var itemData = DataManager.Instance.GetItemData(targetItemID);
        if (itemData != null)
        {
            InventoryItem chipItem = new InventoryItem(itemData, 1);
            _chestData.AddItem(chipItem);
        }
    }
}