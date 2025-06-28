using Godot;

namespace EIODE.Resources;
[GlobalClass]
public partial class WeaponConfig : Resource
{
    [Export] public string Name { get; set; } = "New Gun";
    [Export] public int DamagePerBullet { get; set; } = 10;
    [Export] public float FireRate { get; set; } = 0.5f;
    [Export] public int MaxAmmo { get; set; } = 100;
    [Export] public int MagazineSize = 25;
    // In seconds
    [Export] public float HitboxDuration { get; set; } = 0.03f;
    // In seconds
    [Export] public float ReloadTime { get; set; } = 2f;
    [Export] public bool Auto { get; set; } = false;
    [Export] public bool Infinity { get; set; } = false;
    // Default value is enough for most weapons
    // for melee weapons you may change this with a shorter range value
    [Export] public float Range { get; set; } = 10000f;
    public int CurrentAmmo { get; set; } = 0;
    public int CurrentMaxAmmo { get; set; } = 0;

    public WeaponConfig() { }
}
