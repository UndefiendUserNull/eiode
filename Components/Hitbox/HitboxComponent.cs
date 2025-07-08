using EIODE.Utils;
using Godot;

namespace EIODE.Components;

[GlobalClass]
public partial class HitboxComponent : Area3D, IComponent
{
    [Export] public int Damage { get; set; } = 10;
    [Export] public int HitsLimit { get; set; } = 1;

    private float _range = 1000f;
    public bool Enabled { get; private set; } = false;
    private bool _canHit = true;
    private int _hits = 0;
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

    /// <summary>
    /// Used only if the shape is a SeparationRayShape3D
    /// </summary>
    public void SetRange(float newRange)
    {
        _range = newRange;
        if (_collisionShape.Shape is SeparationRayShape3D sp) sp.Length = _range;
    }

    public void Disable()
    {
        _collisionShape.Disabled = true;
    }

    public void Enable()
    {
        _collisionShape.Disabled = false;
    }

    public void ResetHits()
    {
        _hits = 0;
        _canHit = true;
    }

    public void HitboxComponent_AreaEntered(Area3D area)
    {
        if (area is HurtboxComponent hurtBox)
        {
            if (!_canHit) return;

            _hits++;
            hurtBox.TakeDamage(Damage);
            if (_hits >= HitsLimit) _canHit = false;
        }
    }

    public HitboxComponent() { }
}
