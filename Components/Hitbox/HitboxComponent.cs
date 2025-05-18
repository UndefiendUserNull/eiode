using EIODE.Utils;
using Godot;

namespace EIODE.Components;
public partial class HitboxComponent : Area3D, IComponent
{
    [Export] public int Damage { get; set; } = 10;
    public bool Enabled { get; private set; } = false;

    [Export] private Masks.Collision _currentMasks = Masks.Collision.Hitbox;
    private CollisionShape3D _collisionShape = null;

    public override void _Ready()
    {
        AreaEntered += HitboxComponent_AreaEntered;
        _collisionShape = GetChild<CollisionShape3D>(0);
    }
    public override void _ExitTree()
    {
        AreaEntered -= HitboxComponent_AreaEntered;
    }
    public void ResetMask()
    {
        _currentMasks = Masks.Collision.Hitbox;
        SetMask();
    }
    public void Disable()
    {
        //_currentMasks &= ~Masks.Collision.Hitbox;
        //Enabled = false;
        //SetMask();
        _collisionShape.Disabled = true;
    }
    public void Enable()
    {
        //_currentMasks |= Masks.Collision.Hitbox;
        //Enabled = true;
        //SetMask();
        _collisionShape.Disabled = false;
    }
    private void SetMask()
    {
        CollisionMask = (uint)_currentMasks;
    }
    private void HitboxComponent_AreaEntered(Area3D area)
    {
        if (ComponentsUtils.GetChildWithComponent<HealthComponent>(area) is HealthComponent hc)
        {
            hc.TakeDamage(Damage);
        }
    }
    public HitboxComponent() { }
}
