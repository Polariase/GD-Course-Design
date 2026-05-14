using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : BaseInventoryController
{
    [SerializeField] private int _hotbarSize = 6;
    [SerializeField] private HotbarView _hotbarView;
    private PlayerStateData stateData;

    private void Start()
    {
        inventoryData ??= new InventoryData(_hotbarSize);

        inventoryData.filter = IsValidHotbarItem;

        _hotbarView.Initialize(inventoryData, this);

        stateData = PlayerController.Instance.stateData;

        PlayerController.Instance.GetComponent<PlayerInput>().actions["SwitchSlot"].performed += OnSwitchInput;
        inventoryData.OnSlotChanged += ValidateCurrentSelection;
    }

    private void OnSwitchInput(InputAction.CallbackContext ctx)
    {
        int slot = Mathf.RoundToInt(ctx.ReadValue<float>());
        SelectSlot(slot);
    }

    private void ValidateCurrentSelection(int index,InventoryItem item)
    {
        if (stateData.currentSelectedIndex <= 0 || index != stateData.currentSelectedIndex) return;

        if (item == null)
        {
            stateData.UpdateSelection(0, null);
        }
        else
        {
            stateData.UpdateSelection(index, item);
        }
    }

    public void SelectSlot(int index)
    {
        int selectIndex = 0;
        InventoryItem item = null;
        if (index > 0 && index <= 6 && index != stateData.currentSelectedIndex)
        {
            item = inventoryData.GetItem(index - 1);
            if (item != null)
            {
                selectIndex = index;
            }
        }

        stateData.UpdateSelection(selectIndex, item);
    }

    private bool IsValidHotbarItem(InventoryItem item)
    {
        // 如果槽位变为空（item为null），始终允许
        if (item == null || item.itemID <= 0) return true;

        var config = DataManager.Instance.GetItem(item.itemID);
        if (config == null) return false;

        return config.itemType == ItemType.Weapon ||
               config.itemType == ItemType.Consumable;
    }


    private void UseItem(int index)
    {
        InventoryItem item = inventoryData.GetItem(index);
        if (item == null) return;
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.GetComponent<PlayerInput>().actions["SwitchSlot"].performed -= OnSwitchInput;
        }
        inventoryData.OnSlotChanged -= ValidateCurrentSelection;
    }
}