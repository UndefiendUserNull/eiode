using EIODE.Scenes;
using Godot;

namespace EIODE.Resources;

[GlobalClass]
public partial class RaycastWeaponData : WeaponData
{
    /// <summary>
    /// In Godot units "meters"
    /// </summary>
    [Export] public float Range { get; set; } = 150.0f;

    /// <summary>
    /// In seconds
    /// </summary>
    [Export] public float HitboxDuration { get; set; } = 0.03f;

    /// <summary>
    /// In seconds
    /// </summary>
    [Export] public float ReloadTime { get; set; } = 0.75f;

}
