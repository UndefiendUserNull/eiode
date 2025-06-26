using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Objects;


[Tool]
public partial class JumpPad : Area3D
{
    /// <summary>
    /// Jump power should be equal or less than PlayerMovementSettings.MaxLunchPadForce
    /// </summary>
    [Export] public float JumpPower { get; set; } = 15f;
    private const int DIVISION_BY = 12;
    private RayCast3D _ray;
#if TOOLS
    public override void _Ready()
    {
        _ray = NodeUtils.GetChildWithNodeType<RayCast3D>(this);
    }
    public override void _Process(double delta)
    {
        if (_ray.TargetPosition.Y == JumpPower / DIVISION_BY) return;
        _ray.TargetPosition = Vector3.Up * (JumpPower / DIVISION_BY);
    }
#endif
}

