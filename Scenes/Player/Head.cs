using EIODE.Components;
using EIODE.Resources;
using EIODE.Core.Console;
using EIODE.Utils;
using EIODE.Core;
using System.Collections.Generic;
using System;
using Godot;
using System.Linq;

namespace EIODE.Scenes;
public partial class Head : Node3D
{
    [Export] public float ShootingRayLength { get; set; } = -1000;

    [Export] public float CameraTiltSpeed { get; set; } = 10f;

    [Export] public float MaxCameraTiltRadian { get; set; } = 2f;
    public WeaponConfig CurrentWeapon { get; private set; } = null;

    public List<WeaponConfig> WeaponsInventory { get; private set; } = [];
    private int _currentWeaponIndex = 0;
    public bool _shooting = false;
    public bool _reloading = false;
    // used in special cases
    private bool _magazineFull = false;
    public bool _magazineEmpty = false;
    public float _shootingTime = 0.0f;
    public float _reloadingTimer = 0f;
    public bool _hitboxEnabled = false;
    private bool _hitboxTimerEnded = false;
    private HitboxComponent _hitbox = null;
    private Node3D _parent = null;
    private Timer _hitboxTimer = null;
    private Game _game = null;
    private DevConsole _console = null;
    private Player _player = null;
    public Camera3D Camera { get; private set; } = null;

    private const float MIN_PITCH = -90f;
    private const float MAX_PITCH = 90f;

    [Signal] public delegate void AmmoChangedEventHandler(WeaponConfig weapon);
    [Signal] public delegate void WeaponChangedEventHandler(WeaponConfig current);
    [Signal] public delegate void StartedReloadingEventHandler();
    [Signal] public delegate void EndedReloadingEventHandler();

    public override void _Ready()
    {
        _hitbox = ComponentsUtils.GetChildWithComponent<HitboxComponent>(this);

        // There should be only one timer as a child for the "Head" node
        _hitboxTimer = NodeUtils.GetChildWithNodeType<Timer>(this);

        _hitbox.Disable();

        _parent = GetParent<Node3D>();

        EmitSignalAmmoChanged(CurrentWeapon);

        Camera = NodeUtils.GetChildWithNodeType<Camera3D>(this);

        ConsoleCommandSystem.RegisterInstance(this);

        _game = Game.GetGame(this);

        _player = _game.Player;
        _console = _game.Console;

        // call me lazy but this works
        Cc_TankUp(0);
        ChangeCurrentWeapon(0, true);
        _hitbox.Damage = CurrentWeapon.DamagePerBullet;

    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
            CameraRotation(motion);
        if (@event is InputEventJoypadMotion joypadMotion)
            JoyPadCameraRotation(joypadMotion);
    }
    private void CameraRotation(InputEventMouseMotion e)
    {
        // horizontal
        _player.RotateY(Mathf.DegToRad(-e.Relative.X * _player.Conf.Sensitivity));

        // vertical
        float newPitch = Rotation.X + Mathf.DegToRad(-e.Relative.Y * _player.Conf.Sensitivity);
        newPitch = Mathf.Clamp(newPitch, Mathf.DegToRad(MIN_PITCH), Mathf.DegToRad(MAX_PITCH));
        Rotation = new Vector3(newPitch, Rotation.Y, Rotation.Z);
    }

    private void JoyPadCameraRotation(InputEventJoypadMotion joypadMotion)
    {
        throw new NotImplementedException();
    }

    public override void _Process(double delta)
    {
        HandleShooting(delta);
        CameraTilting(delta, _player.InputDirection);

        if (CurrentWeapon != WeaponsInventory[_currentWeaponIndex])
        {
            CurrentWeapon = WeaponsInventory[_currentWeaponIndex];
            EmitSignalWeaponChanged(CurrentWeapon);
        }

        if (Input.IsActionJustPressed(InputHash.K_E))
        {
            ChangeCurrentWeapon(_currentWeaponIndex + 1);
        }
        if (Input.IsActionJustPressed(InputHash.K_Q))
        {
            ChangeCurrentWeapon(_currentWeaponIndex - 1);
        }
    }


    private void HandleShooting(double delta)
    {
        if (!_shooting && _shootingTime <= CurrentWeapon.FireRate)
        {
            _shootingTime += (float)delta;
        }

        if (_hitboxEnabled && _shootingTime >= CurrentWeapon.HitboxDuration)
        {
            _hitbox.Disable();
            _hitboxEnabled = false;
        }

        _magazineEmpty = CurrentWeapon.CurrentAmmo <= 0;
        _magazineFull = CurrentWeapon.CurrentAmmo == CurrentWeapon.MagazineSize;

        if (Input.IsActionJustPressed(InputHash.RELOAD) && CanReload())
        {
            _reloading = true;
            EmitSignalStartedReloading();
        }

        if (_reloading) Reload(delta);

        if (GetShootingPressed()) Shoot();

        if (_hitboxEnabled && _hitboxTimer.TimeLeft <= 0) _hitbox.Disable();
    }
    private void Shoot()
    {
        _shooting = true;
        _hitbox.Enable();
        _hitboxEnabled = true;
        _shootingTime = 0;
        CurrentWeapon.CurrentAmmo--;
        EmitSignalAmmoChanged(CurrentWeapon);
        if (_hitboxTimer.IsStopped()) _hitboxTimer.Start();
        _shooting = false;
    }

