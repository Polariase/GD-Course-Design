using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateData", menuName = "SOs/PlayerStateData")]
public class PlayerStateData : ScriptableObject
{
    [Header("Slot")]
    public int currentSelectedIndex = 0;
    public Action<int> OnSlotChanged;

    public void SelectSlot(int index)
    {
        if (currentSelectedIndex == index) currentSelectedIndex = 0;
        else currentSelectedIndex = index;

        OnSlotChanged?.Invoke(currentSelectedIndex);
    }

    [Header("Overload")]
    public float currentLoad = 0;
    public Action<float> OnLoadChanged;
    public bool overloaded;
    public float coolingDelay = 1f;
    public float initialCoolingRate = 10f;
    public float coolingAcceleration = 15f;
    private float _lastOverload;

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
