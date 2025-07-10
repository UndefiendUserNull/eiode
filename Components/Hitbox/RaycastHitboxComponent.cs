using EIODE.Utils;
using Godot;

namespace EIODE.Components;

[GlobalClass]
public partial class RaycastHitboxComponent : RayCast3D, IComponent
{
    [Export] public int MaxHits { get; set; } = 1;
    /// <summary>
    /// Checks if the whole hitbox is enabled
    /// </summary>
    new public bool Enabled { get; private set; } = true;
    public int Damage { get; set; } = 10;
    private int _hits = 0;
    private float _range = 1000f;

    public override void _Ready()
    {
        CollideWithBodies = true;

        SetCollisionMaskValue(CollisionLayers.HITTABLE, true);
    }
    public void SetRange(float newRange)
    {
        TargetPosition = Vector3.Forward * newRange;
    }

    public void Disable()
    {
        base.Enabled = false;
        Enabled = false;
        _hits = 0;
    }

    public void Enable()
    {
        base.Enabled = true;
        Enabled = true;
        _hits = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsColliding() && GetCollider() is Area3D area) Collided(area);
    }

    public void Collided(Area3D area)
    {
        if (_hits >= MaxHits) return;
        if (area is HurtboxComponent hurtBox)
        {
            hurtBox.TakeDamage(Damage);
            _hits++;
        }
    }

}
