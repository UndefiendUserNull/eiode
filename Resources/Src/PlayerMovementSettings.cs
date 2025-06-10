using Godot;
using System;

namespace EIODE.Resources.Src;

[GlobalClass]
public partial class PlayerMovementSettings : Resource
{
    [Export] public float _sensitivity = 0.05f;
    [Export] public float _acceleration = 5f;
    [Export] public float _airControl = 2f;
    [Export] public float _maxVelocityAir = 0.6f;
    [Export] public float _maxVelocityGround = 6.0f;
    // Used in Init
    [Export] public float _jumpModifier = 0.85f;
    // Used in Init
    [Export] public float _gravity = 15.34f;
    [Export] public float _jumpBufferingTime = 0.1f;
    [Export] public float _coyoteTime = 0.15f;

    public PlayerMovementSettings() { }
}
