using Godot;

namespace EIODE.Scenes.Tests;
public partial class T_ManualRotation : MeshInstance3D
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            RotateZ(-motion.Relative.X * 0.02f);
        }
    }
}
