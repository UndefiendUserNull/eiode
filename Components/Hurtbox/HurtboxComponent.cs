using Godot;
using EIODE.Utils;
using EIODE.Scripts.Core;
namespace EIODE.Components;

public partial class HurtboxComponent : Area3D, IComponent
{
    public HealthComponent HealthComponent { get; private set; }

    public override void _Ready()
    {
        if (ComponentsUtils.GetChildWithComponent<HealthComponent>(GetParent()) != null)
            HealthComponent = ComponentsUtils.GetChildWithComponent<HealthComponent>(GetParent());
        else
        {
            GD.PushError($"No health component found {Name}");
            SetProcess(false);
        }
        HealthComponent.OnDeath += HealthComponent_OnDeath;
        HealthComponent.OnTakeDamage += HealthComponent_OnTakeDamage;
    }

    private void HealthComponent_OnTakeDamage()
    {
        Game.GetGame(this).Console?.Log($"{GetParent().Name} : {HealthComponent.CurrentHealth}");
    }

    private void HealthComponent_OnDeath()
    {
        GetParent().QueueFree();
    }

    public void TakeDamage(HitboxComponent hitbox)
    {
        HealthComponent.TakeDamage(hitbox.Damage);
    }

    public void TakeDamage(int damage)
    {
        HealthComponent.TakeDamage(damage);
    }
    public HurtboxComponent() { }
}
