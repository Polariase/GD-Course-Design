using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [Header("渲染器引用")]
    public Renderer playerRenderer;

    [Header("Shader属性名称")]
    public string intensityPropertyName = "_FlowIntensity";

    private Material _playerMaterial;
    private int _intensityID;

    void Awake()
    {
        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();

        _playerMaterial = playerRenderer.material;

        _intensityID = Shader.PropertyToID(intensityPropertyName);

        SetElectric(0f);
    }

    public void SetElectric(float intensity)
    {
        _playerMaterial.SetFloat(_intensityID, intensity);
    }
}
