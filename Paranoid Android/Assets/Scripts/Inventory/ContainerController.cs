using UnityEngine;

public class ContainerController : BaseInventoryController
{
    [SerializeField] private InventoryDisplay _display;
    private BaseContainer _currentOpenedContain;

    public void OpenContainer(InventoryData chestData, BaseContainer contain)
    {
        if (_currentOpenedContain != null && _currentOpenedContain != contain)
        {
            CloseCurrentContainer();
        }

        _currentOpenedContain = contain;
        inventoryData = chestData;
        _display.gameObject.SetActive(true);
        _display.Setup(inventoryData, this, inventoryData.CurrentCapacity, 0);

        if (UIManager.Instance != null && !UIManager.Instance.inventoryPanel.isOpen)
        {
            UIManager.Instance.OpenPanel(UIManager.Instance.inventoryPanel);
        }
    }

    public void CloseCurrentContainer()
    {
        if (_currentOpenedContain == null) return;
        _currentOpenedContain.OnContainerClosed();
        _currentOpenedContain = null;
        _display.Cleanup();
        _display.gameObject.SetActive(false);
        inventoryData = null;
    }
}