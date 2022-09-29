using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FiringMode
{
    oneShot, /*On fire button down, shoot once.*/
    automatic, /*On fire button held, shoot constantly new shots.*/
    holdProjectile /*On fire button held, shoot and hold one projectile.*/
}

[System.Serializable]
public class WeaponLevelData
{
    public string _weaponModelName;
    [Header("Common Settings")]
    public int _totalRequierdXpForThisLevel = 100; /*Use this to determine witch projectile to use.*/
    public int _requierdGameProgressionForUpgrade = 0;
    [Space]
    public int _maxAmmo = 500;
    public float _fireRate = 0.1f;
    public int _autoAimMaxTargets = 1;
    public int _projectilesShotPerTrigger = 5;
    [Range(1.1f, 50f)]
    public float _weaponShotAccuracy = 1f;
    [Range(0.01f, 1f)]
    public float _weaponAimAssistStrength = 1f;
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData")]
public class ScriptableObjectWeaponType : ScriptableObject
{
    public string _weaponName;
    public Sprite _weaponSlotSprite;
    [Space]
    public bool _hasAutoAim = true;
    public FiringMode _firingMode = FiringMode.oneShot;
    [Space]
    public List<WeaponLevelData> _weaponLevels = new List<WeaponLevelData>();
}
