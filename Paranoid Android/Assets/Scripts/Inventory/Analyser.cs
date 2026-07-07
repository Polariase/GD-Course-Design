using UnityEngine;

public class ItemAnalyser : BaseContainer
{
    [SerializeField] private int _analyserCapacity = 10;
    private InventoryData _analyserInventoryData;

    public override string ActionName => "启动解析器";

    private void Awake()
    {
        _analyserInventoryData = new InventoryData(_analyserCapacity);
        _analyserInventoryData.filter = (incomingItem) =>
        {
            if (incomingItem == null || incomingItem.data == null) return false;
            return incomingItem.data.value > 0;
        };
    }

    protected override void OpenContainer()
    {
        isOpen = true;
        var containerCtrl = InventoryManager.Instance.container as ContainerController;
        if (containerCtrl != null)
        {
            containerCtrl.OpenContainer(_analyserInventoryData, this);
        }
    }

    public override void OnContainerClosed()
    {
        base.OnContainerClosed();
        int totalSettledValue = 0;
        for (int i = 0; i < _analyserInventoryData.CurrentCapacity; i++)
        {
            InventoryItem item = _analyserInventoryData.GetItem(i);
            if (item != null && item.data != null)
            {
                totalSettledValue += item.data.value * item.count;
            }
        }

        if (totalSettledValue > 0)
        {
            GameManager.Instance.globalDataCount += totalSettledValue;
            PopSuccessText(totalSettledValue);
        }
        ClearAnalyserContents();
    }

    private void ClearAnalyserContents()
    {
        for (int i = 0; i < _analyserInventoryData.CurrentCapacity; i++)
        {
            _analyserInventoryData.SetItem(i, null);
        }
    }

    private void PopSuccessText(int totalValue)
    {
        if (PoolManager.Instance != null && PoolManager.Instance.popup != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2f;
            PopupManager.Instance.ShowWordText(spawnPos, $"成功解析{totalValue}份数据", WordTextType.Good, transform, 3f);
        }
    }
}