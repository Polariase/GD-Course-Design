using UnityEngine;
using TMPro;
using MyPool;

public enum WordTextType
{
    Good,
    Neutral,
    Bad
}

public class WordText : MonoBehaviour
{
    public TextMeshProUGUI textField;
    public float floatSpeed = 1.5f;

    public Color colorGood = new Color32(50, 255, 160, 255);
    public Color colorNeutral = new Color32(230, 235, 240, 255);
    public Color colorBad = new Color32(255, 45, 85, 255);

    private Transform _targetHost;
    private float _heightOffset;
    private float _lifeTime;
    private float _timer;
    private string _poolKey;

    private Vector3 _floatOffset;

    private void Awake()
    {
        textField = GetComponent<TextMeshProUGUI>();
        _poolKey = GetComponent<PoolItem>().key;
    }

    public void Init(Vector3 spawnPos, Transform targetHost, string text, WordTextType type, float duration)
    {
        _targetHost = targetHost;
        _lifeTime = duration;
        _timer = 0f;
        _floatOffset = Vector3.zero;

        if (textField != null)
        {
            textField.text = text;
            textField.color = type switch
            {
                WordTextType.Good => colorGood,
                WordTextType.Bad => colorBad,
                _ => colorNeutral
            };
        }

        if (_targetHost != null)
        {
            _heightOffset = spawnPos.y - _targetHost.position.y;
        }
        else
        {
            transform.position = spawnPos;
        }

        UpdatePositionAndOrientation();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            PoolManager.Instance.popup.Release(gameObject, _poolKey);
            return;
        }

        UpdatePositionAndOrientation();
    }

    private void UpdatePositionAndOrientation()
    {
        if (Camera.main != null)
        {
            _floatOffset += Camera.main.transform.up * (floatSpeed * Time.deltaTime);
            transform.forward = Camera.main.transform.forward;
        }

        if (_targetHost != null)
        {
            transform.position = _targetHost.position + (Vector3.up * _heightOffset) + _floatOffset;
        }
        else
        {
            transform.position += Vector3.up * (floatSpeed * Time.deltaTime);
        }
    }
}