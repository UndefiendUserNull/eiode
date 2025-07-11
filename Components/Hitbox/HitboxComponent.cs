using EIODE.Core;
using EIODE.Utils;
using Godot;
using System.Collections.Generic;

namespace EIODE.Components;

[GlobalClass]
public partial class HitboxComponent : Area3D, IComponent
{
    [Export] public int HitsLimit { get; set; } = 1;

    public int Damage { get; set; }
    private float _range = 1000f;
    public bool Enabled { get; private set; } = false;
    private bool _canHit = true;
    private int _hits = 0;
    public CollisionShape3D CollisionShape { get; set; } = null;
    private readonly List<HurtboxComponent> _hurtBoxesDetected = [];

    public override void _Ready()
    {
        CollisionShape = GetChild<CollisionShape3D>(0);
        AreaEntered += HitboxComponent_AreaEntered;
        AreaExited += HitboxComponent_AreaExited;

        SetCollisionLayerValue(CollisionLayers.HITBOX, true);
        SetCollisionMaskValue(CollisionLayers.HITTABLE, true);
    }

    public override void _ExitTree()
    {
        if (Game.GetGame(this).FirstLevelLoaded)
        {
            AreaEntered -= HitboxComponent_AreaEntered;
            AreaExited += HitboxComponent_AreaExited;
        }
    }

    /// <summary>
    /// Used only if the shape is a SeparationRayShape3D
    /// </summary>
    public void SetRange(float newRange)
    {
        _range = newRange;
        if (CollisionShape.Shape is SeparationRayShape3D sp) sp.Length = _range;
    }

    public void Disable(bool reset = true)
    {
        CollisionShape.Disabled = true;
        Enabled = false;
        if (reset) Reset();
    }

    public void Enable()
    {
        Enabled = true;
        CollisionShape.Disabled = false;
    }

    public void Reset()
    {
        _hits = 0;
        _canHit = true;
        _hurtBoxesDetected.Clear();
    }

    public void HitboxComponent_AreaEntered(Area3D area)
    {
        if (area is HurtboxComponent hurtBox)
        {
            if (!_canHit) return;

            _hurtBoxesDetected.Add(hurtBox);
        }

        foreach (var hurtBoxDetected in _hurtBoxesDetected)
        {
            _hits++;
            hurtBoxDetected.TakeDamage(Damage);
            if (_hits >= HitsLimit) _canHit = false;
        }
    }

    private void HitboxComponent_AreaExited(Area3D area)
    {
        if (area is HurtboxComponent hurtBox && _hurtBoxesDetected.Contains(hurtBox))
            _hurtBoxesDetected.Remove(hurtBox);
    }

    public HitboxComponent() { }
}
