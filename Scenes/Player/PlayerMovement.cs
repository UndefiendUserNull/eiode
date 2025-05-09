using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Player;

public partial class PlayerMovement : CharacterBody3D
{
    [Export] private float _sensitivity = 0.05f;
    private Node3D _head = null;
    private Vector3 _direction = Vector3.Zero;
    private bool _wantToJump = false;
    public override void _Ready()
    {
        _head = GetChild<Node3D>(0);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            CameraRotation(motion);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        MainInput();
    }
    private void MainInput()
    {
        _direction = Vector3.Zero;

        if (Input.IsActionPressed(InputHash.forward)) _direction -= Transform.Basis.Z;
        else if (Input.IsActionPressed(InputHash.backward)) _direction += Transform.Basis.Z;

        if (Input.IsActionPressed(InputHash.left)) _direction -= Transform.Basis.X;
        else if (Input.IsActionPressed(InputHash.right)) _direction += Transform.Basis.X;

        _wantToJump = Input.IsActionJustPressed(InputHash.jump);
    }
    private void CameraRotation(InputEventMouseMotion e)
    {
        RotateY(Mathf.DegToRad(-e.Relative.X * _sensitivity));
        _head.RotateX(Mathf.DegToRad(e.Relative.Y * _sensitivity));

        _head.Rotation = new Vector3(Mathf.Clamp(_head.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(90)), _head.Rotation.Y, _head.Rotation.Z);
    }

}
