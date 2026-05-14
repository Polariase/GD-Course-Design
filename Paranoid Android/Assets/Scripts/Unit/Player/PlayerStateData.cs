using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStateData
{
    [Header("Slot")]
    public int currentSelectedIndex = 0;
    public InventoryItem currentSelectedItem;
    public Action<int, InventoryItem> OnSelectedChanged;

    public void UpdateSelection(int index,InventoryItem newItem)
    {
        currentSelectedIndex = index;
        currentSelectedItem = newItem;
        OnSelectedChanged?.Invoke(currentSelectedIndex, newItem);
    }

    [Header("Overload")]
    public float currentLoad = 0;
    public Action<float> OnLoadChanged;
    public bool overloaded;
    public float coolingDelay = 1f;
    public float initialCoolingRate = 10f;
    public float coolingAcceleration = 15f;
    private float _lastOverload;

    public float OverloadPercentage => Mathf.InverseLerp(0f, 100f, currentLoad);

    public void Overload(float value)
    {
        currentLoad += value;
        _lastOverload = 0f;
        if (currentLoad >= 100f)
            overloaded = true;
        OnLoadChanged?.Invoke(currentLoad);
    }

    public void Cooling(float deltaTime)
    {
        if (currentLoad <= 0) return;

        _lastOverload += deltaTime;

        if (_lastOverload >= coolingDelay)
        {
            float coolingDuration = _lastOverload - coolingDelay;
            float currentRate = initialCoolingRate + (coolingAcceleration * coolingDuration);
            currentLoad -= currentRate * deltaTime;
            currentLoad = Mathf.Max(currentLoad, 0f);
            if (overloaded && currentLoad <= 0f)
                overloaded = false;

            OnLoadChanged?.Invoke(currentLoad);
        }
    }
}
