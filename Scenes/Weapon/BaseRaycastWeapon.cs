using Godot;
using EIODE.Resources;
using EIODE.Components;

namespace EIODE.Scenes;

/// <summary>
/// Base raycast, any weapon that uses raycast "Pistol, <see cref="LaserGun"/>, etc" should inherent from this class 
/// </summary>
public abstract partial class BaseRaycastWeapon : WeaponBase, IWeaponWithAmmo
{
    [Export] public RaycastHitboxComponent Hitbox { get; set; }
    [Export] public RaycastWeaponData Data { get; set; }
    [Export] public WeaponAmmoData AmmoData { get; set; }
    [Export] public Node3D MuzzlePosition { get; set; }

    protected float _shootingCooldown = 0f;
    protected bool _isReloading = false;
    protected float _reloadTimer = 0f;

    [Signal] public delegate void OnShotFiredEventHandler();
    [Signal] public delegate void OnReloadStartedEventHandler();
    [Signal] public delegate void OnReloadCompleteEventHandler();

    public override void _Ready()
    {
        base._Ready();
        Hitbox.Disable();
        Hitbox.SetRange(Data.Range);
        AmmoData.CurrentAmmo = AmmoData.MagSize;
        AmmoData.CurrentMaxAmmo = AmmoData.MaxAmmo;
    }

    /// <summary>
    /// Additional shooting functionality goes here, this first gets executed before the main logic
    /// </summary>
    public abstract void Shoot();

    /// <summary>
    /// Main shooting logic, shouldn't be overridden unless you really NEED to
    /// </summary>
    public override void Attack()
    {
        if (!CanAttack()) return;
        Shoot();

        _shootingCooldown = Data.HitRate;
        AmmoData.CurrentAmmo--;
        Hitbox.Damage = Data.Damage;
        Hitbox.Enable();
        GetTree().CreateTimer(Data.HitboxDuration).Timeout += ResetAttack;

        EmitSignalOnShotFired();
    }

    private void ResetAttack()
    {
        _shootingCooldown = 0f;
        Hitbox.Disable();
    }

    public override void ReloadPressed()
    {
        if (!CanReload()) return;
        _isReloading = true;
        CompleteReload();

        EmitSignalOnReloadStarted();
    }


    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_shootingCooldown > 0)
            _shootingCooldown -= (float)delta;

        if (_isReloading)
        {
            _reloadTimer -= (float)delta;
            if (_reloadTimer <= 0)
                CompleteReload();
        }
    }

    protected virtual void CompleteReload()
    {
        _reloadTimer = Data.ReloadTime;
        int ammoNeeded = AmmoData.MagSize - AmmoData.CurrentAmmo;
        int ammoToTake = Mathf.Min(ammoNeeded, AmmoData.CurrentMaxAmmo);

        AmmoData.CurrentAmmo += ammoToTake;
        AmmoData.CurrentMaxAmmo -= ammoToTake;
        _isReloading = false;

        EmitSignalOnReloadComplete();
    }

    protected virtual bool CanAttack()
    {
        return _shootingCooldown <= 0 &&
               AmmoData.CurrentAmmo > 0 &&
               !_isReloading;
    }

    protected virtual bool CanReload()
    {
        return !_isReloading &&
               AmmoData.CurrentAmmo < AmmoData.MagSize &&
               AmmoData.CurrentMaxAmmo > 0;
    }

    public override string GetWeaponName()
    {
        return Data.Name;
    }

    public override WeaponType GetWeaponType()
    {
        return Data.WeaponType;
    }

    public override WeaponData GetWeaponData()
    {
        return Data;
    }

    public override WeaponAmmoData GetWeaponAmmoData()
    {
        return AmmoData;
    }

}