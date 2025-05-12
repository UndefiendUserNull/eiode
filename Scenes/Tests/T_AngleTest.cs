using Godot;
namespace EIODE.Scenes.Tests;

public partial class T_AngleTest : RayCast3D
{

    public override void _Process(double delta)
    {
        GD.Print(Mathf.RadToDeg(Mathf.Atan2(GetCollisionNormal().X, GetCollisionNormal().Y)));
    }
}
