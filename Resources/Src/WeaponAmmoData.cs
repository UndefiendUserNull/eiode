using Godot;

namespace EIODE.Resources;

[GlobalClass]
public partial class WeaponAmmoData : Resource
{
    [Export] public int MagSize { get; set; }
    [Export] public int MaxAmmo { get; set; }
    public int CurrentAmmo { get; set; }
    public int CurrentMaxAmmo { get; set; }

    /// <summary>
    /// In seconds
    /// </summary>
    [Export] public float ReloadTime { get; set; } = 0.75f;

    public override bool Equals(object obj)
    {
        var weaponAmmoData = obj as WeaponAmmoData;

        return CurrentMaxAmmo == weaponAmmoData.CurrentMaxAmmo && CurrentAmmo == weaponAmmoData.CurrentAmmo;
    }
}
