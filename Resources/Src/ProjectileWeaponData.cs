using Godot;

namespace EIODE.Resources;

/// <summary>
/// The HitRate here is ignored
/// </summary>
[GlobalClass]
public partial class ProjectileWeaponData : WeaponData
{
    [Export] public PackedScene Projectile { get; set; }
    [Export] public int ProjectilePerShot { get; set; } = 1;

    // fuck you
    [Export] public NodePath[] ShootingPositions { get; set; } = [];

    public int ProjectilesLength { get; set; }

}
