using UnityEngine;
using MyPool;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    public PopupPool popupPool;
    public Transform popupCanvas;

    public string damageTextKey = "DamageText";
    public string interactPromptKey = "InteractPrompt";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        popupPool = PoolManager.Instance.popup;
    }

    public void ShowDamage(Vector3 worldPosition, int damageAmount, bool isCrit)
    {
        if (popupPool == null) return;

        // Ëæ»úÆ«ÒÆ·ÀÖ¹ÖØµþ
        Vector3 finalPos = worldPosition + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.2f, 0.4f), Random.Range(-0.2f, 0.2f));
        popupPool.GetAndSet(damageTextKey, finalPos, damageAmount, isCrit);
    }

    public GameObject ShowInteractPrompt(Vector3 targetWorldPos, float heightOffset = 1.5f)
    {
        if (popupPool == null) return null;
        Vector3 spawnPos = targetWorldPos + Vector3.up * heightOffset;
        GameObject promptGo = popupPool.Get(interactPromptKey, spawnPos);

        return promptGo;
    }

    public void HideInteractPrompt(GameObject promptGo)
    {
        popupPool.Release(promptGo, interactPromptKey);
    }
}