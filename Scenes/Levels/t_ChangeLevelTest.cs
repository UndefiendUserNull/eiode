using EIODE.Scenes.Player;
using EIODE.Scripts.Core;
using Godot;
using System;

namespace EIODE.Scenes.Levels;
public partial class t_ChangeLevelTest : Area3D
{
    private bool _entered = false;
    public override void _Ready()
    {
        BodyEntered += T_ChangeLevelTest_BodyEntered;
    }
    public override void _ExitTree()
    {
        BodyEntered -= T_ChangeLevelTest_BodyEntered;
    }
    private void T_ChangeLevelTest_BodyEntered(Node3D body)
    {
        if (body is PlayerMovement && !_entered)
        {
            _entered = true;
            LevelLoader.Instance.ChangeLevel(LevelLoader.LoadLevel("res://Scenes/Levels/platformer_00.tscn"));
        }
    }
}
