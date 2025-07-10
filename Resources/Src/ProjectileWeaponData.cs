using Godot;

namespace EIODE.Resources;

/// <summary>
/// The HitRate here is ignored
/// </summary>
[GlobalClass]
public partial class ProjectileWeaponData : WeaponData
{
    [Export] public PackedScene Projectile { get; set; }

    [Export] public int MaxProjectilesPerShot { get; set; }

    // fuck you
    [Export] public NodePath[] ShootingPositions { get; set; } = [];

    public int CurrentFilledShootingPositions = 0;

}
