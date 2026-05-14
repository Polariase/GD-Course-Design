using UnityEngine;

public class BackpackController : BaseInventoryController
{
    [SerializeField] private int _initialCapacity = 24;
    [SerializeField] private BackpackView _backpackView;

    private void Start()
    {
        if (inventoryData == null || inventoryData.CurrentCapacity == 0)
        {
            inventoryData = new(_initialCapacity);
        }

        if (_backpackView != null)
        {
            _backpackView.Initialize(inventoryData, this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            inventoryData.AddItem(1001, 1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            inventoryData.AddItem(1002, 1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            inventoryData.AddItem(1003, 1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            inventoryData.AddItem(1004, 1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            inventoryData.AddItem(2001, 1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            inventoryData.AddItem(2002, 1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            inventoryData.AddItem(2003, 1);
        }
    }
}