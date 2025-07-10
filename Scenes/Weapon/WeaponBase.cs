using EIODE.Resources;
using Godot;

namespace EIODE.Scenes;

/// <summary>
/// <para>The base for all kind of weapons</para>
/// <para><b>Note:</b> Any class inheriting from this class should have any kind of <see cref="WeaponData"/> within it "see <see cref="RaycastWeaponBase"/> for examples"</para>
/// <para><b>Note:</b> Any class inheriting from this class and is a weapon that uses <see cref="WeaponAmmoData"/> make sure to set your reloading timer to <c>WeaponAmmoData.ReloadingTime</c> at the start"see the _Ready at <see cref="RaycastWeaponBase"/> for example"</para>
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

    public abstract bool IsAttacking();

}
