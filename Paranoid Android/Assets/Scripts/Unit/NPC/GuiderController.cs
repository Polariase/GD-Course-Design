using System.Collections.Generic;
using UnityEngine;

public class GuiderController : UnitController, IInteractable
{
    public string ActionName => "交谈";
    public Transform headBone;
    public float turnSpeed = 5f;
    public float maxVisualDistance = 7f;
    public GameObject borderTeleporter;

    private int _dialogueIndex = 0;
    private List<string> _quotes;
    private Quaternion _initialHeadRotation;

    protected override void Awake()
    {
        base.Awake();

        _quotes = new List<string>
        {
            "如你所见，这里已经没有人类了。",
            "你知道有什么事比沉睡一千年更糟糕吗？那就是在沉睡一千年醒来后必须去完成一件不可完成的任务。",
            "死亡意味着什么？对于人类的话，我并不知道；但对于你，意味着我又要从茫茫的世界中将你的尸块检索回来。",
            "你觉得这里还不错？但是我已经受够这里了。",
            "样本，我需要更多数据样本。"
        };

        _initialHeadRotation = headBone.localRotation;

        borderTeleporter.SetActive(GameManager.Instance.reward200Claimed);
    }

    private void Update()
    {
        HandleHeadLookAt();
    }

    private void HandleHeadLookAt()
    {
        if (headBone == null || PlayerController.Instance == null || isDead) return;
        Vector3 player = PlayerController.Instance.transform.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player);
        if (distanceToPlayer > maxVisualDistance)
        {
            ResetHeadRotation();
            return;
        }

        Vector3 directionToPlayer = (player - headBone.position).normalized;
        Vector3 localDirection = transform.InverseTransformDirection(directionToPlayer);
        float targetYaw = Mathf.Atan2(localDirection.z, localDirection.x) * Mathf.Rad2Deg;
        targetYaw = Mathf.Clamp(targetYaw, -45f, 45f);
        Quaternion targetLocalRotation = _initialHeadRotation * Quaternion.Euler(0, 0, targetYaw);
        headBone.localRotation = Quaternion.Slerp(headBone.localRotation, targetLocalRotation, Time.deltaTime * turnSpeed);
    }

    private void ResetHeadRotation()
    {
        headBone.localRotation = Quaternion.Slerp(headBone.localRotation, _initialHeadRotation, Time.deltaTime * turnSpeed);
    }

    public void Interact()
    {
        if (NeedToReward())
        {
            return;
        }
        string currentSaying = _quotes[_dialogueIndex];
        WordTextType textType = WordTextType.Neutral;
        if (_dialogueIndex == 1 || _dialogueIndex == 3) textType = WordTextType.Bad;
        Say(currentSaying, 3f, textType);
        _dialogueIndex = (_dialogueIndex + 1) % _quotes.Count;
    }

    private bool NeedToReward()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return false;

        if (_dialogueIndex >=2 && gm.globalDataCount >= 0 && !gm.reward0Claimed)
        {
            gm.reward0Claimed = true;
            InventoryManager.Instance.Collect(new InventoryItem(DataManager.Instance.GetItemData(1001), 1));
            Say("现在你需要去收集样品，数据含量越高越好。\n拿上这个，外面或许并不安全。", 3f, WordTextType.Good);
            return true;
        }

        if (gm.reward0Claimed && gm.globalDataCount >= 200 && !gm.reward200Claimed)
        {
            gm.reward200Claimed = true;
            InventoryManager.Instance.Collect(new InventoryItem(DataManager.Instance.GetItemData(1002), 1));
            Say("很好，我对这个世界发生过什么已经有些头绪了。我制作了一把更好的武器，你会很需要这个的。", 3f, WordTextType.Good);
            return true;
        }

        if (gm.reward200Claimed && gm.globalDataCount >= 500 && !gm.reward500Claimed)
        {
            gm.reward500Claimed = true;
            InventoryManager.Instance.Collect(new InventoryItem(DataManager.Instance.GetItemData(1003), 1));
            Say("或许复活程序并非完全不可能完成，不过...算了，还是来看看这把新武器吧。", 3f, WordTextType.Good);
            return true;
        }

        if (gm.reward500Claimed && gm.globalDataCount >= 1000 && !gm.reward1000Claimed)
        {
            gm.reward1000Claimed = true;
            InventoryManager.Instance.Collect(new InventoryItem(DataManager.Instance.GetItemData(1004), 1));
            Say("我想我们距离得到答案已经不远了，但这到底是好事还是坏事呢...\n这是我新制作的武器，一枪就能干掉那些烦人的家伙。", 3f, WordTextType.Good);
            return true;
        }

        if (gm.reward1000Claimed && gm.globalDataCount >= 2000 && !gm.isAnalysisComplete)
        {
            gm.isAnalysisComplete = true;
            Say("你已经全都知道了吧，去世界的间隙，那里有个很有趣的家伙。\n你还真要去啊...那，别被打成筛子就好。", 3f, WordTextType.Bad);
            if (borderTeleporter.activeSelf == false) borderTeleporter.SetActive(true);
            return true;
        }

        return false;
    }
}