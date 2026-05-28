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

    private PlayerStateData _stateData;
    private bool _isInitialized;

    private void Start()
    {
        TryInitialize();
    }

    private void OnEnable()
    {
        TryInitialize();
    }

    private void OnDisable()
    {
        if (_isInitialized && _stateData != null)
        {
            _stateData.OnHpChanged -= OnHpChanged;
            _stateData.OnLoadChanged -= OnLoadChanged;
            _isInitialized = false;
        }
    }

    private void TryInitialize()
    {
        if (_isInitialized) return;

        if (PlayerController.Instance != null && PlayerController.Instance.stateData != null)
        {
            _stateData = PlayerController.Instance.stateData;

            _stateData.OnHpChanged += OnHpChanged;
            _stateData.OnLoadChanged += OnLoadChanged;

            OnHpChanged(_stateData.hp, _stateData.maxHp);
            OnLoadChanged(_stateData.currentLoad, _stateData.maxLoad);

            _isInitialized = true;
        }
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
