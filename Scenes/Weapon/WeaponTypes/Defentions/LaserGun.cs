using EIODE.Components;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes;

public partial class LaserGun : RaycastWeaponBase
{
    [Export] public ChargeableComponent ChargeableComponent { get; set; }

    private bool _canShoot = false;
    private bool _fullyCharged = false;
    private float _currentCharge = 0.0f;

    public override void _Ready()
    {
        base._Ready();
        ChargeableComponent = NodeUtils.GetChildWithNodeType<ChargeableComponent>(this);
        ChargeableComponent.ChargeCompleted += ChargeableComponent_ChargeCompleted;
        ChargeableComponent.ChargeProgressChanged += ChargeableComponent_ChargeProgressChanged;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        ChargeableComponent.Charging = Input.IsMouseButtonPressed(MouseButton.Right);

        if (ChargeableComponent.Charging)
        {
            ChargeableComponent.Charge((float)delta);
        }
        else if (!ChargeableComponent.Charging && !_fullyCharged)
        {
            ChargeableComponent.DeCharge((float)delta);
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
    protected override bool CanReload()
    {
        return base.CanReload() && (!_fullyCharged || !ChargeableComponent.Charging) || AmmoData.CurrentAmmo == 0;
    }

    public override void _ExitTree()
    {
        ChargeableComponent.ChargeCompleted += ChargeableComponent_ChargeCompleted;
        ChargeableComponent.ChargeProgressChanged += ChargeableComponent_ChargeProgressChanged;
    }
}
