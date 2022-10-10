using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCore : MonoBehaviour
{
    public WeaponsProjectileFolder[] _weaponsProjectileFolders;
    public EffectFolder[] _effectFolders;
    public Animator _animator;
}

[System.Serializable]
public class WeaponsProjectileFolder
{
    public string _weaponName;
    public ProjectileComponent[] _projectileLevelComponents;
}

[System.Serializable]
public class EffectFolder
{
    public string _effectName;
    public EffectComponent _effectComponent;
    public string _effectAnimationName;
}
