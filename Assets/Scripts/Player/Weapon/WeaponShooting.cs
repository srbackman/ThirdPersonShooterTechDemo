using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public enum ProjectileType
{
    _hitScan,
    _distanceProjectile,
    _physicsProjectile,
    _melee
}

public class WeaponShooting : MonoBehaviour
{
    private PoolingManager poolingManager;
    private WeaponsWheel weaponsWheel;
    private WeaponsCore weaponsCore;
    private ProjectileComponent _beamProjectileComponent = null;

    private void Awake()
    {
        weaponsWheel = GetComponent<WeaponsWheel>();
        weaponsCore = GetComponent<WeaponsCore>();
        poolingManager = FindObjectOfType<PoolingManager>();
    }

    public void WeaponSystem(RaycastHit[] targets)
    {
        if (weaponsCore._barrelTransform) ShotLoop(targets);
    }

    public void OneExistingProjectileDeactivateTrigger()
    {
        if (_beamProjectileComponent == null) return;
        _beamProjectileComponent.transform.parent.parent.parent.gameObject.SetActive(false);
        _beamProjectileComponent._pooledChunk._projectilesInUse--;
        _beamProjectileComponent._pooledChunk = null;
        _beamProjectileComponent.gameObject.SetActive(false);
        _beamProjectileComponent = null;
    }

    private void ShotLoop(RaycastHit[] targetRays)
    {
        if (!weaponsWheel._currentScriptableObjectWeapon) return;
        int weaponLevel = weaponsWheel._currentWeaponLevel;
        WeaponLevelData weaponLevelData = weaponsWheel._currentScriptableObjectWeapon._weaponLevels[weaponLevel];
        for (int i = 0; i < weaponLevelData._projectilesShotPerTrigger; i++)
        { 
            /*Calculate random projectile direction.*/
            Vector2 maxCircle = new Vector2(WeaponAccuracy(weaponLevelData), WeaponAccuracy(weaponLevelData)).normalized;
            Vector2 finalCircle = new Vector2(UnityEngine.Random.Range(-maxCircle.x, maxCircle.x), UnityEngine.Random.Range(-maxCircle.y, maxCircle.y));
            Vector3 finalRandomDirection = new Vector3(finalCircle.x, finalCircle.y, weaponLevelData._weaponShotAccuracy - 1f);
            /*Add weapon barrel orjentation and aim assist strenght.*/
            Vector3 targetDirection = weaponsCore._barrelTransform.TransformDirection(finalRandomDirection).normalized;

            RaycastHit targetRay = new RaycastHit();
            if (targetRays != null && i < targetRays.Length) targetRay = targetRays[i];
            if (targetRay.transform != null)
            {
                targetDirection = (targetDirection + (targetRay.point - weaponsCore._barrelTransform.position) * weaponLevelData._weaponAimAssistStrength).normalized;
                Debug.DrawRay(weaponsCore._barrelTransform.position, targetDirection * 20, Color.blue);
            }

            ChooseFiringType(weaponsCore._barrelTransform, targetDirection, weaponsWheel._currentScriptableObjectWeapon._weaponName,
                weaponLevel, weaponsWheel._currentScriptableObjectWeapon._firingMode);
        }
    }

    private void ChooseFiringType(Transform barrelTransform, Vector3 direction, string weaponName, int level, FiringMode firingMode)
    {
        if (FiringMode.holdProjectile == firingMode)
        {
            if (_beamProjectileComponent == null)
            {
                _beamProjectileComponent = poolingManager.GetProjectileComponent(weaponName, level);/*Get free projectile slot.*/
            }
            FireHitScan(barrelTransform, direction, _beamProjectileComponent);
            return;
        }
        ProjectileComponent projectileComponent = poolingManager.GetProjectileComponent(weaponName, level);/*Get free projectile slot.*/
        switch (projectileComponent._projectileType)
        {
            case ProjectileType._distanceProjectile: FireDistanceProjectile(barrelTransform, direction, projectileComponent); break;
            case ProjectileType._hitScan: FireHitScan(barrelTransform, direction, projectileComponent); break;
            case ProjectileType._physicsProjectile: FirePhysicsProjectile(barrelTransform, direction, projectileComponent); break;
        }
    }

    /*HitScan: shoot using Raycast.*/
    private void FireHitScan(Transform barrelTransform, Vector3 direction, ProjectileComponent projectileComponent)
    {
        RaycastHit hit;
        LineRenderer lineRenderer = projectileComponent.transform.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, barrelTransform.position);
        float projectileMaxDistance = projectileComponent._hitScanProjectileData._hitScanMaxDistance;
        Physics.Raycast(barrelTransform.position, direction, out hit, projectileMaxDistance);
        if (hit.transform)
        { /*Hit.*/
            Debug.DrawRay(barrelTransform.position, direction * hit.distance, Color.green);
            ReceiveDamage receiver = hit.collider.GetComponent<ReceiveDamage>();
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, /*barrelTransform.position + (direction * hit.distance)*/ hit.point);
            if (receiver)
            {
                receiver.TakeDamage(projectileComponent._projectileDamage, hit.point, projectileComponent._impactEffectDatas);
            }
        }
        else
        { /*No hit.*/
            Debug.DrawRay(barrelTransform.position, direction * projectileMaxDistance, Color.red);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, barrelTransform.position + (direction * projectileMaxDistance));
        }
    }

    private void FireDistanceProjectile(Transform barrelTransfrom, Vector3 direction, ProjectileComponent projectileComponent)
    {
        Transform projectileTransform = projectileComponent.transform;
        projectileTransform.position = barrelTransfrom.position;
        projectileTransform.rotation = Quaternion.LookRotation(direction);
    }

    private void FirePhysicsProjectile(Transform barrelTransfrom, Vector3 direction, ProjectileComponent projectileComponent)
    {
        Transform projectileTransform = projectileComponent.transform;
        projectileTransform.position = barrelTransfrom.position;
        projectileComponent.transform.parent.parent.parent.gameObject.SetActive(true);
        projectileTransform.rotation = Quaternion.LookRotation(direction);
        Rigidbody rb = projectileComponent.GetComponent<Rigidbody>();
        if (!rb) Debug.LogError("Rigidbody not found.");
        rb.velocity = direction * (projectileComponent._physicsProjectileData._startVelocityForce/* * Time.deltaTime*/);
    }

    private float WeaponAccuracy(WeaponLevelData weaponLevelData)
    {
        float inputAccuracy = weaponLevelData._weaponShotAccuracy;
        float outputAccuracy = UnityEngine.Random.Range(-inputAccuracy, inputAccuracy);
        return(outputAccuracy);
    }
}
