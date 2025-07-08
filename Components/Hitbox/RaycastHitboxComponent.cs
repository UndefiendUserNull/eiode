using Godot;

namespace EIODE.Components;

[GlobalClass]
public partial class RaycastHitboxComponent : RayCast3D, IComponent
{
    [Export] public int Damage { get; set; } = 10;
    [Export] public int MaxHits { get; set; } = 1;
    private int _hits = 0;
    private float _range = 1000f;

    public override void _Ready()
    {
        CollideWithBodies = true;
    }
    public void SetRange(float newRange)
    {
        TargetPosition = Vector3.Forward * newRange;
    }

    public void Disable()
    {
        Enabled = false;
        _hits = 0;
    }

    public void Enable()
    {
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
