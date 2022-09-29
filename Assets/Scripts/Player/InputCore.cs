using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCore : MonoBehaviour
{
    private InputActions inputActions;
    [SerializeField] private WeaponsCore weaponsCore;
    [SerializeField] private WeaponsWheel weaponsWheel;
    private PlayerMovement playerMovement;
    private bool _oneShotLock = false;
    private float _firerateTimer = 0;

    private void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
    }

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (!weaponsCore || !weaponsWheel)
            Debug.LogError("WeaponsCore or WeaponsWheel are not set.");
    }

    private void Update()
    {
        /*Weapons Wheel input*/
        bool weaponsWheelPressed = inputActions.WeaponSelection.WeaponsWheel.IsPressed();
        int scrollInput = (int)inputActions.WeaponSelection.ScrollPagesWeapons.ReadValue<Vector2>().y;
        int selector = scrollInput == 0 ? 0 : scrollInput < 0 ? -1 : 1;
        weaponsWheel.WeaponsWheelCore(weaponsWheelPressed, selector);
        /*Aim input*/
        if (inputActions.Player.Aim.IsPressed()) weaponsCore.GetAimData(AimMode.manual);
        else if (weaponsWheel._currentScriptableObjectWeapon._hasAutoAim) weaponsCore.GetAimData(AimMode.auto);
        else weaponsCore.GetAimData(AimMode.none);
        /*Firing input*/
        FiringInputControll(weaponsWheelPressed);
    }

    private void FixedUpdate()
    {
        playerMovement.MovementCore(inputActions.Player.Move.ReadValue<Vector2>(), inputActions.Player.Jump.WasPressedThisFrame());
    }

    private void FiringInputControll(bool weaponsWheelPressed)
    {
        _firerateTimer -= Time.deltaTime;
        if (0 < _firerateTimer) return;

        if (!weaponsWheelPressed && !_oneShotLock && inputActions.Player.Fire.IsPressed())
        {
            _firerateTimer = weaponsWheel._currentScriptableObjectWeapon._weaponLevels[weaponsWheel._currentWeaponLevel]._fireRate;
            if (weaponsWheel._currentScriptableObjectWeapon._firingMode == FiringMode.oneShot) _oneShotLock = true;
            weaponsCore.FireWeapon(false);
        }
        if (!inputActions.Player.Fire.IsPressed())
        {
            _oneShotLock = false;
            if (weaponsWheel._currentScriptableObjectWeapon._firingMode == FiringMode.holdProjectile)
                weaponsCore.FireWeapon(true);
        }
    }
}
