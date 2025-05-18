using Godot;
using EIODE.Utils;
namespace EIODE.Components;

public partial class HurtboxComponent : Area3D
{
    private HealthComponent _healthComponent;
    public override void _Ready()
    {
        if (ComponentsUtils.GetChildWithComponent<HealthComponent>(GetParent()) != null)
            _healthComponent = ComponentsUtils.GetChildWithComponent<HealthComponent>(GetParent());
        else
        {
            GD.PushError($"No health component found {Name}");
            SetProcess(false);
        }

        _healthComponent.OnHealthChange += _healthComponent_OnHealthChange;
    }

    private void _healthComponent_OnHealthChange(int health)
    {
        GD.Print(health);
    }

    public void TakeDamage(HitboxComponent hitbox)
    {
        _healthComponent.TakeDamage(hitbox.Damage);
    }

    public void TakeDamage(int damage)
    {
        _healthComponent.TakeDamage(damage);
    }
    public HurtboxComponent() { }
}
