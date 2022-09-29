using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponFolder
{
    public string _weaponName;
    public Transform[] _weaponLevelModels;
}

public class WeaponModelCollection : MonoBehaviour
{
    public WeaponFolder[] _weaponFolders;
}
