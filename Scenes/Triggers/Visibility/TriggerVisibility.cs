using Godot;

namespace EIODE.Scenes.Triggers;

public partial class TriggerVisibility : Trigger
{
    [Export] public Node3D Target { get; set; } = null;
    [Export] public bool StartHidden = true;
    [Export] public bool ShowOnlyWhileInside = false;
    public override void _Ready()
    {
        base._Ready();
        if (StartHidden) Target.Hide();

    }

    public override void Triggerr()
    {
        if (_body is Player)
        {
            Target.Show();
            base.Triggerr();
        }
    }

    public override void Trigger_BodyExited(Node3D body)
    {
        if (body is Player) UnTriggerr();
    }

    public override void UnTriggerr()
    {
        if (_body is Player)
        {
            Target.Hide();
            base.UnTriggerr();
        }
    }
}
