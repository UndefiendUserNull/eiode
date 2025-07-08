using EIODE.Resources;
using Godot;

namespace EIODE.Scenes;

/// <summary>
/// <para>The base for all kind of weapons</para>
/// <para><b>Note:</b> Any class inheriting from this class should have any kind of <see cref="WeaponData"/> within it "see <see cref="RaycastWeaponBase"/> for examples"</para>
/// </summary>

public abstract partial class WeaponBase : Node3D
{
    public abstract void Attack();
    public abstract string GetWeaponName();
    public abstract WeaponType GetWeaponType();
    public abstract WeaponData GetWeaponData();

    /// <summary>
    /// Optional, used only with non-melee weapons
    /// </summary>
    public abstract WeaponAmmoData GetWeaponAmmoData();
}
