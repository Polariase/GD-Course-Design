using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [Header("引用")]
    public Renderer playerRenderer;
    private PlayerStateData _stateData;

    [Header("Shader属性名称")]
    public string intensityPropertyName = "_FlowIntensity";
    public string burnIntensityName = "_BurnIntensity";

    public float maxBurnIntensity = 1f;

    private Material _playerMaterial;
    private int _intensityID;
    private int _burnIntensityID;

    void Awake()
    {
        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();

        _playerMaterial = playerRenderer.material;

        _intensityID = Shader.PropertyToID(intensityPropertyName);
        _burnIntensityID = Shader.PropertyToID(burnIntensityName);

        SetElectric(0f);
    }

    private void Start()
    {
        _stateData = PlayerController.Instance.stateData;
        _stateData.OnLoadChanged += OnLoadChanged;
    }

    void OnLoadChanged(float load,float maxLoad)
    {
        float weight = Mathf.InverseLerp(maxLoad / 2f, maxLoad, load);
        float currentBurn = weight * maxBurnIntensity;
        _playerMaterial.SetFloat(_burnIntensityID, currentBurn);
    }

    public void SetElectric(float intensity)
    {
        _playerMaterial.SetFloat(_intensityID, intensity);
    }
}
