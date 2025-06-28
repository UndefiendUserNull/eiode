using EIODE.Utils;
using Godot;

namespace EIODE.Components;
public partial class HitboxComponent : Area3D, IComponent
{
    [Export] public int Damage { get; set; } = 10;
    public bool Enabled { get; private set; } = false;

    private CollisionShape3D _collisionShape = null;

    public override void _Ready()
    {
        _collisionShape = GetChild<CollisionShape3D>(0);
    }
    public override void _EnterTree()
    {
        AreaEntered += HitboxComponent_AreaEntered;
    }
    public override void _ExitTree()
    {
        AreaEntered -= HitboxComponent_AreaEntered;
    }

    public void Disable()
    {
        _collisionShape.Disabled = true;
    }
    public void Enable()
    {
        _collisionShape.Disabled = false;
    }

    private void HitboxComponent_AreaEntered(Area3D area)
    {
        if (area is HurtboxComponent hurtBox)
        {
            hurtBox.TakeDamage(Damage);
        }
    }
    public HitboxComponent() { }
}
