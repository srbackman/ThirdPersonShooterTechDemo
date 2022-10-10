using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectComponent : MonoBehaviour
{
    public float _visualEffectDuartion = 0.2f;

    public KnockbackEffectData _knockbackEffect;
    public ShatterEffectData _shatterEffect;
    public FireElectricityAcidEffectsData _fireElectricityAcidEffects;
    public IceEffectData _iceEffect;
    public ExplosionEffectData _explosionEffect;
    public DeployEffectData _deployEffect;

    private void OnDrawGizmos()
    {
        if (_explosionEffect._explosionRadius <= 0)
            return;

        Gizmos.DrawWireSphere(transform.position, _explosionEffect._explosionRadius);
    }
}

public class KnockbackEffectData
{
    public float _knockbackForce = 5f;
}

[System.Serializable]
public class ShatterEffectData
{
    public int _splinterAmount = 8;
    public int _maxTargetsPierced = 1;
    public int _shatterRadius = 45;
}

[System.Serializable]
public class FireElectricityAcidEffectsData
{
    public int _effectDamagePerTick = 2;
    public float _effectDuration = 5f;
}

[System.Serializable]
public class IceEffectData
{
    public float _slowdownStrenght = 2f;
    public float _effectDuration = 5f;
}

[System.Serializable]
public class ExplosionEffectData
{
    public float _explosionRadius;
    [Range(0.1f, 1)]
    public float _spreadChance = 1;
}

[System.Serializable]
public class DeployEffectData
{
    public string _deployableName;
    public float _lifeTime = 12f;
}

