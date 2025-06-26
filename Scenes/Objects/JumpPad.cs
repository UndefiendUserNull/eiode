using Godot;
using System.Linq;

namespace EIODE.Scenes.Objects;

public partial class JumpPad : Area3D
{
    /// <summary>
    /// Jump power should be equal or less than PlayerMovementSettings.MaxLunchPadForce
    /// </summary>
    [Export] public float JumpPower { get; set; } = 15f;

}