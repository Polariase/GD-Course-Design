using UnityEngine;

public class SPC : BaseContainer
{
    public override string ActionName => "∑√Œ ¥Ê¥¢";

    protected override void OpenContainer()
    {
        InventoryData globalData = GameManager.Instance.GlobalStorageData;
        isOpen = true;

        var containerCtrl = InventoryManager.Instance.container as ContainerController;
        if (containerCtrl != null)
        {
            containerCtrl.OpenContainer(globalData, this);
        }
    }
}