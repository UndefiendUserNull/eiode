using EIODE.Core;
using EIODE.Utils;
using Godot;
using System.Collections.Generic;

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
        _detectionArea.AreaEntered += DetectionArea_AreaEntered;

        _detectionArea.SetCollisionLayerValue(CollisionLayers.PROJECTILE, true);

        _detectionArea.SetCollisionMaskValue(CollisionLayers.HITTABLE, true);
        _detectionArea.SetCollisionMaskValue(CollisionLayers.ENEMY, true);
        _detectionArea.SetCollisionMaskValue(CollisionLayers.WORLD, true);
        _detectionArea.SetCollisionMaskValue(CollisionLayers.HITBOX, true);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (Game.GetGame(this).FirstLevelLoaded)
        {
            _detectionArea.BodyEntered -= DetectionArea_BodyEntered;
            _detectionArea.AreaEntered -= DetectionArea_AreaEntered;
        }
    }

    private void DetectionArea_BodyEntered(Node3D body)
    {
        if (body is CollisionObject3D && body is not Player)
        {
            if (body == GetParent()) return;
            Explode();
        }

        if (body != null && FreeAfterAnyImpact)
        {
            GetTree().CreateTimer(0.05f).Timeout += () =>
            {
                Hitbox.Disable();
                QueueFree();
            };
        }
    }

    private void DetectionArea_AreaEntered(Area3D area)
    {
        DetectionArea_BodyEntered(area);
    }

    protected override void EnableHitboxTimer_Timeout()
    {
        // blank, shouldn't be enabled unless on impact
    }

    private void Explode()
    {
        Hitbox.Enable();
        _detectionArea.BodyEntered -= DetectionArea_BodyEntered;
        _detectionArea.AreaEntered -= DetectionArea_AreaEntered;
        _detectionArea.QueueFree();
        GetTree().CreateTimer(Data.TimerDisableHitboxWaitTime).Timeout += () => { Hitbox.Disable(); };

    }
}
