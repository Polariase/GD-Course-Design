using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    public TextMeshProUGUI overload;
    private PlayerStateData _stateData;


    private void Start()
    {
        if(_stateData == null)
        {
            _stateData = PlayerController.Instance.stateData;
            _stateData.OnLoadChanged += OnLoadChanged;
        }
    }

    private void OnEnable()
    {
        if (_stateData != null)
        {
            _stateData.OnLoadChanged += OnLoadChanged;
        }
    }

    private void OnDisable()
    {
        if (_stateData != null)
        {
            _stateData.OnLoadChanged -= OnLoadChanged;
        }
    }

    void OnLoadChanged(float value)
    {
        overload.text = $"{value:F0}%";

        overload.color = value >= 100 ? Color.red : Color.white;
    }
}
