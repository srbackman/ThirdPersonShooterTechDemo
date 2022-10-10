using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileTarget
{
    enemy,
    player
}

public class PooledProjectile
{ /*Holds a single projectile.*/
    public Transform _assignedWeaponPrefab;
    public float _projectileTimeLeft;
    public float _visualImpactEffectTimeLeft;
    public bool _oneTimeImpactEffect;
    public int _totalBounces;
    public ProjectileCore _projectileCore;
    public ProjectileComponent _activeProjectile;
}

public class PooledChunk
{ /*Holds projectile pool chunks.*/
    public int _projectilesInUse = 0;
    public PooledProjectile[] _pooledProjectiles;
}

public class PoolingManager : MonoBehaviour
{
    private ClassLibrary lib;
    [SerializeField] private int _poolChunkCreationSize = 64;
    [Space]
    [Header("ProjectilesPoolStuff")]
    private PooledChunk[] _pooledProjectileChunks;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectilesParent;
    [SerializeField] private LayerMask _hittableLayers;
    [Space]
    [Header("AutoAimSightsPoolStuff")]
    private RectTransform[] _pooledAutoAimSights;
    [SerializeField] private RectTransform _autoAimSightPrefab;
    [SerializeField] private Transform _autoAimSightsParent;


    private void Awake()
    {
        lib = FindObjectOfType<ClassLibrary>();
        _pooledAutoAimSights = new RectTransform[_poolChunkCreationSize];
        if (!_autoAimSightPrefab) Debug.LogError("AutoAimSight prefab missing!");
        if (!_projectilePrefab) Debug.LogError("Projectile prefab missing!");
        InstantiateEmptyAutoAimSlots();
        ExpandProjectilePool();
    }

    void Update()
    {
        PooledProjectilesManager();
    }

    private void ExpandProjectilePool()
    {
        /*Copy existing chunks.*/
        PooledChunk[] tempPooledProjectilesChunks = null;
        if (_pooledProjectileChunks != null)
        {
            tempPooledProjectilesChunks = new PooledChunk[_pooledProjectileChunks.Length];
            for (int i = 0; i < tempPooledProjectilesChunks.Length; i++)
            {
                tempPooledProjectilesChunks[i] = _pooledProjectileChunks[i];
            }
        }
        /*Create expanded chunk array*/
        _pooledProjectileChunks = new PooledChunk[_pooledProjectileChunks == null ? 1 : _pooledProjectileChunks.Length + 1];
        _pooledProjectileChunks[_pooledProjectileChunks.Length - 1] = new PooledChunk();
        /*Add existing chunks back.*/
        if (tempPooledProjectilesChunks != null)
        {
            for (int i = 0; i < tempPooledProjectilesChunks.Length; i++)
            {
                _pooledProjectileChunks[i] = tempPooledProjectilesChunks[i];
            }
        }
        InstantiateNewProjectileSlots();
    }

