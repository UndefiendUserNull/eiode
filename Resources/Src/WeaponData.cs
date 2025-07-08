using EIODE.Scenes;
using Godot;

namespace EIODE.Resources;

[GlobalClass]
public partial class WeaponData : Resource
{
    /// <summary>
    /// If the name is shown in UI it's shown capitalized and replacing underscores with spaces
    /// </summary>
    [Export] public string Name { get; set; } = "new_weapon";
    [Export] public int Damage { get; set; } = 1;

    /// <summary>
    /// In seconds
    /// </summary>
    [Export] public float HitRate { get; set; } = 1.0f;

    [Export] public WeaponType WeaponType { get; set; } = WeaponType.RAYCAST;

    public override bool Equals(object obj)
    {
        var weaponData = obj as WeaponData;
        return this.Name == weaponData.Name && this.Damage == weaponData.Damage && this.HitRate == weaponData.HitRate && this.WeaponType == weaponData.WeaponType;
    }

    public WeaponData() { }
}

public enum WeaponType
{
    RAYCAST,
    PROJECTILE,
    MELEE,
    HYBIRD
}