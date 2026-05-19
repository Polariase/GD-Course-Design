using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "SOs/WeaponData")]
public class WeaponData : ItemData
{
    public float fireRate;
    public float loadPerShot;

    public float baseSpread;     
    public float aimSpreadMult;
    public float aimSpeed;

    public int damage;
    public float bulletSpeed;
    public float distance;

    public string modelAddress;
}