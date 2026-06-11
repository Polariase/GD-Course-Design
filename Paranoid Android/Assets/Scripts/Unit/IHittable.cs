using UnityEngine;

public interface IHittable
{
    bool Hit(float damage, RaycastHit hitInfo);

    bool TakeDamage(float damage);

    Vector3 HitPoint();
}