using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    [Header("Projectile Settings")]
    public ProjectileType _projectileType = ProjectileType._hitScan;
    [Space]
    public int _projectileDamage = 10;
    [Tooltip("0 = wont be deactivated by time.")]
    public float _projectileLifetime = 0.1f;
    public HitScanProjectileData _hitScanProjectileData;
    public DistanceProjectileData _distanceProjectileData;
    public PhysicsProjectileData _physicsProjectileData;
    /*================================Impact_Effects==============================*/
    [Space]
    [Header("Impact Effect Settings")]
    public bool _hasImpactEffect = false;
    [Tooltip("If multiple effects are in the array they are combined if possible.")]
    public ImpactEffectData[] _impactEffectDatas;

    [HideInInspector] public PooledChunk _pooledChunk;

    private PoolingManager _poolingManager;
    private int _chunk;
    private int _projectileSlot;

    public void ReceivePhysicsSlotData(int chunk, int projectileSlot, PoolingManager poolingManager)
    {
        _poolingManager = poolingManager;
        _chunk = chunk;
        _projectileSlot = projectileSlot;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_projectileType == ProjectileType._physicsProjectile)
            _poolingManager.PhysicsProjectileCollided(_chunk, _projectileSlot, collision);
    }
}

[System.Serializable]
public class DistanceProjectileData
{
    [Header("Distance Projectile Settings")]
    public float _projectileCollisionAreaSize = 1f;
    public float _constantSpeed = 3f;
}

[System.Serializable]
public class HitScanProjectileData
{
    [Header("Hit Scan Settings")]
    public float _hitScanMaxDistance = 15f;
}

[System.Serializable]
public class PhysicsProjectileData
{
    public float _startVelocityForce = 5f;
    [Header("Bounce Settings")]
    public bool _bouncesOnOff = true;
    public int _maxBounces = 2;
    public float _bounceForce = 1f;
    [Space]
    [Header("Deployable Settings")]
    public bool _isDeployableWeapon = false;
}

[System.Serializable]
public class ImpactEffectData
{
    public string _effectName = "Unnamed Effect";

    //[Header("Combining may result into an error if there is \nmore than 2 effects trying to combine into one!")]
    //public bool _combineWithBelow = false;
    public ImpactEffectType _impactEffect = ImpactEffectType.none;
    //[Space]
    //public KnockbackEffectData _knockbackEffect;
    //public ShatterEffectData _shatterEffect;
    //public FireElectricityAcidEffectsData _fireElectricityAcidEffects;
    //public IceEffectData _iceEffect;
    //public ExplosionEffectData _explosionEffect;
    //public DeployEffectData _deployEffect;
}