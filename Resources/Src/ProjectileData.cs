using Godot;

namespace EIODE.Resources;

[GlobalClass]
public partial class ProjectileData : Resource
{
    [Export] public int Damage = 15;
    [Export] public float Force = 20.0f;
    [Export] public float GravityScale = 3.5f;
    [Export] public float TimerEnableHitboxWaitTime = 0.7f;
    [Export] public float TimerDisableHitboxWaitTime = 0.7f;
}
