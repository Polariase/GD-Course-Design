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
    public string dissolveLevelName = "_DissolveLevel";

    public float maxBurnIntensity = 1f;
    public float dissolveDuration = 1.5f;

    private static MaterialPropertyBlock _mpb;
    private int _intensityID;
    private int _burnIntensityID;
    private int _dissolveLevelID;
    private Coroutine _dissolveCoroutine;
    private const float DISSOLVE_HIDDEN = -0.45f;
    private const float DISSOLVE_SHOWN = 1.2f;

    void Awake()
    {
        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        _intensityID = Shader.PropertyToID(intensityPropertyName);
        _burnIntensityID = Shader.PropertyToID(burnIntensityName);
        _dissolveLevelID = Shader.PropertyToID(dissolveLevelName);

        SetDissolve(true);
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
        playerRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(_burnIntensityID, currentBurn);
        playerRenderer.SetPropertyBlock(_mpb);
    }

    public void SetElectric(float intensity)
    {
        playerRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(_intensityID, intensity);
        playerRenderer.SetPropertyBlock(_mpb);
    }

    public void SetDissolve(bool show)
    {
        if (_dissolveCoroutine != null)
        {
            StopCoroutine(_dissolveCoroutine);
        }

        _dissolveCoroutine = StartCoroutine(DissolveRoutine(show));
    }

    private IEnumerator DissolveRoutine(bool show)
    {
        playerRenderer.GetPropertyBlock(_mpb);
        float startValue = _mpb.GetFloat(_dissolveLevelID);

        float targetValue = show ? DISSOLVE_SHOWN : DISSOLVE_HIDDEN;
        float elapsed = 0f;

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);
            float currentValue = Mathf.Lerp(startValue, targetValue, t);
            SetDissolveProperty(currentValue);
            yield return null;
        }

        SetDissolveProperty(targetValue);
        _dissolveCoroutine = null;
    }

    private void SetDissolveProperty(float value)
    {
        playerRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(_dissolveLevelID, value);
        playerRenderer.SetPropertyBlock(_mpb);
    }
}
