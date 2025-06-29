using Godot;

namespace EIODE.Components;

public partial class RaycastHitboxComponent : RayCast3D, IComponent
{
    [Export] public int Damage { get; set; } = 10;

    private float _range = 1000f;

    /// <summary>
    /// Used only if the shape is a SeparationRayShape3D
    /// </summary>
    public void SetRange(float newRange)
    {
        TargetPosition = Vector3.Forward * newRange;
    }

    public void Disable()
    {
        Enabled = false;
    }

    public void Enable()
    {
        Enabled = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsColliding() && GetCollider() is Area3D area) Collided(area);
    }

    public void Collided(Area3D area)
    {
        if (area is HurtboxComponent hurtBox)
        {
            hurtBox.TakeDamage(Damage);
        }
    }

}
