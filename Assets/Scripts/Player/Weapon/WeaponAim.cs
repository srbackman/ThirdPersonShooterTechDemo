using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum AimMode
{
    none,
    manual,
    auto
}

public class WeaponAim : MonoBehaviour
{
    private ClassLibrary lib;
    private WeaponsWheel weaponsWheel;
    private WeaponsCore weaponCore;
    public Camera _playerCamera;
    public Transform _playerTransform;
    [SerializeField] private RectTransform _manualAimSightRect;
    [HideInInspector] public Vector3 _manualAimHitPoint;
    [SerializeField] private Rigidbody _leftHandRigidBody;
    [SerializeField] private Rigidbody _rightHandRigidBody;
    [Space]
    [Header("Aim Fine Roation Controll")]
    [SerializeField] private Transform _fineAimRotationTransform;
    [SerializeField] private Vector3 _fineAimAutoAimDisplacementVector;
    [Range(0.1f, 3f)]
    [SerializeField] private float _fineAimTickRotation = 1f;
    [SerializeField] private float _fineAimTreshold = 1f;
    [Space]
    [SerializeField] private GameObject _autoAimParentObject;
    [SerializeField] private RectTransform[] _registeredAutoAimRects;
    [SerializeField] private RaycastHit[] _registeredAimHits;
    [Space]
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private LayerMask _raycastLayers;
    [Header("AutoAim")]
    [SerializeField] private float _maxAutoAimDistance = 10f;
    [SerializeField] private float _maxAutoAimRadius = 4f;
    [Header("PhysicsAiming")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _endHitDotTransform;

    private RaycastHit[] _validTargets;

    private void Awake()
    {
        lib = FindObjectOfType<ClassLibrary>();
        weaponsWheel = GetComponent<WeaponsWheel>();
        weaponCore = GetComponent<WeaponsCore>();
        _playerCamera = Camera.main;
    }

    public void AwakePooling()
    {
        if (!weaponsWheel) weaponsWheel = GetComponent<WeaponsWheel>();
        _registeredAutoAimRects = new RectTransform[weaponsWheel._currentScriptableObjectWeapon._weaponLevels[weaponsWheel._currentWeaponLevel]._autoAimMaxTargets + 1]; /* +1 slot for sorting.*/
        _registeredAimHits = new RaycastHit[weaponsWheel._currentScriptableObjectWeapon._weaponLevels[weaponsWheel._currentWeaponLevel]._autoAimMaxTargets + 1]; /* +1 slot for sorting.*/
    }

    public RaycastHit[] AimCoreControll(AimMode aimMode)
    {
        if (aimMode == AimMode.none)
        {
            if (_registeredAimHits[0].transform)
            {
                ClearValidTargetsArray();
            }
            AutoAimCheckTargets();
            WeaponFineRotation(aimMode);
        }
        if (aimMode == AimMode.manual)
        {
            if (_registeredAimHits[0].transform)
            {
                ClearValidTargetsArray();
            }
            AutoAimCheckTargets();
            ManualAimSystem();
            WeaponFineRotation(aimMode);
            return (_registeredAimHits);
        }
        if (_manualAimSightRect.gameObject.activeSelf)
        {
            _manualAimSightRect.gameObject.SetActive(false);
        }
        if(aimMode == AimMode.auto && weaponsWheel._currentScriptableObjectWeapon._hasAutoAim)
        {
            AutoAimSystem();
            WeaponFineRotation(aimMode);
            return (_registeredAimHits);
        }
        WeaponFineRotation(aimMode);
        return (null);
    }

    private void WeaponFineRotation(AimMode aimMode)
    {
        Vector3 direction = Vector3.one;
        if (aimMode == AimMode.none)
        {
            direction = _playerTransform.forward;
        }
        else if (aimMode == AimMode.manual)
        {
            RaycastHit hit;
            /*Raycast straight forward from camera into the world.*/
            Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out hit, 1000);
            if (!hit.transform)
                direction = ((_playerCamera.transform.forward * 1000) - _playerTransform.position).normalized;
            else
                direction = (hit.point - weaponCore._barrelTransform.position).normalized;
            Debug.DrawLine(weaponCore._barrelTransform.position, weaponCore.transform.position + direction * 40, Color.white);
        }
        else if (aimMode == AimMode.auto)
        {
            int i = 0;
            Vector3 sum = Vector3.zero;
            while (i < _registeredAimHits.Length && _registeredAimHits[i].transform != null)
            {
                sum += _registeredAimHits[i].transform.position;
                i++;
            }
            if (_registeredAimHits[i].transform != null)
                direction = ((sum / i) - (weaponCore._barrelTransform.position + _fineAimAutoAimDisplacementVector)).normalized;
            else
                direction = _playerTransform.forward;
        }
        if (direction == Vector3.zero) direction = weaponCore._barrelTransform.forward;
        Vector3 rotationSteps = Vector3.RotateTowards(_fineAimRotationTransform.forward, direction, _fineAimTickRotation * Time.deltaTime, 1f);
        _fineAimRotationTransform.rotation = Quaternion.LookRotation(rotationSteps);
        if (_fineAimTreshold > Quaternion.Angle(_fineAimRotationTransform.rotation, Quaternion.LookRotation(direction)))
            _fineAimRotationTransform.rotation = Quaternion.LookRotation(direction);
    }

