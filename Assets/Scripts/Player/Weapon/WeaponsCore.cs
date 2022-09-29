using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ImpactEffectType
{ //On impact:
    none,       //do nothing special.
    shatter,    //shatter the projectile into small fragments that can damage enemies behind the target. Flying enemies may fall down and can't return up.
    fire,       //ignite the target on fire and do constant damage for a limited time. Effective on frozen enemies.
    ice,        //slow or freeze enemy movement. Add fire and do extra damage to the target.
    electricity,//stun the target and do constant damage over time. Has a change to spread to nearby enemies if target is over chagred.
    acid,       //do small constant damage over time and temporarily lower targets defence values or permanently destroy physical shields.
    explosion,  //do damage and knockback enemies on a certain radius. Use on a enemy that is suffering of fire or acid to spread the corresponding effects.
    deployWeapon //deploy an independent weapon.
}

public class WeaponsCore : MonoBehaviour
{
    private ClassLibrary lib;
    private WeaponAim weaponAim;
    private WeaponShooting weaponShooting;
    private RaycastHit[] _aimTargetDatas;
    public Transform _barrelTransform;

    private void Awake()
    {
        lib = FindObjectOfType<ClassLibrary>();
        weaponAim = GetComponent<WeaponAim>();
        weaponShooting = GetComponent<WeaponShooting>();
    }

    public void SetBarrelTransform(Transform transform)
    {
        if (transform == null)
        {
            _barrelTransform = this.transform;
            return;
        }
        Transform barrelTransform = transform.GetComponent<WeaponModelComponent>()._weaponBarrelEndTransform;
        if (!barrelTransform)
        {
            _barrelTransform = this.transform;
            return;
        }
        _barrelTransform = barrelTransform;
    }

    public void GetAimData(AimMode aimMode)
    {
        _aimTargetDatas = weaponAim.AimCoreControll(aimMode);
        //if (aimTargetDatas == null)
        //{ return; }
    }

    public void FireWeapon(bool fireReleased)
    {
        if (!fireReleased)
            weaponShooting.WeaponSystem(_aimTargetDatas);
        else
            weaponShooting.OneExistingProjectileDeactivateTrigger();
    }
}
