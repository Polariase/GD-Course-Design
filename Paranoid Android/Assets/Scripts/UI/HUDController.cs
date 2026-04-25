using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    public TextMeshProUGUI overload;
    public TextMeshProUGUI slot;
    public PlayerStateData stateData;

    private void OnEnable()
    {
        if (stateData != null)
        {
            stateData.OnSlotChanged += OnSlotChange;
            stateData.OnLoadChanged += OnLoadChanged;
        }
    }

    private void OnDisable()
    {
        if (stateData != null)
        {
            stateData.OnSlotChanged -= OnSlotChange;
            stateData.OnLoadChanged -= OnLoadChanged;
        }
    }

    // 更新槽位显示
    void OnSlotChange(int slotIndex)
    {
        if (slotIndex == -1)
        {
            slot.text = "Unarmed";
        }
        else
        {
            slot.text = $"Slot: {slotIndex}";
        }
    }

    void OnLoadChanged(float value)
    {
        overload.text = $"{value:F0}%";

        overload.color = value >= 100 ? Color.red : Color.white;
    }
}
