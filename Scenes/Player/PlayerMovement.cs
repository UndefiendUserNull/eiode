using EIODE.Resources.Src;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Player;

public partial class PlayerMovement : CharacterBody3D
{
    [Export] private PlayerMovementSettings _res_playerSettings;

    private PlayerMovementSettings S => _res_playerSettings;
    private float _jumpHeight = 0.0f;
    private Vector3 _direction = Vector3.Zero;
    private bool _wantToJump = false;
    private Node3D _head = null;
    private Head _headSrc = null;

    #region Constants
    private const float DEFAULT_HEAD_Y_POSITION = 1.5f;
    private const float MIN_PITCH = -80f;
    private const float MAX_PITCH = 90f;
    #endregion

    #region Timers
    public float _timeInAir { get; private set; } = 0f;
    public float _timeSinceLastJumpInput { get; private set; } = 0f;
    #endregion

    public override void _EnterTree() => Init();

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
            CameraRotation(motion);
    }

    public override void _PhysicsProcess(double delta)
    {
        MainInput();
        Movement(delta);
    }

    private void Init()
    {
        Validation();
        // Should be unlocked from outside
        Lock();
        _head = GetChild<Node3D>(0);
        _headSrc = _head as Head;
        _jumpHeight = Mathf.Sqrt(2 * S._gravity * S._jumpModifier);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void MainInput()
    {
        _direction = Vector3.Zero;

        if (Input.IsActionPressed(InputHash.FORWARD)) _direction -= Transform.Basis.Z;
        else if (Input.IsActionPressed(InputHash.BACKWARD)) _direction += Transform.Basis.Z;

        if (Input.IsActionPressed(InputHash.LEFT)) _direction -= Transform.Basis.X;
        else if (Input.IsActionPressed(InputHash.RIGHT)) _direction += Transform.Basis.X;

        if (Input.IsActionJustPressed(InputHash.JUMP))
        {
            _wantToJump = true;
            _timeSinceLastJumpInput = 0.0f;
        }
    }

    private void Movement(double delta)
    {
        Vector3 desiredDirection = _direction.Normalized();
        bool onFloor = IsOnFloor();

        if (_wantToJump)
            _timeSinceLastJumpInput += (float)delta;

        if (_timeSinceLastJumpInput > S._jumpBufferingTime * 2 && onFloor)
            _timeSinceLastJumpInput = S._jumpBufferingTime + 1;

        if (onFloor)
            _timeInAir = 0.0f;
        else
        {
            _timeInAir += (float)delta;
            Velocity -= new Vector3(0, S._gravity * (float)delta, 0);
            Velocity = UpdateVelocityAir(desiredDirection, delta);
        }

        if (_wantToJump && CanJump())
        {
            Jump(desiredDirection, delta);
        }

        Velocity = onFloor ? UpdateVelocityGround(desiredDirection, delta) : UpdateVelocityAir(desiredDirection, delta);
        MoveAndSlide();
    }

    private Vector3 Accelerate(Vector3 direction, float maxVelocity, double delta)
    {
        float currentSpeed = Velocity.Dot(direction);
        float increasingSpeed = Mathf.Clamp(maxVelocity - currentSpeed, 0, S._maxAcceleration * (float)delta);
        return Velocity + increasingSpeed * direction;
    }

    private Vector3 UpdateVelocityGround(Vector3 direction, double delta)
    {
        float speed = Velocity.Length();
        if (speed != 0)
        {
            float control = Mathf.Max(S._stopSpeed, speed);
            float drop = (control * S._friction * (float)delta);
            Velocity *= Mathf.Max(speed - drop, 0) / speed;
        }
        return Accelerate(direction, S._maxVelocityGround, delta);
    }

    private Vector3 UpdateVelocityAir(Vector3 direction, double delta)
    {
        return Accelerate(direction, S._maxVelocityAir, delta);
    }

    private void Jump(Vector3 desiredDirection, double delta)
    {
        Vector3 horizontalVelocity = UpdateVelocityGround(desiredDirection, delta);
        Velocity = new Vector3(horizontalVelocity.X, _jumpHeight, horizontalVelocity.Z);
        _wantToJump = false;
    }

    private bool CanJump()
    {
        return (IsOnFloor() && (_timeSinceLastJumpInput < S._jumpBufferingTime)) ||
               (!IsOnFloor() && (_timeInAir < S._coyoteTime));
    }

    private void CameraRotation(InputEventMouseMotion e)
    {
        RotateY(Mathf.DegToRad(-e.Relative.X * S._sensitivity));
        _head.RotateX(Mathf.DegToRad(-e.Relative.Y * S._sensitivity));

        _head.Rotation = new Vector3(Mathf.Clamp(_head.Rotation.X, Mathf.DegToRad(MIN_PITCH), Mathf.DegToRad(MAX_PITCH)), _head.Rotation.Y, _head.Rotation.Z);
    }
    public void Lock()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void Unlock()
    {
        ProcessMode = ProcessModeEnum.Inherit;
    }
    public Head GetHead()
    {
        if (_headSrc != null) return _headSrc;
        else
        {
            GD.PushError("Head in player is null");
            return null;
        }
    }

    private void Validation()
    {
        if (S == null)
        {
            GD.PushError("PlayerMovementSettings resource not assigned.");
            return;
        }

        if (GetChild(0).GetChildOrNull<Camera3D>(0) == null)
            GD.PushError("The first child of the player doesn't have Camera3D as a first child");

        if (!Mathf.IsEqualApprox(GetChild<Node3D>(0).Position.Y, DEFAULT_HEAD_Y_POSITION))
            GD.PushWarning($"Player's head position {_head.Position.Y} != {DEFAULT_HEAD_Y_POSITION}");
    }
}
