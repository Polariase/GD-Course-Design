using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class HotbarView : MonoBehaviour
{
    [SerializeField] private InventoryDisplay _display;

    private InventoryData _data;
    private IInventoryHandler _handler;
    private PlayerStateData _stateData;

    public void Initialize(InventoryData data, IInventoryHandler handler, PlayerStateData stateData)
    {
        Cleanup();
        _data = data;
        _handler = handler;
        _stateData = stateData;
        _display.Setup(_data, _handler, _data.CurrentCapacity, 0);
        if (_stateData != null)
        {
            _stateData.OnSelectedChanged += UpdateHighlight;
            UpdateHighlight(_stateData.currentSelectedIndex, _stateData.currentSelectedItem);
        }
    }

    public void Cleanup()
    {
        if (_stateData != null)
        {
            _stateData.OnSelectedChanged -= UpdateHighlight;
            _stateData = null;
        }
    }

    public void OnDestroy()
    {
        Cleanup();
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