    /*Find a free slot and set the correct projectile gameobject active.*/
    public ProjectileComponent GetProjectileComponent(string weaponName, int level)
    {
        int chunk = 0;
        while (true)
        {
            if (_pooledProjectileChunks[chunk]._projectilesInUse != _poolChunkCreationSize)
            {
                for (int projectileSlot = 0; projectileSlot < _pooledProjectileChunks[chunk]._pooledProjectiles.Length; projectileSlot++)
                {
                    PooledProjectile pooledProjectile = _pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot];
                    if (pooledProjectile._projectileCore.gameObject.activeSelf)
                        continue;
                    _pooledProjectileChunks[chunk]._projectilesInUse++;
                    ProjectileComponent projectileComponent = FindProjectileComponent(pooledProjectile._projectileCore, weaponName, level);
                    if (projectileComponent == null)
                        Debug.LogError("ProjectileComponent not found for: " + weaponName);
                    if (projectileComponent._projectileType != ProjectileType._physicsProjectile)
                        pooledProjectile._projectileCore.gameObject.SetActive(true);
                    pooledProjectile._activeProjectile = projectileComponent;
                    pooledProjectile._projectileTimeLeft = projectileComponent._projectileLifetime;
                    projectileComponent._pooledChunk = _pooledProjectileChunks[chunk];
                    if (projectileComponent._projectileType == ProjectileType._physicsProjectile)
                    {
                        pooledProjectile._totalBounces = 0;
                        projectileComponent.ReceivePhysicsSlotData(chunk, projectileSlot, this);
                    }
                    return (projectileComponent);
                }
            }
            chunk++;
            if (chunk >= _pooledProjectileChunks.Length) ExpandProjectilePool();
        }
    }

    /*Find the correct projectile gameobject*/
    private ProjectileComponent FindProjectileComponent(ProjectileCore projectileCore, string weaponName, int level)
    {
        int i = 0;
        while (i < projectileCore._weaponsProjectileFolders.Length && projectileCore._weaponsProjectileFolders[i]._weaponName != weaponName)
        {
            i++;
        }
        if (i >= projectileCore._weaponsProjectileFolders.Length) { Debug.LogError("Projectile name could not be found: " + weaponName); return(null); }
        ProjectileComponent projectileComponent = projectileCore._weaponsProjectileFolders[i]._projectileLevelComponents[level];
        projectileComponent.gameObject.SetActive(true);
        return (projectileComponent);
    }

    public RectTransform SpawnAutoAimTarget()
    {
        for(int i = 0; i < _pooledAutoAimSights.Length; i++)
        {
            /*Set curreent slot(i) active and return it.*/
            if (!_pooledAutoAimSights[i].gameObject.activeSelf)
            {
                _pooledAutoAimSights[i].gameObject.SetActive(true);
                return (_pooledAutoAimSights[i]);
            }
            /*Create more AutoAimSights.*/
            if ((i + 1) >= _pooledAutoAimSights.Length)
            {
                _pooledAutoAimSights = lib.genericFunctions.ExpandArray<RectTransform>(_pooledAutoAimSights, _poolChunkCreationSize);
                InstantiateEmptyAutoAimSlots();
            }
        }
        return (new RectTransform());
    }

    private void InstantiateEmptyAutoAimSlots()
    {
        for (int i = 0; i < _pooledAutoAimSights.Length; i++)
        {
            if (_pooledAutoAimSights[i]) continue;
            _pooledAutoAimSights[i] = Instantiate(_autoAimSightPrefab, _autoAimSightsParent);
            _pooledAutoAimSights[i].gameObject.SetActive(false);
        }
    }

    private void InstantiateNewProjectileSlots()
    {
        int chunksLenght = _pooledProjectileChunks.Length == 0 ? 1 : _pooledProjectileChunks.Length;
        _pooledProjectileChunks[chunksLenght - 1]._pooledProjectiles = new PooledProjectile[_poolChunkCreationSize];
        for (int i = 0; i < _pooledProjectileChunks[chunksLenght - 1]._pooledProjectiles.Length; i++)
        {
            if (_pooledProjectileChunks[chunksLenght - 1]._pooledProjectiles[i] != null) continue;
            _pooledProjectileChunks[chunksLenght - 1]._pooledProjectiles[i] = new PooledProjectile();
            Transform transform = Instantiate(_projectilePrefab, _projectilesParent).transform;
            _pooledProjectileChunks[chunksLenght - 1]._pooledProjectiles[i]._assignedWeaponPrefab = transform;
            _pooledProjectileChunks[chunksLenght - 1]._pooledProjectiles[i]._projectileCore = transform.GetComponent<ProjectileCore>();
            _pooledProjectileChunks[chunksLenght - 1]._pooledProjectiles[i]._assignedWeaponPrefab.gameObject.SetActive(false);
        }
    }

    private void PooledProjectilesManager()
    {
        /*Loop through Chunks*/
        for (int chunk = 0; chunk < _pooledProjectileChunks.Length; chunk++)
        {
            if (_pooledProjectileChunks[chunk]._projectilesInUse != 0)
            {
                /*Loop through Pooled Projectiles inside a Chunk*/
                for (int projectileSlot = 0; projectileSlot < _pooledProjectileChunks[chunk]._pooledProjectiles.Length; projectileSlot++)
                {
                    if (!_pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot]._projectileCore.gameObject.activeSelf)
                        continue;
                    if (CountDownPooledProjectileTimers(chunk, projectileSlot)) 
                        MovePooledProjectiles(chunk, projectileSlot);
                }
            }
        }
    }

    private bool CountDownPooledProjectileTimers(int chunk, int projectileSlot)
    {
        PooledProjectile pooledProjectile = _pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot];
        if ((pooledProjectile._activeProjectile._projectileLifetime == 0))
            return (false);
        pooledProjectile._projectileTimeLeft -= Time.deltaTime;
        if (pooledProjectile._projectileTimeLeft <= 0) 
        {
            pooledProjectile._visualImpactEffectTimeLeft -= Time.deltaTime;
            if (pooledProjectile._activeProjectile._hasImpactEffect ? (pooledProjectile._visualImpactEffectTimeLeft <= 0) : (true))
            {
                ResetProjectileCore(chunk, projectileSlot);
                return (false);
            }
        }
        return (true);
    }

    private void MovePooledProjectiles(int chunk, int projectileSlot)
    {
        ProjectileComponent projectileComponent = _pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot]._activeProjectile;
        if (projectileComponent._projectileType == ProjectileType._distanceProjectile)
        {
            projectileComponent.transform.position += projectileComponent.transform.forward * projectileComponent._distanceProjectileData._constantSpeed * Time.deltaTime;

            RaycastHit[] raycastHits;
            raycastHits = Physics.SphereCastAll(projectileComponent.transform.position, projectileComponent._distanceProjectileData._projectileCollisionAreaSize, Vector3.one, 0, _hittableLayers);
            if (raycastHits.Length > 0)
            {
                foreach (RaycastHit hit in raycastHits)
                {
                    ReceiveDamage receiveDamageComponent = hit.transform.GetComponent<ReceiveDamage>();
                    if (receiveDamageComponent)
                    {
                        receiveDamageComponent.TakeDamage(projectileComponent._projectileDamage, hit.point, projectileComponent._impactEffectDatas);
                    }
                }
                ResetProjectileCore(chunk, projectileSlot);
            }
        }
    }

    /*Called when "PhysicsProjectile" type collides with something.*/
    public void PhysicsProjectileCollided(int chunk, int projectileSlot, Collision collision)
    {
        PooledProjectile pooledProjectile = _pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot];
        ProjectileComponent projectileComponent = pooledProjectile._activeProjectile;
        pooledProjectile._totalBounces++;
        ReceiveDamage receiveDamage = collision.transform.GetComponent<ReceiveDamage>();
        
        if (receiveDamage && pooledProjectile._totalBounces < projectileComponent._physicsProjectileData._maxBounces)
            receiveDamage.TakeDamage(projectileComponent._projectileDamage, projectileComponent.transform.position, projectileComponent._impactEffectDatas);

        if (pooledProjectile._totalBounces >= projectileComponent._physicsProjectileData._maxBounces)
            ImpactEffectControll(receiveDamage, pooledProjectile, projectileComponent, chunk, projectileSlot);
        
    }

    private void ImpactEffectControll(ReceiveDamage receiveDamage, PooledProjectile pooledProjectile, ProjectileComponent projectileComponent, int chunk, int projectileSlot)
    {
        print(pooledProjectile._oneTimeImpactEffect);
        EffectComponent effectComponent = null;
        if (projectileComponent._impactEffectDatas != null && projectileComponent._impactEffectDatas.Length > 0)
            effectComponent = FindImpactEffectData(projectileComponent._impactEffectDatas[0]._effectName, pooledProjectile);
        if (effectComponent == null)
        {
            ResetProjectileCore(chunk, projectileSlot);
            return;
        }
        if (!pooledProjectile._oneTimeImpactEffect && pooledProjectile._activeProjectile._hasImpactEffect)
        {
            effectComponent.gameObject.SetActive(true);
            print("impact");
            int damage = 0;
            float visualImpactEffectTime = 0;
            string animationName = FindVisualImpactEffectAnimationName(projectileComponent._impactEffectDatas[0]._effectName, pooledProjectile._projectileCore);
            pooledProjectile._oneTimeImpactEffect = true;
            print(animationName);
            if (animationName != "")
            {
                print("play");
                effectComponent.transform.position = projectileComponent.transform.position;
                pooledProjectile._projectileCore._animator.Play(animationName);
            }
            
            ImpactEffectType impactEffectType = projectileComponent._impactEffectDatas[0]._impactEffect;
            visualImpactEffectTime = effectComponent._visualEffectDuartion;

            switch (impactEffectType)
            {
                case ImpactEffectType.none:
                    break;
                case ImpactEffectType.shatter:
                    DealDirectionalDamage();
                    break;
                case ImpactEffectType.explosion:
                    float areaSize = effectComponent._explosionEffect._explosionRadius;
                    DealAreaDamage(areaSize, damage, projectileComponent.transform.position, projectileComponent._impactEffectDatas);
                    break;
                case ImpactEffectType.deployWeapon:
                    DeployWeapon(projectileComponent.transform.position, projectileComponent._impactEffectDatas);
                    break;
                default: /*fire, ice, electricity and acid effects.*/
                    DoDamageOverTime(receiveDamage, damage, projectileComponent.transform.position, projectileComponent._impactEffectDatas);
                    break;
            }
            projectileComponent.gameObject.SetActive(false);
            pooledProjectile._visualImpactEffectTimeLeft = visualImpactEffectTime;
            pooledProjectile._projectileTimeLeft = 0;
        }
    }

    private string FindVisualImpactEffectAnimationName(string effectName, ProjectileCore projectileCore)
    {
        string animationName = "";
        foreach (EffectFolder folder in projectileCore._effectFolders)
        {
            if (folder._effectName == effectName)
            {
                animationName = folder._effectAnimationName;
                break;
            }
        }
        return (animationName);
    }

    private void DoDamageOverTime(ReceiveDamage receiveDamage, int damage, Vector3 hitPoint, ImpactEffectData[] impactEffectData)
    {

    }

    private void DealDirectionalDamage()
    {

    }

    private void DealAreaDamage(float size, int damage, Vector3 hitPoint, ImpactEffectData[] impactEffectData)
    {
        print("area");
        RaycastHit[] raycastHits = Physics.SphereCastAll(hitPoint, size, new Vector3(0.01f, 0.01f, 0.01f));
        foreach (RaycastHit hit in raycastHits)
        {
            ReceiveDamage receiveDamage = hit.transform.GetComponent<ReceiveDamage>();
            if (receiveDamage)
                receiveDamage.TakeDamage(damage, hitPoint, impactEffectData);
        }
    }

    private void DeployWeapon(Vector3 hitPoint, ImpactEffectData[] impactEffectData)
    {

    }

    private EffectComponent FindImpactEffectData(string effectName, PooledProjectile pooledProjectile)
    {
        ProjectileCore projectileCore = pooledProjectile._projectileCore;
        int i = 0;
        while (i < projectileCore._effectFolders.Length && projectileCore._effectFolders[i]._effectName != effectName)
        {
            i++;
        }
        if (i >= projectileCore._effectFolders.Length) 
        {
            Debug.LogError(effectName + "impact effect not found");
            return (null);
        }
        return (projectileCore._effectFolders[i]._effectComponent);
    }

    private void ResetProjectileCore(int chunk, int projectileSlot)
    {
        _pooledProjectileChunks[chunk]._projectilesInUse--;

        _pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot]._activeProjectile.gameObject.SetActive(false);
        _pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot]._activeProjectile = null;
        _pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot]._oneTimeImpactEffect = false;
        ProjectileCore projectileCore = _pooledProjectileChunks[chunk]._pooledProjectiles[projectileSlot]._projectileCore;
        foreach (EffectFolder folder in projectileCore._effectFolders)
        {
            folder._effectComponent.gameObject.SetActive(false);
        }
        projectileCore.gameObject.SetActive(false);
    }
}
