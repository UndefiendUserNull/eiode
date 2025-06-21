using Godot;

namespace EIODE.Resources.Src;

[GlobalClass]
public partial class PlayerMovementSettings : Resource
{
    // The initial values are the default values used for the latest commit

    [Export] public float Sensitivity { get; set; } = 0.2f;
    [Export] public float Acceleration { get; set; } = 10f;
    [Export] public float AirControl { get; set; } = 5f;
    [Export] public float GravityRampStart { get; set; } = 0.3f;
    [Export] public float GravityRampMultiplier { get; set; } = 2.0f;
    [Export] public float GravityMinScale { get; set; } = 1.0f;
    [Export] public float GravityMaxScale { get; set; } = 3.0f;

    [Export] public float MaxVelocityAir { get; set; } = 20f;
    [Export] public float MaxVelocityGround { get; set; } = 22f;
    // Used in Init
    [Export] public float JumpModifier { get; set; } = 11.5f;
    // Used in Init
    [Export] public float Gravity { get; set; } = 110.0f;
    [Export] public float JumpBufferingTime { get; set; } = 0.12f;
    [Export] public float CoyoteTime { get; set; } = 0.15f;

    public PlayerMovementSettings() { }
}