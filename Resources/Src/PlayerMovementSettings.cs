using Godot;
using System;

namespace EIODE.Resources.Src;

[GlobalClass]
public partial class PlayerMovementSettings : Resource
{
    [Export] public float _sensitivity = 0.05f;
    [Export] public float _maxVelocityAir = 0.6f;
    [Export] public float _maxVelocityGround = 6.0f;
    // Used in Init
    [Export] public float _jumpModifier = 0.85f;
    // Value gets multiplied by maxVelocityGround on Init
    [Export] public float _maxAcceleration = 10;
    // Used in Init
    [Export] public float _gravity = 15.34f;
    [Export] public float _stopSpeed = 1.5f;
    [Export] public float _friction = 4;
    [Export] public float _jumpBufferingTime = 0.1f;
    [Export] public float _coyoteTime = 0.15f;

    public PlayerMovementSettings() { }
}