    private void AimPhysicsTrajectory()
    {

    }

    private void ClearValidTargetsArray()
    {
        for (int i = 0; i < _validTargets.Length; i++)
        {
            _validTargets[i] = new RaycastHit();
        }
    }

    private void AutoAimSystem()
    {
        ScriptableObjectWeaponType scriptableWeapon = weaponsWheel._currentScriptableObjectWeapon;
        /*Setup. Get objects in front of the player. Changing in future for something better.*/
        if ((_validTargets == null) || (_validTargets.Length != scriptableWeapon._weaponLevels[0]._autoAimMaxTargets + 1))
        {
            _validTargets = new RaycastHit[scriptableWeapon._weaponLevels[0]._autoAimMaxTargets + 1];
        }
        Vector3 pos = _playerTransform.position + (_playerTransform.forward * _maxAutoAimDistance);

        RaycastHit[] currentRaycastHits;
        currentRaycastHits = Physics.CapsuleCastAll(_playerTransform.position + (_playerTransform.forward * (_maxAutoAimRadius + 1)),
            pos, _maxAutoAimRadius, _playerTransform.forward, 0f, _targetLayers); /*collision scan everything in area*/

        /*Filtering. Save only those that are visible *
         * and the closest ones get to stay in the array.*/
        AutoAimGetValidTargets(currentRaycastHits);

        /*Add or Remove auto aim targets.*/
        bool check = AutoAimCheckTargets();

        /*Keep the sights on targets.*/
        if (check) MoveAutoAimSights();
        ClearValidTargetsArray();
    }

    private void ManualAimSystem() //!!add physics aim!!
    {
        if (!_manualAimSightRect.gameObject.activeSelf)
        {
            _manualAimSightRect.gameObject.SetActive(true);
        }
        if (Physics.Raycast(_playerCamera.ScreenToWorldPoint(_manualAimSightRect.position), _playerCamera.transform.forward, out RaycastHit hit, 1000, _raycastLayers))
        {
            _registeredAimHits[0] = hit;
            print("found");
        }
        else
            _registeredAimHits[0] = new RaycastHit();
    }

    private void AutoAimGetValidTargets(RaycastHit[] currentRaycastHits)
    {
        float[] targetDistances = CalculateDistanceAll(currentRaycastHits);
        ClosestQuickSort(false, currentRaycastHits, 0, currentRaycastHits.Length - 1, targetDistances);
        int validsFound = 0;
        foreach (RaycastHit ray in currentRaycastHits)
        {
            Vector3 rayDirectionForCamera = (ray.transform.position - _playerCamera.transform.position).normalized;
            Vector3 rayDirectionForPlayer = (ray.transform.position - _playerTransform.position).normalized;
            Physics.Raycast(_playerCamera.transform.position, rayDirectionForCamera, out RaycastHit cameraHit, 1000, _raycastLayers);
            Physics.Raycast(_playerTransform.position, rayDirectionForPlayer, out RaycastHit playerHit, 1000, _raycastLayers);
            if (playerHit.transform != ray.transform || cameraHit.transform != ray.transform) continue;

            _validTargets[validsFound] = playerHit;
            validsFound++;
            if (validsFound > _validTargets.Length - 1) break;
        }
        _validTargets[_validTargets.Length - 1] = new RaycastHit();
    }

    private float[] CalculateDistanceAll(RaycastHit[] rayHits)
    {
        float[] distances = new float[rayHits.Length];
        for (int i = 0; i < rayHits.Length; i++)
        {
            if (!rayHits[i].transform) { distances[i] = Mathf.Infinity; continue; }
            distances[i] = Vector3.Distance(_playerCamera.transform.position, rayHits[i].transform.position);
        }
        return (distances);
    }

