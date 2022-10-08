using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class WeaponsWheel : MonoBehaviour
{
    private WeaponAim weaponAim;
    private WeaponsCore weaponsCore;
    private WeaponShooting weaponShooting;
    private WeaponModelCollection weaponModelCollection;
    [SerializeField] private GameObject _weaponWheelSelectionObject;
    [SerializeField] private Image[] _weaponWheelSlots = new Image[8];
    private int _weaponWheelCurrentPage = 0;
    [Range(1,10)]
    [SerializeField] private int _weaponWheelMaxPages = 3;
    [HideInInspector] public int _currentWeaponSlot = 0;
    [HideInInspector] public int _currentWeaponLevel = 0;
    [SerializeField] private TMP_Text _selectedWeaponWheelWeaponNameText;
    [Space]
    [SerializeField] private Image _currentWeaponImageObject;
    [SerializeField] private Image _previousWeaponImageObject;
    [SerializeField] private Image _nextWeaponImageObject;
    [Space]
    public List<ScriptableObjectWeaponType> _tempWeaponsList;
    public ScriptableObjectWeaponType _currentScriptableObjectWeapon;
    private void Awake()
    {
        weaponAim = GetComponent<WeaponAim>();
        weaponsCore = GetComponent<WeaponsCore>();
        weaponShooting = GetComponent<WeaponShooting>();
        weaponModelCollection = GetComponent<WeaponModelCollection>();
        SwitchWeapon(_tempWeaponsList[_currentWeaponSlot]);
        weaponAim.AwakePooling();
    }

    public void WeaponsWheelCore(bool onOff, int leftRight)
    {
        if (_weaponWheelSelectionObject.activeSelf != onOff) PlayerWeaponsWheelVisualOnOff(onOff);
        if (leftRight != 0)
        {
            if (onOff) /*Go to left or right on weaponsWheel pages*/
            {
                _weaponWheelCurrentPage += leftRight;
                if (_weaponWheelCurrentPage < 0) _weaponWheelCurrentPage = _weaponWheelMaxPages - 1;
                if (_weaponWheelCurrentPage > _weaponWheelMaxPages - 1) _weaponWheelCurrentPage = 0;
                UpdateWeaponWheelSelection();
                return;
            }
            /*Go to next or previous weapon on weaponsWheel*/
            int startPoint = _currentWeaponSlot;
            _currentWeaponSlot += leftRight;
            if (_currentWeaponSlot < 0) _currentWeaponSlot = _tempWeaponsList.Count - 1;
            if (_currentWeaponSlot > _tempWeaponsList.Count - 1) _currentWeaponSlot = 0;
            
            while (_currentWeaponSlot != startPoint && _tempWeaponsList[_currentWeaponSlot] == null)
            {
                _currentWeaponSlot += leftRight;
                if (_currentWeaponSlot < 0) _currentWeaponSlot = _tempWeaponsList.Count - 1;
                if (_currentWeaponSlot > _tempWeaponsList.Count - 1) _currentWeaponSlot = 0;
            }
            SwitchWeapon(_tempWeaponsList[_currentWeaponSlot]);
        }
    }

    public void PlayerWeaponsWheelVisualOnOff(bool onOff)
    {
        _weaponWheelSelectionObject.SetActive(onOff);
        if (onOff)
        {
            UpdateWeaponWheelSelection();
            return;
        }
    }

    private void UpdateSelectedWeaponWheelWeapon()
    {

    }

    private void UpdateWeaponWheelSelection()
    {
        int i = 0;
        while(i < _weaponWheelSlots.Length)
        {
            int a = i + (_weaponWheelCurrentPage * 8);
            if ((a < _tempWeaponsList.Count) && _tempWeaponsList[a])
            {
                _weaponWheelSlots[i].sprite = _tempWeaponsList[i + (_weaponWheelCurrentPage * 8)]._weaponSlotSprite;
                i++;
                continue;
            }
            _weaponWheelSlots[i].sprite = null;
            i++;
        }
    }

    private void UpdateWeaponQuickSelection()
    {
        int nextImageSlot = _currentWeaponSlot + 1;
        int previousImageSlot = _currentWeaponSlot - 1;

        if (nextImageSlot > _tempWeaponsList.Count - 1) nextImageSlot = 0;
        while (_tempWeaponsList[nextImageSlot] == null)
        {
            nextImageSlot++;
            if (nextImageSlot > _tempWeaponsList.Count - 1) nextImageSlot = 0;
        }
        if (previousImageSlot < 0) previousImageSlot = _tempWeaponsList.Count - 1;
        while (_tempWeaponsList[previousImageSlot] == null)
        {
            previousImageSlot--;
            if (previousImageSlot < 0) previousImageSlot = _tempWeaponsList.Count - 1;
        }
        _nextWeaponImageObject.sprite = _tempWeaponsList[nextImageSlot]._weaponSlotSprite;
        _currentWeaponImageObject.sprite = _currentScriptableObjectWeapon._weaponSlotSprite;
        _previousWeaponImageObject.sprite = _tempWeaponsList[previousImageSlot]._weaponSlotSprite;
    }

    public void SwitchWeapon(ScriptableObjectWeaponType weaponTypeData)
    {
        weaponShooting.OneExistingProjectileDeactivateTrigger();
        _currentScriptableObjectWeapon = _tempWeaponsList[_currentWeaponSlot];
        UpdateWeaponQuickSelection();
        Transform weaponModel = GetSOWeaponLevelModel(weaponTypeData);
        weaponsCore.SetBarrelTransform(weaponModel);
        //if (weaponTypeData && _currentScriptableObjectWeapon == weaponTypeData) return;
    }
    
    /*Find the weapon level model from the "_weaponModelCollection".*/
    private Transform GetSOWeaponLevelModel(ScriptableObjectWeaponType weaponTypeData)
    {
        int i = 0;
        while (i < weaponModelCollection._weaponFolders.Length
            && weaponTypeData._weaponName != weaponModelCollection._weaponFolders[i]._weaponName)
        {
            i++;
        }
        if (i == weaponModelCollection._weaponFolders.Length) return (null);
        _currentWeaponLevel = CheckWeaponLevel(weaponTypeData.name, weaponTypeData);
        Transform transform = weaponModelCollection._weaponFolders[i]._weaponLevelModels[_currentWeaponLevel];
        return (transform);
    }

    /*Check the weapons current level using XP value from DataManagment.*/
    private int CheckWeaponLevel(string weaponName, ScriptableObjectWeaponType weaponTypeData)
    {
        int level = 0;
        //get weapons stored xp
        int weaponXp = 0;  //here
        //-----------------------
        while (level < weaponTypeData._weaponLevels.Count)
        {
            if (weaponXp >= weaponTypeData._weaponLevels[level]._totalRequierdXpForThisLevel)
                level++;
            else
                break;
        }
        return (level);
    }
}
