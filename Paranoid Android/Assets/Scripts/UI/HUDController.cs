using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Image hpFill;
    public TextMeshProUGUI hpText;
    public Image overloadFill;
    public TextMeshProUGUI overloadText;
    public Gradient overloadGradient;
    public HotbarController hotbar;

    private PlayerStateData _stateData;

    public void Initialize(PlayerController pc)
    {
        Cleanup();
        _stateData = pc.stateData;
        if (_stateData == null) return;
        _stateData.OnHpChanged += OnHpChanged;
        _stateData.OnLoadChanged += OnLoadChanged;
        OnHpChanged(_stateData.hp, _stateData.maxHp);
        OnLoadChanged(_stateData.currentLoad, _stateData.maxLoad);
        hotbar.Initialize(pc);
    }

    public void Cleanup()
    {
        if (_stateData != null)
        {
            _stateData.OnHpChanged -= OnHpChanged;
            _stateData.OnLoadChanged -= OnLoadChanged;
            _stateData = null;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void OnHpChanged(int currentHp, int maxHp)
    {
        if (maxHp <= 0) return;

        float pct = Mathf.Clamp01(currentHp / (float)maxHp);
        hpFill.fillAmount = pct;

        hpText.SetText("{0}%", Mathf.RoundToInt(pct * 100f));
    }

    private void OnLoadChanged(float currentLoad, float maxLoad)
    {
        if (maxLoad <= 0) return;

        float pct = Mathf.Clamp01(currentLoad / maxLoad);
        overloadFill.fillAmount = pct;

        overloadText.SetText("{0}%", Mathf.RoundToInt(pct * 100f));

        Color targetColor;
        if (_stateData != null && _stateData.overloaded)
        {
            targetColor = overloadGradient.Evaluate(1f);
        }
        else
        {
            targetColor = overloadGradient.Evaluate(pct);
        }
        targetColor.a = 0.6f;
        overloadFill.color = targetColor;
    }
}
