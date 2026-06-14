using UnityEngine;
using TMPro;
using MyPool;

public class DamageText : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    private float _lifeTimer;
    private Vector3 _moveVector;
    private string _poolKey;

    private float MAX_LIFE_TIME = 0.8f;

    private void Awake()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _poolKey = GetComponent<PoolItem>().key;
    }

    public void Init(int damageAmount, Vector3 startPos, bool isCrit)
    {
        transform.position = startPos;
        _textMesh.text = damageAmount.ToString();
        _lifeTimer = MAX_LIFE_TIME;

        if (isCrit)
        {
            _textMesh.fontSize = 50f;
            _textMesh.fontStyle = FontStyles.Underline;
            _textMesh.color = new Color(1.0f, 0.12f, 0.22f);
            _moveVector = new Vector3(Random.Range(-1.75f, 1.75f), 4f, Random.Range(-1.25f, 1.25f));
        }
        else
        {
            _textMesh.fontSize = 45f;
            _textMesh.fontStyle = FontStyles.Normal;
            _textMesh.color = Color.white;
            _moveVector = new Vector3(Random.Range(-1.5f, 1.5f), 3.5f, Random.Range(-1f, 1f));
        }

        _textMesh.alpha = 1f;
    }

    void Update()
    {
        // 模拟抛物线位移
        transform.position += _moveVector * Time.deltaTime;
        _moveVector.y -= 9.8f * Time.deltaTime;

        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer < 0.2f)
        {
            float progress = Mathf.Clamp01(_lifeTimer / 0.2f);
            _textMesh.alpha = progress;
        }

        if (_lifeTimer <= 0f)
        {
            _textMesh.alpha = 0f;
            PoolManager.Instance.popup.Release(gameObject, _poolKey);
        }
    }
}