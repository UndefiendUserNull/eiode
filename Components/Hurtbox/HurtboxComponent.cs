using Godot;
using EIODE.Utils;
namespace EIODE.Components;

[GlobalClass]
public partial class HurtboxComponent : Area3D, IComponent
{
    public HealthComponent HealthComponent { get; private set; }
    private Node3D _parent = null;

    public override void _Ready()
    {
        _parent = GetParent<Node3D>();

        if (ComponentsUtils.GetChildWithComponent<HealthComponent>(GetParent()) != null)
            HealthComponent = ComponentsUtils.GetChildWithComponent<HealthComponent>(GetParent());
        else
        {
            GD.PushError($"No health component found {Name}");
            SetProcess(false);
        }

        CollisionLayer = (uint)CollisionLayers.HITTABLE;
        CollisionMask = (uint)CollisionLayers.HITBOX;

        HealthComponent.OnDeath += HealthComponent_OnDeath;
        HealthComponent.OnTakeDamage += HealthComponent_OnTakeDamage;
    }

    private void HealthComponent_OnTakeDamage()
    {
        if (NodeUtils.GetChildrenWithNodeType<Label3D>(GetParent()) != null)
        {
            var label = NodeUtils.GetChildWithNodeType<Label3D>(GetParent());
            label.Text = HealthComponent.CurrentHealth.ToString();
        }
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
