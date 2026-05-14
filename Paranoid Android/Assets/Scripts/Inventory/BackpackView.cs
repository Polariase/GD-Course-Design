using UnityEngine;

public class BackpackView : MonoBehaviour
{
    [SerializeField] private InventoryDisplay _display;

    private InventoryData _data;
    private IInventoryHandler _handler;

    public void Initialize(InventoryData data, IInventoryHandler handler)
    {
        _data = data;
        _handler = handler;
        _display.Setup(_data, _handler, _data.CurrentCapacity, 0);
    }
}