using EIODE.Components;
using EIODE.Core;
using EIODE.Resources;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Projectiles;

public partial class ProjectileBase : RigidBody3D
{
    [Export] public ProjectileData Data { get; set; }
    /// <summary>
    /// Disable the <c>_enableHitboxTimer</c> and <c>_disableHitboxTimer</c>
    /// </summary>
    [Export] public bool DisableDefaultTimers { get; set; }
    public HitboxComponent Hitbox { get; set; }
    private Timer _enableHitboxTimer;
    private Timer _disableHitboxTimer;

    public override void _Ready()
    {
        Hitbox = NodeUtils.GetChildWithNodeType<HitboxComponent>(this);

        Hitbox.Damage = Data.Damage;
        Hitbox.Disable();
        Hitbox.BodyEntered += Hitbox_BodyEntered;

        _enableHitboxTimer = NodeUtils.GetChildWithName<Timer>("timer_enable_hitbox", this);
        _disableHitboxTimer = NodeUtils.GetChildWithName<Timer>("timer_disable_hitbox", this);

        _enableHitboxTimer.WaitTime = Data.TimerEnableHitboxWaitTime;
        _disableHitboxTimer.WaitTime = Data.TimerDisableHitboxWaitTime;

        if (!DisableDefaultTimers)
        {
            _enableHitboxTimer.Timeout += EnableHitboxTimer_Timeout;
            _disableHitboxTimer.Timeout += DisableHitboxTimer_Timeout;
        }

        GravityScale = Data.GravityScale;
    }

    public override void _ExitTree()
    {
        if (!DisableDefaultTimers && Game.GetGame(this).FirstLevelLoaded)
        {
            _enableHitboxTimer.Timeout -= EnableHitboxTimer_Timeout;
            _disableHitboxTimer.Timeout -= DisableHitboxTimer_Timeout;
        }
    }

    /// <summary>
    /// What happens after ProjectileData.TimerDisableHitboxWaitTime is passed/>
    /// </summary>
    protected virtual void DisableHitboxTimer_Timeout()
    {
        Hitbox.Disable();
    }

    /// <summary>
    /// What happens after ProjectileData.TimerEnableHitboxWaitTime is passed/>
    /// </summary>
    protected virtual void EnableHitboxTimer_Timeout()
    {
        Hitbox.Enable();
        _disableHitboxTimer.Start();
    }

    public virtual void ApplyShootingForce()
    {
        ApplyImpulse(-GlobalTransform.Basis.Z * Data.Force);
        _enableHitboxTimer.Start();
    }

    protected virtual void Hitbox_BodyEntered(Node3D body)
    {
        if (body is HurtboxComponent hurtbox)
        {
            hurtbox.TakeDamage(Data.Damage);
        }
    }
}
