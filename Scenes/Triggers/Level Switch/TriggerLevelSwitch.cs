using EIODE.Core;
using Godot;

namespace EIODE.Scenes.Triggers;

public partial class TriggerLevelSwitch : Trigger
{
    [Export] public PackedScene NextScene { get; set; }

    public override void Triggerr()
    {
        if (_body is Player p)
        {
            p.Reset();
            LevelLoader.Instance.ChangeLevel(NextScene);
            base.Triggerr();
        }
    }
}
