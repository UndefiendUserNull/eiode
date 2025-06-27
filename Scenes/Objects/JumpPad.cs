using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Objects;


[Tool]
public partial class JumpPad : Area3D
{
    [Export] public float JumpPower { get; set; } = 15f;
    [Export] public bool IgnoreMaxJumpPadPower = false;
    private const int DEBUG_DRAW_DIVISION_BY = 12;

    private RayCast3D _ray;
#if TOOLS
    public override void _Ready()
    {
        _ray = NodeUtils.GetChildWithNodeType<RayCast3D>(this);
    }
    public override void _Process(double delta)
    {
        if (_ray.TargetPosition.Y == JumpPower / DEBUG_DRAW_DIVISION_BY) return;
        _ray.TargetPosition = Vector3.Up * (JumpPower / DEBUG_DRAW_DIVISION_BY);
    }
#endif
}

