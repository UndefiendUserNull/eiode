using EIODE.Core.Console;
using EIODE.Resources.Src;
using EIODE.Utils;
using System;
using Godot;
using EIODE.Scripts.Core;

namespace EIODE.Scenes.Player;

public partial class PlayerMovement : CharacterBody3D
{
    [Export] private PlayerMovementSettings _res_playerSettings;

    private PlayerMovementSettings S => _res_playerSettings;
    private float _jumpHeight = 0.0f;
    private Vector3 _direction = Vector3.Zero;
    private bool _wantToJump = false;
    private Head _headSrc = null;
    private RayCast3D _feet = null; // :D
    private Game _game = null;

    #region Constants
    private const float DEFAULT_HEAD_Y_POSITION = 1.5f;
    private const float MIN_PITCH = -80f;
    private const float MAX_PITCH = 90f;
    private const float PLAYER_COLLISION_RADIUS = 0.3f;
    private const float PLAYER_COLLISION_HEIGHT = 3.0f;
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
        // Should be unlocked from outside
        Lock();

        _feet = NodeUtils.GetChildWithNodeType<RayCast3D>(this);

        _headSrc = GetChild<Head>(0);

        _jumpHeight = Mathf.Sqrt(2 * S._gravity * S._jumpModifier);

        Input.MouseMode = Input.MouseModeEnum.Captured;

        _game = Game.GetGame(this);

        Validation();

        ConsoleCommandSystem.RegisterInstance(this);
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
        Vector3 desiredDirection = _direction.LengthSquared() > 1f ? _direction.Normalized() : _direction;
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
        if (onFloor) Velocity = AdjustedVelocityToSlope(Velocity);
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
        //float speed = Velocity.Length();
        //if (speed != 0)
        //{
        //    float control = Mathf.Max(S._stopSpeed, speed);
        //    float drop = (control * S._friction * (float)delta);
        //    Velocity *= Mathf.Max(speed - drop, 0) / speed;
        //}
        //return Accelerate(direction, S._maxVelocityGround, delta);
        Vector3 desiredVelocity = direction * S._maxVelocityGround;
        return Velocity.Lerp(desiredVelocity, 1f - Mathf.Exp(-S._acceleration * (float)delta));
    }

    private Vector3 UpdateVelocityAir(Vector3 direction, double delta)
    {
        //return Accelerate(direction, S._maxVelocityAir, delta);
        Vector3 desiredVelocity = direction * S._maxVelocityAir;
        return Velocity.Lerp(desiredVelocity, 1f - Mathf.Exp(-S._airControl * (float)delta));
    }

    private Vector3 AdjustedVelocityToSlope(Vector3 velocity)
    {
        if (_feet.CollideWithBodies)
        {
            Quaternion slopeRotation = new(Vector3.Up, _feet.GetCollisionNormal());
            Vector3 adjustedVelocity = slopeRotation * velocity;
            if (adjustedVelocity.Y < 0) return adjustedVelocity;
        }
        return velocity;
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
        _headSrc.RotateX(Mathf.DegToRad(-e.Relative.Y * S._sensitivity));

        _headSrc.Rotation = new Vector3(Mathf.Clamp(_headSrc.Rotation.X, Mathf.DegToRad(MIN_PITCH), Mathf.DegToRad(MAX_PITCH)), _headSrc.Rotation.Y, _headSrc.Rotation.Z);
    }
    public void Lock()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void UnLock()
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

        if (!_feet.Name.ToString().Equals("feet", StringComparison.CurrentCultureIgnoreCase)) GD.PushWarning("Raycast's name found in player is not \"feet\"");

        if (GetChild(0).GetChildOrNull<Camera3D>(0) == null) GD.PushError("The first child of the player doesn't have Camera3D as a first child");

        if (!Mathf.IsEqualApprox(GetChild<Node3D>(0).Position.Y, DEFAULT_HEAD_Y_POSITION)) GD.PushWarning($"Player's head position {_headSrc.Position.Y} != {DEFAULT_HEAD_Y_POSITION}");
    }

    [ConsoleCommand("player_move", "Moves Player To Given Position (x, y, z)", true)]
    public void Cc_MovePosition(float x, float y, float z)
    {
        Position = new Vector3(x, y, z);
    }
    [ConsoleCommand("player_move_ray", "Shoots a ray from the head and moves the player to the hit point", true)]
    public void Cc_MovePositionToRay()
    {
        RayCast3D ray = new()
        {
            TargetPosition = Vector3.Forward * 5000,
            ExcludeParent = true,
            ProcessMode = ProcessModeEnum.Always,
            HitBackFaces = false,
            CollideWithBodies = true,
        };

        _headSrc.Camera.AddChild(ray);
        ray.Position = _headSrc.Camera.Position;
        ray.Position = _headSrc.Camera.Position;

        ray.ForceUpdateTransform();
        ray.ForceRaycastUpdate();

        // Set the position to where the ray hit and move the player away from the hit surface using the radius and the height
        // so the player doesn't sink inside the hit surface
        if (ray.IsColliding()) GlobalPosition = new Vector3(ray.GetCollisionPoint().X - PLAYER_COLLISION_RADIUS, ray.GetCollisionPoint().Y + PLAYER_COLLISION_HEIGHT / 2, ray.GetCollisionPoint().Z - PLAYER_COLLISION_RADIUS);
        else _game.Console.Log("Ray didn't collide with any object.", DevConsole.LogLevel.ERROR);
        ray.QueueFree();
    }

    [ConsoleCommand("player_movement_set", "jumpmod (float), grav (float)", true)]
    public void Cc_MovementSet(string arg, float value)
    {
        switch (arg)
        {
            case "jumpmod":
                S._jumpModifier = value;
                break;
            case "grav":
                if (value < 0f) _game.Console.Log("Gravity value should be positive.", DevConsole.LogLevel.WARNING);
                S._gravity = value;
                break;
            default:
                break;
        }
        _jumpHeight = Mathf.Sqrt(2 * S._gravity * S._jumpModifier);
    }

}
