using Godot;

namespace EIODE.Scenes.Triggers;

public partial class Trigger : Area3D
{
    /// <summary>
    /// Trigger only if the entity fully entered the trigger "entered and exited" 
    /// </summary>
    [Export] public bool TriggerOnlyAfterFullyEnter = false;
    private bool _isTriggered = false;
    private bool _isBodyEntered = false;
    protected ITriggerable _body = null;

    public override void _Ready()
    {
        BodyEntered += Trigger_BodyEntered;
        BodyExited += Trigger_BodyExited;
    }

    public override void _ExitTree()
    {
        BodyEntered -= Trigger_BodyEntered;
        BodyExited -= Trigger_BodyExited;
    }
    public virtual void Trigger_BodyExited(Node3D body)
    {
        if (_isTriggered) return;

        if (TriggerOnlyAfterFullyEnter && _isBodyEntered && (_body != null && _body == body)) Triggerr();
    }

    public virtual void Trigger_BodyEntered(Node3D body)
    {
        if (_isTriggered) return;

        if (body is ITriggerable iTriggerableBody)
        {
            _body = iTriggerableBody;

            if (!TriggerOnlyAfterFullyEnter)
                Triggerr();
            else
                _isBodyEntered = true;
        }
    }

    /// <summary>
    /// The additional 'r' at the end is because the class name is Trigger and the method cannot have the same name as the class :D
    /// </summary>
    public virtual void Triggerr()
    {
        _isTriggered = true;
    }
    public virtual void UnTriggerr()
    {
        _isTriggered = false;
    }

    public bool IsTriggered()
    {
        return _isTriggered;
    }
}
