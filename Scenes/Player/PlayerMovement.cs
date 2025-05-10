using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Player;

public partial class PlayerMovement : CharacterBody3D
{
    [Export] private float _sensitivity = 0.05f;
    [Export] private float _maxVelocityAir = 0.6f;
    [Export] private float _maxVelocityGround = 6.0f;
    // Used in Init
    [Export] private float _jumpModifier = 0.85f;
    // Value gets multiplied by maxVelocityGround on Init
    [Export] private float _maxAcceleration = 10;
    // Used in Init
    [Export] private float _gravity = 15.34f;
    [Export] private float _stopSpeed = 1.5f;
    [Export] private float _friction = 4;
    [Export] private float _coyoteTime = 0.15f;
    private const float DEFAULT_HEAD_Y_POSITION = 1.5f;
    private const float MIN_PITCH = -80f;
    private const float MAX_PITCH = 90f;

    private float _timeInAir = 0f;
    // Set in Init
    private float _jumpHeight = 0.0f;
    private Vector3 _direction = Vector3.Zero;
    private bool _wantToJump = false;
    private Node3D _head = null;
    public override void _Ready()
    {
        Init();
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
        Movement(delta);
    }
    private void Init()
    {
        _head = GetChild<Node3D>(0);
        Validation();
        _jumpHeight = Mathf.Sqrt(2 * _gravity * _jumpModifier);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void MainInput()
    {
        _direction = Vector3.Zero;

        if (Input.IsActionPressed(InputHash.FORWARD)) _direction -= Transform.Basis.Z;
        else if (Input.IsActionPressed(InputHash.BACKWARD)) _direction += Transform.Basis.Z;

        if (Input.IsActionPressed(InputHash.LEFT)) _direction -= Transform.Basis.X;
        else if (Input.IsActionPressed(InputHash.RIGHT)) _direction += Transform.Basis.X;

        _wantToJump = Input.IsActionJustPressed(InputHash.JUMP);
    }
    private void Movement(double delta)
    {
        Vector3 desiredDirection = _direction.Normalized();
        Velocity = UpdateVelocityGround(desiredDirection, delta);

        if (IsOnFloor()) _timeInAir = 0.0f;

        if (_wantToJump && (IsOnFloor() || (_timeInAir < _coyoteTime)))
        {
            Velocity = new Vector3(Velocity.X, _jumpHeight, Velocity.Z);
            Velocity = UpdateVelocityGround(desiredDirection, delta);
            _wantToJump = false;
        }
        if (!IsOnFloor())
        {
            _timeInAir += (float)delta;
            Velocity -= new Vector3(0, _gravity * (float)delta, 0);
            Velocity = UpdateVelocityAir(desiredDirection, delta);
        }
        MoveAndSlide();
    }
    private Vector3 Accelerate(Vector3 direction, float maxVelocity, double delta)
    {
        float currentSpeed = Velocity.Dot(direction);
        float increasingSpeed = Mathf.Clamp(maxVelocity - currentSpeed, 0, _maxAcceleration * (float)delta);
        return Velocity + increasingSpeed * direction;
    }

    private Vector3 UpdateVelocityGround(Vector3 direction, double delta)
    {
        float speed = Velocity.Length();
        if (speed != 0)
        {
            float control = Mathf.Max(_stopSpeed, speed);
            float drop = (control * _friction * (float)delta);
            Velocity *= Mathf.Max(speed - drop, 0) / speed;
        }
        return Accelerate(direction, _maxVelocityGround, delta);
    }

    private Vector3 UpdateVelocityAir(Vector3 direction, double delta)
    {
        return Accelerate(direction, _maxVelocityAir, delta);
    }

    private void CameraRotation(InputEventMouseMotion e)
    {
        RotateY(Mathf.DegToRad(-e.Relative.X * _sensitivity));
        _head.RotateX(Mathf.DegToRad(-e.Relative.Y * _sensitivity));

        _head.Rotation = new Vector3(Mathf.Clamp(_head.Rotation.X, Mathf.DegToRad(MIN_PITCH), Mathf.DegToRad(MAX_PITCH)), _head.Rotation.Y, _head.Rotation.Z);
    }

    /// <summary>
    /// Makes sure the Scene setup is correct
    /// </summary>
    private void Validation()
    {
        if (_head.GetChildOrNull<Camera3D>(0) == null) { GD.PushError("The first child of the player doesn't have Camera3D as a first child"); }
        if (_head.Position.Y != DEFAULT_HEAD_Y_POSITION) { GD.PushWarning($"Player's head position {_head.Position.Y} != {DEFAULT_HEAD_Y_POSITION}"); }
    }

}
