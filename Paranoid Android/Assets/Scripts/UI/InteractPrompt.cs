using UnityEngine;
using MyPool;
using TMPro;

public class InteractPrompt : MonoBehaviour
{
    private Transform _targetHost;
    private float _heightOffset;

    public void Init(Transform targetHost, float heightOffset)
    {
        _targetHost = targetHost;
        _heightOffset = heightOffset;
        UpdatePosition();
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_targetHost != null)
        {
            if (Camera.main != null)
            {
                transform.forward = Camera.main.transform.forward;
            }
            transform.position = _targetHost.position + Vector3.up * _heightOffset;
        }
    }
}