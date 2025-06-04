using Godot;

namespace EIODE.Scripts.Core.Level;
public abstract partial class Objective : GodotObject
{
    [Signal] public delegate void ObjectiveCompletedEventHandler();
    [Signal] public delegate void ObjectiveProgressedEventHandler(float progress);

    public string ID { get; protected set; }
    public string Description { get; protected set; }
    public bool IsCompleted { get; protected set; }
    public bool IsOptional { get; protected set; }

    public abstract void Activate();
    public abstract void Deactivate();

    protected void Complete()
    {
        IsCompleted = true;
        EmitSignal(SignalName.ObjectiveCompleted);
        Deactivate();
    }

    protected void UpdateProgress(float progress)
    {
        EmitSignal(SignalName.ObjectiveProgressed, progress);
    }
}

