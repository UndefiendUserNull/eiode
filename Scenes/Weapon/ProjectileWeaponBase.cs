using EIODE.Core;
using EIODE.Resources;
using EIODE.Scenes.Projectiles;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes;

public partial class ProjectileWeaponBase : WeaponBase, IWeaponWithAmmo
{
    [Export] public ProjectileWeaponData Data { get; set; }
    [Export] public WeaponAmmoData AmmoData { get; set; }

    protected float _shootingCooldown = 0f;
    protected bool _isReloading = false;
    protected float _reloadTimer = 0f;

    public bool FinishedReloading { get; private set; } = false;

    public override void _Ready()
    {
        base._Ready();

        Data.ProjectilesLength = Data.ShootingPositions.Length;

        AmmoData.CurrentAmmo = AmmoData.MagSize;
        AmmoData.CurrentMaxAmmo = AmmoData.MaxAmmo;
        _reloadTimer = AmmoData.ReloadTime;
    }

    /// <summary>
    /// Additional shooting functionality goes here, this first gets executed before the main logic
    /// </summary>
    public virtual void Shoot() { }

    /// <summary>
    /// Main shooting logic, shouldn't be overridden unless you really NEED to
    /// </summary>
    public override void Attack()
    {
        if (!CanAttack()) return;
        Shoot();

        _shootingCooldown = Data.HitRate;

        for (int i = 0; i < Data.ProjectilesLength; i++)
        {
            var spawnedProjectile = Data.Projectile.Instantiate<ProjectileBase>();
            Node3D spawnNode = GetNode<Node3D>($"{Data.ShootingPositions[i]}");

            if (spawnedProjectile.GetParent() != null)
                spawnedProjectile.Reparent(LevelLoader.Instance?.CurrentLevel);
            else
                LevelLoader.Instance?.CurrentLevel.AddChild(spawnedProjectile);

            spawnedProjectile.GlobalPosition = spawnNode.GlobalPosition;
            spawnedProjectile.GlobalRotation = spawnNode.GlobalRotation;
            spawnedProjectile.GlobalBasis = spawnNode.GlobalBasis;

            spawnedProjectile.ApplyShootingForce();
            AmmoData.CurrentAmmo -= Data.ProjectilePerShot;
        }

    }

    /// <summary>
    /// What gets executed when the reload button is pressed
    /// </summary>
    public virtual void ReloadPressed()
    {
        if (!CanReload()) return;
        _isReloading = true;
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
                ReloadCompleted();
        }
    }

    /// <summary>
    /// What gets executed when the reload timer reaches 0
    /// </summary>
    public virtual void ReloadCompleted()
    {
        _reloadTimer = AmmoData.ReloadTime;
        int ammoNeeded = AmmoData.MagSize - AmmoData.CurrentAmmo;
        int ammoToTake = Mathf.Min(ammoNeeded, AmmoData.CurrentMaxAmmo);

        AmmoData.CurrentAmmo += ammoToTake;
        AmmoData.CurrentMaxAmmo -= ammoToTake;

        _isReloading = false;
        FinishedReloading = true;
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

    public bool IsReloading()
    {
        return _isReloading;
    }
}