    private void Reload(double delta)
    {
        _reloadingTimer += (float)delta;
        if (_reloadingTimer >= CurrentWeapon.ReloadTime)
        {
            EmitSignalEndedReloading();
            _reloading = false;
            int ammoNeeded = CurrentWeapon.MagazineSize - CurrentWeapon.CurrentAmmo;
            int ammoToTake = Mathf.Min(ammoNeeded, CurrentWeapon.CurrentMaxAmmo);
            CurrentWeapon.CurrentAmmo += ammoToTake;
            CurrentWeapon.CurrentMaxAmmo -= ammoToTake;
            _reloadingTimer = 0f;
            EmitSignalAmmoChanged(CurrentWeapon);
        }
    }

    private bool CanShoot()
    {
        return !_reloading && _shootingTime >= CurrentWeapon.FireRate && !_magazineEmpty;
    }

    private bool CanReload()
    {
        return !_reloading && !_magazineFull && CurrentWeapon.CurrentMaxAmmo > 0;
    }

    private bool GetShootingPressed()
    {
        return (CurrentWeapon.Auto ? Input.IsActionPressed(InputHash.SHOOT) : Input.IsActionJustPressed(InputHash.SHOOT)) && CanShoot();
    }
    private void CameraTilting(double delta, Vector2 _inputDirection)
    {
        float desiredZRotation;

        desiredZRotation = -_inputDirection.X * MaxCameraTiltRadian;

        desiredZRotation = Mathf.DegToRad(desiredZRotation);

        Vector3 rot = Rotation;

        // Responsiveness, idk why it looks like this and it makes the Z tilting look cool instead of using CameraTiltSpeed directly
        float t = 1f - Mathf.Exp(-CameraTiltSpeed * (float)delta);
        rot.Z = Mathf.LerpAngle(rot.Z, desiredZRotation, t);

        Rotation = rot;
    }

    private void AddWeaponToInventory(WeaponConfig weapon, bool replaceAll = false)
    {
        bool weaponExistsInInventory = WeaponsInventory.Contains(weapon);

        if (!weaponExistsInInventory || replaceAll)
        {
            weapon.CurrentAmmo = weapon.MagazineSize;
            weapon.CurrentMaxAmmo = weapon.MaxAmmo;

            if (replaceAll && weaponExistsInInventory)
            {
                WeaponsInventory.RemoveAll(x => x == weapon);
            }

            if (!weaponExistsInInventory)
            {
                WeaponsInventory.Add(weapon);
            }
        }

        _console?.Log($"Added {weapon.Name}, Count : {WeaponsInventory.Count}");
    }

    private void AddWeaponToInventory(ReadOnlySpan<string> weapons)
    {
        foreach (var weapon in weapons)
        {
            AddWeaponToInventory(_game.FindWeapon(weapon));
        }
    }

    private void ReplaceInventory(WeaponConfig[] newInventory)
    {
        WeaponsInventory = [.. newInventory];
    }

    private void ChangeCurrentWeapon(int index, bool forceSet = false)
    {
        if (forceSet)
        {
            CurrentWeapon = WeaponsInventory[index];
        }
        else
        {
            if (_currentWeaponIndex == index) return;

            if (index > WeaponsInventory.Count - 1) index = 0;

            if (index < 0) index = WeaponsInventory.Count - 1;

            _currentWeaponIndex = index;
            CurrentWeapon = WeaponsInventory[_currentWeaponIndex];
        }

        EmitSignalAmmoChanged(CurrentWeapon);
        EmitSignalWeaponChanged(CurrentWeapon);
    }

    #region CC
    [ConsoleCommand("tank_up", "Gives all weapons of given set (0 | 1 | 2 | 3)", true)]
    public void Cc_TankUp(int set)
    {
        switch (set)
        {
            case 0:
                AddWeaponToInventory(WeaponsSets.SET_0);
                break;
            case 1:
                AddWeaponToInventory(WeaponsSets.SET_1);
                break;
            default:
                break;
        }
    }

    [ConsoleCommand("current_weapon_set", "Change a setting of the current gun settings (cammo int, mammo int, damage int)")]
    public void Cc_CurrentWeaponSet(string type, int amount)
    {
        switch (type)
        {
            case "cammo":
                CurrentWeapon.CurrentAmmo = amount;
                EmitSignalAmmoChanged(CurrentWeapon);
                _console?.Log($"Changed current ammo to be {amount}");
                break;
            case "mammo":
                CurrentWeapon.CurrentMaxAmmo = amount;
                EmitSignalAmmoChanged(CurrentWeapon);
                _console?.Log($"Changed max ammo to be {amount}");
                break;
            case "damage":
                CurrentWeapon.DamagePerBullet = amount;
                _console?.Log($"Changed current damage to be {amount}");
                break;
            default:
                break;
        }
    }

    [ConsoleCommand("fast_reload", "Reloads Current Gun")]
    public void Cc_FastReload()
    {
        // passes big number to delta so it reloads immediately 
        Reload(99999);
    }

    [ConsoleCommand("desired_fov", "Sets default FOV (float)")]
    public void Cc_SetFov(float v)
    {
        Camera.Fov = v;
    }
    #endregion
}
