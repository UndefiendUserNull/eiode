using Godot;

using EIODE.Utils;
using EIODE.Components;
public partial class t_Get : Node2D
{
    [Export] Node node;

    public override void _Ready()
    {
        var health = ComponentsUtils.GetChildWithComponent<HealthComponent>(node);
        health.TakeDamage(10);
        GD.Print(health.CurrentHealth);
    }
}