    private void ClosestQuickSort(bool withRects, RaycastHit[] rayHits, int low, int high, float[] targetDistances)
    {
        if (low < high)
        {
            int m = Partition(withRects, rayHits, low, high, targetDistances);
            ClosestQuickSort(withRects, rayHits, low, m - 1, targetDistances);
            ClosestQuickSort(withRects, rayHits, m + 1, high, targetDistances);
        }
    }

    private int Partition(bool withRects, RaycastHit[] rayHits, int min, int max, float[] targetDistances)
    {
        RaycastHit pivot = rayHits[min];
        int m = min;
        for(int k = min + 1; k <= max; k++)
        {
            if (pivot.transform == null || rayHits[k].transform != null && targetDistances[k] < targetDistances[min])
            {
                m++;
                SwapRaycastHits(withRects, rayHits, k, m);
            }
        }
        SwapRaycastHits(withRects, rayHits, min, m);
        return (m);
    }

    private void SwapRaycastHits(bool withRects, RaycastHit[] raycastHits, int t1, int t2)
    {
        RaycastHit tempRay = raycastHits[t1];
        raycastHits[t1] = raycastHits[t2];
        raycastHits[t2] = tempRay;
        if(withRects)
        {
            RectTransform tempRect = _registeredAutoAimRects[t1];
            _registeredAutoAimRects[t1] = _registeredAutoAimRects[t2];
            _registeredAutoAimRects[t2] = tempRect;
        }
    }

    private bool AutoAimCheckTargets()
    {
        for (int i = 0; i < _registeredAimHits.Length - 1;)
        {
            if (_registeredAutoAimRects[i] && (_registeredAimHits[i].transform == null || !CheckTransformFromArray(_validTargets, _registeredAimHits[i], 0, _validTargets.Length - 1)))
            {
                //print("Set off.");
                _registeredAutoAimRects[i].gameObject.SetActive(false);
                _registeredAutoAimRects[i] = null;
                _registeredAimHits[i] = new RaycastHit();
                float[] targetDistances = CalculateDistanceAll(_registeredAimHits);
                ClosestQuickSort(true, _registeredAimHits, 0, _registeredAimHits.Length - 1, targetDistances);
                i++;
                continue;
            }
            if (_validTargets.Length > i && _validTargets[i].transform && !CheckTransformFromArray(_registeredAimHits, _validTargets[i], 0, _registeredAimHits.Length - 1))
            {
                //print("Set on.");
                RectTransform sight = lib.poolingManager.SpawnAutoAimTarget();
                _registeredAutoAimRects[_registeredAutoAimRects.Length - 1] = sight;
                _registeredAimHits[_registeredAimHits.Length - 1] = _validTargets[i];
                float[] targetDistances = CalculateDistanceAll(_registeredAimHits);
                ClosestQuickSort(true, _registeredAimHits, 0, _registeredAimHits.Length - 1, targetDistances);

                if (_registeredAutoAimRects[_registeredAutoAimRects.Length - 1])
                {
                    //print("Set off during 'Set on'.");
                    _registeredAutoAimRects[_registeredAutoAimRects.Length - 1].gameObject.SetActive(false);
                }
                _registeredAutoAimRects[_registeredAutoAimRects.Length - 1] = null;
                _registeredAimHits[_registeredAimHits.Length - 1] = new RaycastHit();
            }
            i++;
        }
        return (_registeredAimHits.Length != 0);
    }

    /**/
    private bool CheckTransformFromArray(RaycastHit[] rayHits, RaycastHit hit, int low, int high)
    {
        int mostMiddle = (low + (int)((high - low) / 2));
        if ((high - low) > -1 && rayHits.Length > mostMiddle && hit.transform != null && rayHits[mostMiddle].transform == hit.transform) return (true);
        
        if ((high - low) > -1)
        {
            bool check;
            check = CheckTransformFromArray(rayHits, hit, low, mostMiddle - 1);//Check left.
            if (check) return (true);
            check = CheckTransformFromArray(rayHits, hit, mostMiddle + 1, high);//Check right.
            return (check);
        }
        return (false);
    }

    /*Move AutoAim sights.*/
    private void MoveAutoAimSights()
    {
        for(int i = 0; i < _registeredAimHits.Length; i++)
        {
            if (_registeredAimHits[i].transform == null || _registeredAutoAimRects[i] == null || !_registeredAutoAimRects[i].gameObject.activeSelf)
            {
                continue;
            }
            _registeredAutoAimRects[i].position = _playerCamera.WorldToScreenPoint(_registeredAimHits[i].transform.position);
        }
    }
}
