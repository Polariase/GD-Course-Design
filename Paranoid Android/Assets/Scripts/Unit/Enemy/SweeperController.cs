using UnityEngine;

public class SweeperController : RobotSphereController
{
    public Transform chipPos;

    public float upwardForce = 8f;
    public float forwardForce = 2f;

    public override async void OnDeathAnimationFinished()
    {
        Vector3 throwDir = transform.forward;
        if (currentTarget != null)
        {
            throwDir = (currentTarget.position - transform.position).normalized;
        }
        Vector3 spawnPos = chipPos != null ? chipPos.position : HitPoint();
        await PoolManager.Instance.item.SpawnAndThrowItemAsync(new InventoryItem(DataManager.Instance.GetItemData(2004), 1), spawnPos, throwDir, upwardForce, forwardForce);
    }
}