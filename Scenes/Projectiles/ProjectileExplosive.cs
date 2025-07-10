using EIODE.Core;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Projectiles;

/// <summary>
/// A projectile that explodes on collision impact 
/// </summary>
public partial class ProjectileExplosive : ProjectileBase
{
    [Export] public bool FreeAfterAnyImpact = true;

    private Area3D _detectionArea;
    private CollisionShape3D _detectionAreaCollision = null;
    public override void _Ready()
    {
        base._Ready();
        _detectionArea = NodeUtils.GetChildWithName<Area3D>("detection_area", this);
        _detectionAreaCollision = NodeUtils.GetChildWithNodeType<CollisionShape3D>(_detectionArea);
        _detectionArea.BodyEntered += DetectionArea_BodyEntered;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (Game.GetGame(this).FirstLevelLoaded)
            _detectionArea.BodyEntered -= DetectionArea_BodyEntered;
    }

    private void DetectionArea_BodyEntered(Node3D body)
    {
        if (body != null && FreeAfterAnyImpact)
        {
            GetTree().CreateTimer(0.03f).Timeout += () =>
            {
                Hitbox.Disable();
                QueueFree();
            };
        }
        if (body is PhysicsBody3D && body is not Player)
        {
            Explode();
        }
    }

    protected override void EnableHitboxTimer_Timeout()
    {
        // blank, shouldn't be enabled unless on impact
    }

    private void Explode()
    {
        Hitbox.Enable();
        _detectionArea.BodyEntered -= DetectionArea_BodyEntered;
        _detectionArea.QueueFree();
        GetTree().CreateTimer(Data.TimerDisableHitboxWaitTime).Timeout += Hitbox.Disable;

    }
}
