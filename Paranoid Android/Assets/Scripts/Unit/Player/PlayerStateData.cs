using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStateData
{
    //物品栏
    public int currentSelectedIndex = 0;
    public InventoryItem currentSelectedItem;
    public Action<int, InventoryItem> OnSelectedChanged;

    public void ClearAllSubscribers()
    {
        OnSelectedChanged = null;
        OnHpChanged = null;
        OnLoadChanged = null;
    }

    public void ResetStatus(bool heal)
    {
        if (heal)
            hp = maxHp;
        currentLoad = 0f;
        overloaded = false;
        currentSelectedIndex = 0;
        currentSelectedItem = null;
        _coolingTimer = 0f;
    }

    public void UpdateSelection(int index,InventoryItem newItem)
    {
        currentSelectedIndex = index;
        currentSelectedItem = newItem;
        OnSelectedChanged?.Invoke(currentSelectedIndex, newItem);
    }

    //角色属性
    public int maxHp = 100;
    public int hp = 100;
    public Action<int, int> OnHpChanged;

    public float maxLoad = 100f;
    public float currentLoad = 0;
    public Action<float, float> OnLoadChanged;

    public bool overloaded;
    public float coolingDelay = 1f;
    public float initialCoolingRate = 10f;
    public float coolingAcceleration = 15f;
    private float _coolingTimer;

    public float OverloadPercentage => Mathf.InverseLerp(0f, maxLoad, currentLoad);

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        hp = Mathf.Max(hp - damage, 0);
        OnHpChanged?.Invoke(hp, maxHp);

        if (hp <= 0f)
        {
            // 通过其他事件通知玩家死亡
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        hp = Mathf.Min(hp + amount, maxHp);
        OnHpChanged?.Invoke(hp, maxHp);
    }

    public void ModifyMaxHp(int newMaxHp, bool healToMax = false)
    {
        float oldMax = maxHp;
        maxHp = Mathf.Max(newMaxHp, 1);

        if (healToMax)
        {
            hp = maxHp;
        }
        else
        {
            float pct = hp / oldMax;
            hp = (int)(maxHp * pct);
        }
        OnHpChanged?.Invoke(hp, maxHp);
    }

    public void Overload(float value)
    {
        currentLoad += value;
        _coolingTimer = 0f;
        if (currentLoad >= maxLoad)
            overloaded = true;
        OnLoadChanged?.Invoke(currentLoad, maxLoad);
    }

    public void Cooling(float deltaTime)
    {
        if (currentLoad <= 0) return;

        _coolingTimer += deltaTime;

        if (_coolingTimer >= coolingDelay)
        {
            float coolingDuration = _coolingTimer - coolingDelay;
            float currentRate = initialCoolingRate + (coolingAcceleration * coolingDuration);
            currentLoad -= currentRate * deltaTime;
            currentLoad = Mathf.Max(currentLoad, 0f);
            if (overloaded && currentLoad <= 0f)
                overloaded = false;

            OnLoadChanged?.Invoke(currentLoad, maxLoad);
        }
    }

    public void ModifyMaxLoad(float newMaxLoad)
    {
        maxLoad = Mathf.Max(newMaxLoad, 1f);
        if (currentLoad >= maxLoad)
        {
            currentLoad = maxLoad;
            if (!overloaded)
            {
                overloaded = true;
            }
        }
        OnLoadChanged?.Invoke(currentLoad, maxLoad);
    }
}
