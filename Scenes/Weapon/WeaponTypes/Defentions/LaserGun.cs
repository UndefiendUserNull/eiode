using EIODE.Components;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes;

public partial class LaserGun : BaseRaycastWeapon
{
    [Export] public float FullChargeDuration = 2.0f;
    [Export] public ChargeableComponent ChargeableComponent { get; set; }

    private bool _canShoot = false;
    private bool _charging = false;
    private bool _fullyCharged = false;
    private float _currentCharge = 0.0f;

    public override void _Ready()
    {
        base._Ready();
        ChargeableComponent = NodeUtils.GetChildWithNodeType<ChargeableComponent>(this);
        ChargeableComponent.FullChargeDuration = FullChargeDuration;
        ChargeableComponent.ChargeCompleted += ChargeableComponent_ChargeCompleted;
        ChargeableComponent.ChargeProgressChanged += ChargeableComponent_ChargeProgressChanged;
    }

    public override void _Process(double delta)
    {
        _charging = Input.IsMouseButtonPressed(MouseButton.Right);

        if (_charging)
        {
            ChargeableComponent.Charge((float)delta);
        }
        else if (!_charging && !_fullyCharged)
        {
            ChargeableComponent.DeCharge((float)delta);
        }

        GD.Print(ChargeableComponent.CurrentCharge);

        if (Input.IsActionJustPressed(InputHash.SHOOT))
        {
            Shoot();
        }
    }
    public override void Shoot()
    {
        Hitbox.SetRange(Data.Range);
        _canShoot = false;
        _fullyCharged = false;
        ChargeableComponent.InstantDeCharge();
    }

    private void ChargeableComponent_ChargeProgressChanged(float duration)
    {
        _currentCharge = duration;
    }

    private void ChargeableComponent_ChargeCompleted()
    {
        _canShoot = true;
        _fullyCharged = true;
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && _canShoot;
    }

    public override void _ExitTree()
    {
        ChargeableComponent.ChargeCompleted += ChargeableComponent_ChargeCompleted;
        ChargeableComponent.ChargeProgressChanged += ChargeableComponent_ChargeProgressChanged;
    }
}
