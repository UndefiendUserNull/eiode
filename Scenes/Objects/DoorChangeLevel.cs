using EIODE.Scripts.Core;
using Godot;

namespace EIODE.Scenes.Objects;

public partial class DoorChangeLevel : Area3D
{
    // This is a test, it should use PackedScene instead
    [Export] public string NextScene { get; set; }
    private bool _entered = false;

    public override void _Ready()
    {
        BodyEntered += DoorChangeLevel_BodyEntered;
    }
    public override void _ExitTree()
    {
        BodyEntered -= DoorChangeLevel_BodyEntered;
    }

    private void DoorChangeLevel_BodyEntered(Node3D body)
    {
        if (body is Player.Player p && !_entered)
        {
            p.Reset();
            LevelLoader.Instance.ChangeLevel(NextScene);
            _entered = true;
        }
    }
}
