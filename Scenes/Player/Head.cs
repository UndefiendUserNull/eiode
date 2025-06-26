using EIODE.Components;
using EIODE.Resources;
using EIODE.Core.Console;
using EIODE.Utils;
using EIODE.Core;
using Godot;
using System.IO;
using System;

namespace EIODE.Scenes;
public partial class Head : Node3D
{
    [Export] public float ShootingRayLength { get; set; } = -1000;
    [Export] private WeaponConfig WeaponResource { get; set; } = null;

    [Export] public float CameraTiltSpeed { get; set; } = 10f;

    [Export] public float MaxCameraTiltRadian { get; set; } = 2f;
    public WeaponConfig W { get; private set; } = null;

    public bool _shooting = false;
    public bool _reloading = false;
    // used in special cases
    private bool _magazineFull = false;
    public bool _magazineEmpty = false;
    public float _shootingTime = 0.0f;
    public int CurrentAmmo { get; private set; } = 0;
    public int CurrentMaxAmmo { get; private set; } = 0;
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

    private const string WEAPONS_PATH = "res://Resources/Gun Types/";
    private const float MIN_PITCH = -90f;
    private const float MAX_PITCH = 90f;

    [Signal] public delegate void AmmoChangedEventHandler(WeaponConfig weapon);
    [Signal] public delegate void GunSettingsChangedEventHandler(WeaponConfig previous, WeaponConfig current);
    [Signal] public delegate void StartedReloadingEventHandler();
    [Signal] public delegate void EndedReloadingEventHandler();

    public override void _Ready()
    {
        if (WeaponResource == null) GD.PushError("No weapon was given to player");
        else W = WeaponResource;

        _hitbox = ComponentsUtils.GetChildWithComponent<HitboxComponent>(this);

        // There should be only one timer as a child for the "Head" node
        _hitboxTimer = NodeUtils.GetChildWithNodeType<Timer>(this);

        _hitbox.Damage = W.damagePerBullet;
        _hitbox.Disable();

        CurrentAmmo = W.magazineSize;
        CurrentMaxAmmo = W.maxAmmo;

        _parent = GetParent<Node3D>();

        EmitSignalAmmoChanged(W);

        Camera = NodeUtils.GetChildWithNodeType<Camera3D>(this);

        ConsoleCommandSystem.RegisterInstance(this);

        _game = Game.GetGame(this);
        _player = _game.Player;
        _console = _game.Console;
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

        if (W != WeaponResource)
        {
            W = WeaponResource;
            CurrentAmmo = W.magazineSize;
            CurrentMaxAmmo = W.maxAmmo;
            EmitSignalGunSettingsChanged(W, WeaponResource);
        }
    }


    private void HandleShooting(double delta)
    {
        if (!_shooting && _shootingTime <= W.fireRate)
        {
            _shootingTime += (float)delta;
        }

        if (_hitboxEnabled && _shootingTime >= W.HitboxDuration)
        {
            _hitbox.Disable();
            _hitboxEnabled = false;
        }

        _magazineEmpty = CurrentAmmo <= 0;
        _magazineFull = CurrentAmmo == W.magazineSize;

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
        CurrentAmmo--;
        EmitSignalAmmoChanged(W);
        if (_hitboxTimer.IsStopped()) _hitboxTimer.Start();
        _shooting = false;
    }

    private void Reload(double delta)
    {
        _reloadingTimer += (float)delta;
        if (_reloadingTimer >= W.reloadTime)
        {
            EmitSignalEndedReloading();
            _reloading = false;
            int ammoNeeded = W.magazineSize - CurrentAmmo;
            int ammoToTake = Mathf.Min(ammoNeeded, CurrentMaxAmmo);
            CurrentAmmo += ammoToTake;
            CurrentMaxAmmo -= ammoToTake;
            _reloadingTimer = 0f;
            EmitSignalAmmoChanged(W);
        }
    }

    private bool CanShoot()
    {
        return !_reloading && _shootingTime >= W.fireRate && !_magazineEmpty;
    }

    private bool CanReload()
    {
        return !_reloading && !_magazineFull && CurrentMaxAmmo > 0;
    }
    private bool GetShootingPressed()
    {
        return (W.auto ? Input.IsActionPressed(InputHash.SHOOT) : Input.IsActionJustPressed(InputHash.SHOOT)) && CanShoot();
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

    #region CC
    [ConsoleCommand("change_weapon", "Changes current weapon (string)", true)]
    public void Cc_ChangeWeapon(string weaponPath)
    {
        string weaponPathCombined = Path.Combine(WEAPONS_PATH, weaponPath + ".tres");
        var loadedWeapon = ResourceLoader.Load<WeaponConfig>(weaponPathCombined);
        if (loadedWeapon != null)
        {
            WeaponResource = loadedWeapon;
            _game.Console?.Log($"Changed current weapon to {loadedWeapon}");
        }
        else
            _game.Console?.Log($"Couldn't find a gun at {weaponPathCombined}", DevConsole.LogLevel.ERROR);
    }

    [ConsoleCommand("current_weapon_set", "Change a setting of the current gun settings (cammo int, mammo int, damage int)")]
    public void Cc_CurrentWeaponSet(string type, int amount)
    {
        switch (type)
        {
            case "cammo":
                CurrentAmmo = amount;
                EmitSignalAmmoChanged(W);
                _console?.Log($"Changed current ammo to be {amount}");
                break;
            case "mammo":
                CurrentMaxAmmo = amount;
                EmitSignalAmmoChanged(W);
                _console?.Log($"Changed max ammo to be {amount}");
                break;
            case "damage":
                _hitbox.Damage = amount;
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
