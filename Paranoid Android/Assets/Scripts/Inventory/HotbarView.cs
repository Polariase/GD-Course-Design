using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class HotbarView : MonoBehaviour
{
    [SerializeField] private InventoryDisplay _display;

    private InventoryData _data;
    private IInventoryHandler _handler;
    private PlayerStateData _stateData;

    public void Initialize(InventoryData data, IInventoryHandler handler)
    {
        _data = data;
        _handler = handler;
        _display.Setup(_data, _handler, _data.CurrentCapacity, 0);
        _stateData = PlayerController.Instance.stateData;
        _stateData.OnSelectedChanged += UpdateHighlight;
    }

    public void OnDestroy()
    {
        _stateData.OnSelectedChanged -= UpdateHighlight;
    }

    public void UpdateHighlight(int index, InventoryItem item)
    {
        for (int i = 0; i < _display.slots.Count; i++)
        {
            bool isSelected = (i == index - 1);
            _display.slots[i].SetHighlight(isSelected);
        }
    }
}