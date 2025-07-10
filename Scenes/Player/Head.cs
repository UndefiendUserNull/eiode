using EIODE.Core.Console;
using EIODE.Utils;
using EIODE.Core;
using System.Collections.Generic;
using System;
using Godot;
using EIODE.Resources;

namespace EIODE.Scenes;
public partial class Head : Node3D
{
    [Export] public float CameraTiltSpeed { get; set; } = 10f;
    [Export] public float MaxCameraTiltRadian { get; set; } = 2f;

    private const float MIN_PITCH = -90f;
    private const float MAX_PITCH = 90f;

    private Node3D _parent = null;
    private Game _game = null;
    private DevConsole _console = null;
    private Player _player = null;
    private int _currentWeaponIndex = 0;
    private WeaponAmmoData _currentWeaponAmmoData = null;
    private Node3D _weaponsPosition = null;
    public Camera3D Camera { get; private set; } = null;
    public WeaponBase CurrentWeapon { get; private set; } = null;
    public List<WeaponBase> WeaponsInventory { get; private set; } = new();
    public RayCast3D RayCast { get; private set; }

    [Signal] public delegate void WeaponChangedEventHandler(WeaponBase current);
    [Signal] public delegate void AmmoChangedEventHandler(WeaponBase weaponAmmoData);
    public override void _Ready()
    {
        _parent = GetParent<Node3D>();
        _weaponsPosition = NodeUtils.GetChildWithName<Node3D>("weapons_position", this);
        RayCast = NodeUtils.GetChildWithName<RayCast3D>("RAY", this);
        Camera = NodeUtils.GetChildWithNodeType<Camera3D>(this);
        ConsoleCommandSystem.RegisterInstance(this);

        _game = Game.GetGame(this);
        _player = _game.Player;
        _console = _game.Console;

        // Initialize with default weapons if needed
        Cc_TankUp(0);
        ChangeCurrentWeapon(0, true);
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
        HandleWeaponInput();
        CameraTilting(delta, _player.InputDirection);
    }

    private void HandleWeaponInput()
    {
        if (CurrentWeapon == null) return;

        // switching
        if (Input.IsActionJustPressed(InputHash.K_E))
        {
            ChangeCurrentWeapon(_currentWeaponIndex + 1);
        }
        if (Input.IsActionJustPressed(InputHash.K_Q))
        {
            ChangeCurrentWeapon(_currentWeaponIndex - 1);
        }

        // actions
        bool isAmmoWeapon = CurrentWeapon is IWeaponWithAmmo;

        if (Input.IsActionJustPressed(InputHash.RELOAD) && isAmmoWeapon)
        {
            ((IWeaponWithAmmo)CurrentWeapon).ReloadPressed();

            EmitSignalAmmoChanged(CurrentWeapon);
        }

        if (Input.IsActionPressed(InputHash.SHOOT))
        {
            CurrentWeapon.Attack();

            if (CurrentWeapon is RaycastWeaponBase raycastWeapon && RayCast.IsColliding())
                raycastWeapon.Hitbox.TargetPosition = RayCast.GetCollisionPoint();
            else if (CurrentWeapon is RaycastWeaponBase raycastWeaponn && !RayCast.IsColliding())
                raycastWeaponn.Hitbox.SetRange(raycastWeaponn.Data.Range);

            if (isAmmoWeapon)
            {
                EmitSignalAmmoChanged(CurrentWeapon);
            }
        }
    }

    private void CameraTilting(double delta, Vector2 _inputDirection)
    {
        float desiredZRotation = -_inputDirection.X * MaxCameraTiltRadian;
        desiredZRotation = Mathf.DegToRad(desiredZRotation);

        Vector3 rot = Rotation;

        // Responsiveness, idk why it looks like this but it makes the Z tilting look cooler than using CameraTiltSpeed directly
        float t = 1f - Mathf.Exp(-CameraTiltSpeed * (float)delta);
        rot.Z = Mathf.LerpAngle(rot.Z, desiredZRotation, t);
        Rotation = rot;
    }

    public void AddWeaponToInventory(WeaponBase weapon)
    {
        if (!WeaponsInventory.Contains(weapon))
        {
            WeaponsInventory.Add(weapon);
            AddChild(weapon);
            weapon.Hide(); // Hide all weapons except current
        }
    }

