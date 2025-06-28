using EIODE.Utils;
using Godot;
using System.Linq;

namespace EIODE.Scenes.Triggers;

#if TOOLS
[Tool]
#endif

public partial class TriggerVisibility : Trigger
{
    [Export] public Node3D Target { get; set; } = null;
    [Export] public bool StartHidden = true;
    [Export] public bool ShowOnlyWhileInside = false;
    private Color _debugDrawColor = Colors.Red;
    private bool _isVisible = false;
    private MeshInstance3D _triggerVisual = null;
    public override void _Ready()
    {
        base._Ready();
        if (StartHidden) Target.Hide();

        _triggerVisual = NodeUtils.GetChildWithNodeType<MeshInstance3D>(this);

        _triggerVisual.Hide();

#if DEBUG
        _triggerVisual.Show();
#endif

    }

    public void HideTriggerVisual()
    {
        _triggerVisual.Hide();
    }
    public void ShowTriggerVisual()
    {
        _triggerVisual.Show();
    }

    public override void Triggerr()
    {
        if (_body is Player)
        {
            Target.Show();
            _isVisible = true;
            base.Triggerr();
        }
    }
#if TOOLS
    public override void _Process(double delta)
    {
        _isVisible = Target.Visible | NodeUtils.GetChildrenWithNodeType<Node3D>(Target).All((x) => x.Visible);
        _debugDrawColor = _isVisible ? Colors.Green : Colors.Red;
        if (Target.GetChildCount() > 0)
        {
            foreach (var child in NodeUtils.GetChildrenWithNodeType<Node3D>(Target))
            {
                DebugDraw3D.DrawLine(GlobalPosition, child.GlobalPosition, _debugDrawColor);
            }
        }
        else
        {
            DebugDraw3D.DrawLine(GlobalPosition, Target.GlobalPosition, _debugDrawColor);
        }
    }
#endif
    public override void Trigger_BodyExited(Node3D body)
    {
        if (body is Player) UnTriggerr();
    }

    public override void UnTriggerr()
    {
        if (_body is Player)
        {
            Target.Hide();
            _isVisible = false;
            base.UnTriggerr();
        }
    }
}
