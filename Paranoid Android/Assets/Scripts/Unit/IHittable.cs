using UnityEngine;

public interface IHittable
{
    bool Hit(int damage, Vector3 hitPoint, bool isCrit);

    bool TakeDamage(int damage, Vector3 hitPoint, bool isCrit);

    Vector3 HitPoint();
}