    public void AddWeaponToInventory(WeaponBase[] weapons)
    {
        foreach (var weapon in weapons)
        {
            AddWeaponToInventory(weapon);
        }
    }

    public void AddWeaponToInventory(ReadOnlySpan<string> weaponsNames)
    {
        var foundWeapons = _game.FindWeapons([.. weaponsNames]);
        foreach (var foundWeapon in foundWeapons)
        {
            AddWeaponToInventory((WeaponBase)foundWeapon.Instantiate());
        }
    }

    public void ChangeCurrentWeapon(int index, bool forceSet = false)
    {
        if (WeaponsInventory == null || WeaponsInventory.Count == 0)
            return;

        if (!forceSet)
        {
            if (_currentWeaponIndex == index)
                return;

            // Handle index wrapping
            index = (index < 0) ? WeaponsInventory.Count - 1 :
                   (index >= WeaponsInventory.Count) ? 0 : index;
        }
        else
        {
            // Validate index when forced
            if (index < 0 || index >= WeaponsInventory.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        var newWeapon = WeaponsInventory[index];

        if (newWeapon == null) return;  // Skip if null weapon

        // Only change if different weapon or forced
        if (forceSet || !ReferenceEquals(CurrentWeapon, newWeapon) && !CurrentWeapon.IsAttacking())
        {
            CurrentWeapon?.Hide();

            _currentWeaponIndex = index;
            CurrentWeapon = newWeapon;

            try
            {
                CurrentWeapon.Show();
                if (CurrentWeapon.Position != _weaponsPosition.Position)
                    CurrentWeapon.Position = _weaponsPosition.Position;

                EmitSignalWeaponChanged(CurrentWeapon);

                //if (CurrentWeapon is IWeaponWithAmmo)
                //    EmitSignalAmmoChanged(CurrentWeapon);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    #region CC
    [ConsoleCommand("tankup", "Gives all weapons of given set (0 | 1 | 2 | 3)", true)]
    public void Cc_TankUp(int set)
    {
        switch (set)
        {
            case 0:
                AddWeaponToInventory(WeaponsSets.SET_0);
                break;
            //case 1:
            //    AddWeaponToInventory(WeaponsSets.SET_1);
            //break;
            default:
                break;
        }
    }

    [ConsoleCommand("weapons_size")]
    public void Cc_WeaponsSize()
    {
        _console?.Log($"Size: {WeaponsInventory.Count}");
    }

    [ConsoleCommand("weapon_givecurrentammo", "Gives big amount of ammo to the current weapon if it has WeaponAmmoData", true)]
    public void Cc_GiveCurrentAmmo()
    {
        int givenAmmo = 9999;
        if (CurrentWeapon is IWeaponWithAmmo weaponWithAmmo)
        {
            weaponWithAmmo.AmmoData.CurrentAmmo = givenAmmo;
            weaponWithAmmo.AmmoData.CurrentMaxAmmo = givenAmmo;
        }
        else
        {
            _console?.Log("Current weapon is not IWeaponWithAmmo", DevConsole.LogLevel.ERROR);

        }
        _console?.Log("Set current ammo to 9999");
    }

    [ConsoleCommand("weapon_ignorehitrate", "Ignores hit rate for the current weapon", true)]
    public void Cc_IgnoreHitRate()
    {
        if (CurrentWeapon != null)
        {
            if (CurrentWeapon.GetWeaponData() != null)
                CurrentWeapon.GetWeaponData().HitRate = 0;
            _console?.Log("Hit rate set to 0");
        }
    }

    [ConsoleCommand("weapon_sethitrate", "Sets current weapon hit rate", true)]
    public void Cc_SetHitRate(float value)
    {
        if (CurrentWeapon != null)
        {
            if (CurrentWeapon.GetWeaponData() != null)
                CurrentWeapon.GetWeaponData().HitRate = value;
            _console?.Log($"Hit rate set to {value}");
        }
    }

    [ConsoleCommand("desired_fov", "Sets default FOV (float)")]
    public void Cc_SetFov(float v)
    {
        Camera.Fov = v;
    }
    #endregion
}