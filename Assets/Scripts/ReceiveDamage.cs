using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReceiveDamage : MonoBehaviour
{
    [SerializeField] private EnemyCore _enemyCore;

    [SerializeField] private Color _hitColor = Color.red;
    [SerializeField] private Color _normalColor = Color.gray;
    [SerializeField] private float _colorTime = 1;
    private MeshRenderer _meshRenderer;
    private bool _takingDamage = false;
    private float _timer = 0;


    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (_takingDamage) _timer -= Time.deltaTime;
        if (_timer < 0) _takingDamage = false;

        Material material;
        material = _meshRenderer.material;
        if (_takingDamage)
            material.color = _hitColor;
        else
            material.color = _normalColor;
        _meshRenderer.material = material;
    }

    public void TakeDamage(int damage, Vector3 hitPoint, ImpactEffectData[] impactEffectDatas)
    {
        _takingDamage = true;
        _timer = _colorTime;
        foreach (ImpactEffectData impactData in impactEffectDatas)
        {
            switch (impactData._impactEffect)
            {
                case ImpactEffectType.shatter: break;
                case ImpactEffectType.fire: break;
                case ImpactEffectType.ice: break;
                case ImpactEffectType.acid: break;
                case ImpactEffectType.electricity: break;
                case ImpactEffectType.explosion: break;
                case ImpactEffectType.deployWeapon: break;
                default: break;
            }
        }
    }

}
