using EIODE.Core.Console;
using EIODE.Resources.Src;
using EIODE.Utils;
using EIODE.Scripts.Core;
using System;
using Godot;

namespace EIODE.Scenes.Player;
public partial class Player : CharacterBody3D
{
    [Export] private PlayerMovementSettings _res_playerSettings;

    public PlayerMovementSettings S { get; set; } = default;
    private float _jumpHeight = 0.0f;
    public Vector3 _direction = Vector3.Zero;
    private bool _wantToJump = false;
    private Head _head = null;
    private RayCast3D _feet = null; // :D
    private Game _game = null;
    private Camera3D _camera = null;
    private DevConsole _console;
    private float _cameraZRotation = 0f;
    public Vector2 InputDirection { get; private set; } = Vector2.Zero;

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

    public override void _Process(double delta)
    {
        MainInput();
    }

    public override void _PhysicsProcess(double delta)
    {
        Movement(delta);
    }

    private void Init()
    {
        // Should be unlocked from outside
        Lock();

        S = (PlayerMovementSettings)_res_playerSettings.Duplicate();

        _feet = NodeUtils.GetChildWithNodeType<RayCast3D>(this);

        _head = GetChild<Head>(0);

        _camera = _head.Camera;

        _jumpHeight = Mathf.Sqrt(2 * S.Gravity * S.JumpModifier);

        Input.MouseMode = Input.MouseModeEnum.Captured;

        _game = Game.GetGame(this);

        Validation();

        ConsoleCommandSystem.RegisterInstance(this);

        _console = _game.Console;

    }

    private void MainInput()
    {
        _direction = Vector3.Zero;

        InputDirection = Input.GetVector(InputHash.LEFT, InputHash.RIGHT, InputHash.FORWARD, InputHash.BACKWARD);

        _direction = (Transform.Basis.Z * InputDirection.Y + Transform.Basis.X * InputDirection.X).Normalized();

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

        if (onFloor)
        {
            _timeInAir = 0.0f;
        }
        else
        {
            _timeInAir += (float)delta;
            float variableGravity = S.Gravity;

            if (_timeInAir > 0.3f)
            {
                float gravityScale = 1.0f + (_timeInAir - 0.3f) * 2.0f;
                gravityScale = Mathf.Clamp(gravityScale, 1.0f, 3.0f);
                variableGravity *= gravityScale;
            }

            Velocity -= new Vector3(0, variableGravity * (float)delta, 0);
        }

        if (_wantToJump && CanJump())
        {
            Jump(desiredDirection, delta);
        }
        Velocity = onFloor ? UpdateVelocityGround(desiredDirection, delta) : UpdateVelocityAir(desiredDirection, delta);
        MoveAndSlide();
    }

    private Vector3 UpdateVelocityGround(Vector3 direction, double delta)
    {
        Vector3 desiredVelocity = direction * S.MaxVelocityGround;
        return Velocity.Lerp(desiredVelocity, 1f - Mathf.Exp(-S.Acceleration * (float)delta));
    }

    private Vector3 UpdateVelocityAir(Vector3 direction, double delta)
    {
        Vector3 desiredVelocity = direction * S.MaxVelocityAir;
        return Velocity.Lerp(desiredVelocity, 1f - Mathf.Exp(-S.AirControl * (float)delta));
    }

    [Obsolete]
    private Vector3 AdjustedVelocityToSlope(Vector3 velocity)
    {
        if (_feet.IsColliding())
        {
            Vector3 normal = _feet.GetCollisionNormal();
            float angle = Mathf.RadToDeg(Mathf.Acos(normal.Dot(Vector3.Up)));

            if (angle > 5f && velocity.Y < 0)
            {
                float boost = 1f + (angle / 45f);
                return velocity * boost;
            }
        }
        return velocity;
    }

    private void Jump(Vector3 desiredDirection, double delta)
    {
        Vector3 horizontalVelocity = UpdateVelocityGround(desiredDirection, delta);
        Velocity = new Vector3(horizontalVelocity.X, _jumpHeight, horizontalVelocity.Z);
        _wantToJump = false;
        _timeSinceLastJumpInput = S.JumpBufferingTime + 1; // Ensure no immediate re-jump
    }

    private bool CanJump()
    {
        bool hasBufferedJump = _timeSinceLastJumpInput < S.JumpBufferingTime;
        bool canCoyoteJump = !IsOnFloor() && (_timeInAir < S.CoyoteTime);

        return (IsOnFloor() && hasBufferedJump) || canCoyoteJump;
    }

    private void CameraRotation(InputEventMouseMotion e)
    {
        // horizontal
        RotateY(Mathf.DegToRad(-e.Relative.X * S.Sensitivity));

        // vertical
        float newPitch = _head.Rotation.X + Mathf.DegToRad(-e.Relative.Y * S.Sensitivity);
        newPitch = Mathf.Clamp(newPitch, Mathf.DegToRad(MIN_PITCH), Mathf.DegToRad(MAX_PITCH));

        _head.Rotation = new Vector3(newPitch, _head.Rotation.Y, _head.Rotation.Z);
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
        if (_head != null) return _head;
        else
        {
            GD.PushError("Head in player is null");
            throw new NullReferenceException();
        }
    }

    public void AddForce(Vector3 force)
    {
        if (force == Vector3.Zero) return;
        Velocity += force;
        _wantToJump = false;
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

        if (!Mathf.IsEqualApprox(GetChild<Node3D>(0).Position.Y, DEFAULT_HEAD_Y_POSITION)) GD.PushWarning($"Player's head position {_head.Position.Y} != {DEFAULT_HEAD_Y_POSITION}");
    }


    #region CC

    [ConsoleCommand("player_move", "Moves Player To Given Position (x, y, z)", true)]
    public void Cc_MovePosition(float x, float y, float z)
    {
        var newPos = new Vector3(x, y, z);
        Position = newPos;
        _console?.Log($"Moved player to {newPos}");
    }

    [ConsoleCommand("player_move_ray", "Shoots a ray from the head and moves the player to the hit point", true)]
    public void Cc_MovePositionToRay()
    {
        int maxRayDistance = 5000;
        RayCast3D ray = new()
        {
            TargetPosition = Vector3.Forward * maxRayDistance,
            ExcludeParent = true,
            ProcessMode = ProcessModeEnum.Always,
            HitBackFaces = false,
            CollideWithBodies = true,
        };

        _head.Camera.AddChild(ray);
        ray.Position = _head.Camera.Position;
        ray.Rotation = _head.Camera.Rotation;

        ray.ForceUpdateTransform();
        ray.ForceRaycastUpdate();

        // Set the position to where the ray hit and move the player away from the hit surface using the radius and the height
        // so the player doesn't sink inside the hit surface
        if (ray.IsColliding())
        {
            Vector3 collisionPoint = ray.GetCollisionPoint();
            Vector3 newPos = new(collisionPoint.X - PLAYER_COLLISION_RADIUS, collisionPoint.Y + PLAYER_COLLISION_HEIGHT / 2, collisionPoint.Z - PLAYER_COLLISION_RADIUS);
            GlobalPosition = newPos;
            _console?.Log($"Moved player to {newPos}");
        }
        else _console?.Log("Ray didn't collide with any object.", DevConsole.LogLevel.ERROR);
        ray.QueueFree();
    }

    [ConsoleCommand("movement_set", "Sets movement values: \"jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote\"", true)]
    public void Cc_MovementSet(string arg, float value = 0)
    {
        switch (arg)
        {
            case "jumpmod":
                var _prevJumpMod = S.JumpModifier;
                S.JumpModifier = value;
                _console?.Log($"Changed _jumpModifier from {_prevJumpMod} to {S.JumpModifier}");
                break;
            case "grav":
                var _prevGrav = S.Gravity;
                if (value < 0f) _game.Console?.Log("Gravity value should be positive.", DevConsole.LogLevel.WARNING);
                S.Gravity = value;
                _console?.Log($"Changed _gravity from {_prevGrav} to {S.Gravity}");
                break;
            case "sens":
                var _prevSens = S.Sensitivity;
                S.Sensitivity = value;
                _console?.Log($"Changed _sensitivity from {_prevSens} to {S.Sensitivity}");
                break;
            case "accel":
                var _prevAccel = S.Acceleration;
                S.Acceleration = value;
                _console?.Log($"Changed _acceleration from {_prevAccel} to {S.Acceleration}");
                break;
            case "airctrl":
                var _prevAirCtrl = S.AirControl;
                S.AirControl = value;
                _console?.Log($"Changed _airControl from {_prevAirCtrl} to {S.AirControl}");
                break;
            case "maxair":
                var _prevMaxAir = S.MaxVelocityAir;
                S.MaxVelocityAir = value;
                _console?.Log($"Changed _maxVelocityAir from {_prevMaxAir} to {S.MaxVelocityAir}");
                break;
            case "maxground":
                var _prevMaxGround = S.MaxVelocityGround;
                S.MaxVelocityGround = value;
                _console?.Log($"Changed _maxVelocityGround from {_prevMaxGround} to {S.MaxVelocityGround}");
                break;
            case "jumpbuffer":
                var _prevJumpBuffer = S.JumpBufferingTime;
                S.JumpBufferingTime = value;
                _console?.Log($"Changed _jumpBufferingTime from {_prevJumpBuffer} to {S.JumpBufferingTime}");
                break;
            case "coyote":
                var _prevCoyote = S.CoyoteTime;
                S.CoyoteTime = value;
                _console?.Log($"Changed _coyoteTime from {_prevCoyote} to {S.CoyoteTime}");
                break;
            case "help":
                _console?.Log("jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote");
                break;
            default:
                _console?.Log($"Unknown parameter: {arg}", DevConsole.LogLevel.WARNING);
                break;
        }
        _jumpHeight = Mathf.Sqrt(2 * S.Gravity * S.JumpModifier);
    }

    [ConsoleCommand("movement_reset", "Resets given movement setting \"jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote\"")]
    public void Cc_MovementReset(string arg)
    {
        switch (arg)
        {
            case "jumpmod":
                S.JumpModifier = _res_playerSettings.JumpModifier;
                _console?.Log($"_jumpModifier Reset to {S.JumpModifier}");
                break;
            case "grav":
                S.Gravity = _res_playerSettings.Gravity;
                _console?.Log($"_gravity Reset to {S.Gravity}");
                break;
            case "sens":
                S.Sensitivity = _res_playerSettings.Sensitivity;
                _console?.Log($"_sensitivity Reset to {S.Sensitivity}");
                break;
            case "accel":
                S.Acceleration = _res_playerSettings.Acceleration;
                _console?.Log($"_acceleration Reset to {S.Acceleration}");
                break;
            case "airctrl":
                S.AirControl = _res_playerSettings.AirControl;
                _console?.Log($"_airControl Reset to {S.AirControl}");
                break;
            case "maxair":
                S.MaxVelocityAir = _res_playerSettings.MaxVelocityAir;
                _console?.Log($"_maxVelocityAir Reset to {S.MaxVelocityAir}");
                break;
            case "maxground":
                S.MaxVelocityGround = _res_playerSettings.MaxVelocityGround;
                _console?.Log($"_maxVelocityGround Reset to {S.MaxVelocityGround}");
                break;
            case "jumpbuffer":
                S.JumpBufferingTime = _res_playerSettings.JumpBufferingTime;
                _console?.Log($"_jumpBufferingTime Reset to {S.JumpBufferingTime}");
                break;
            case "coyote":
                S.CoyoteTime = _res_playerSettings.CoyoteTime;
                _console?.Log($"_coyoteTime Reset to {S.CoyoteTime}");
                break;
            case "help":
                _console?.Log("Available reset parameters: jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote");
                break;
            default:
                _console?.Log($"Unknown reset parameter: {arg}");
                break;
        }
        _jumpHeight = Mathf.Sqrt(2 * S.Gravity * S.JumpModifier);

    }

    [ConsoleCommand("reset", "Resets player's position", true)]
    public void Cc_ResetPlayerPosition()
    {
        Position = Game.PLAYER_SPAWN_POSITION;
        Velocity = Vector3.Zero;
        _console?.Log("Player set position to spawn Reset");
    }

    [ConsoleCommand("gravity_ramp_set", "Sets gravity ramp values: \"start, mult, min, max\"")]
    public void Cc_GravityRampSet(string arg, float value = 0)
    {
        switch (arg)
        {
            case "start":
                S.GravityRampStart = value;
                _console?.Log($"GravityRampStart set to {S.GravityRampStart}");
                break;
            case "mult":
                S.GravityRampMultiplier = value;
                _console?.Log($"GravityRampMultiplier set to {S.GravityRampMultiplier}");
                break;
            case "min":
                S.GravityMinScale = value;
                _console?.Log($"GravityMinScale set to {S.GravityMinScale}");
                break;
            case "max":
                S.GravityMaxScale = value;
                _console?.Log($"GravityMaxScale set to {S.GravityMaxScale}");
                break;
            case "help":
                _console?.Log("Available parameters: start, mult, min, max");
                break;
            default:
                _console?.Log($"Unknown parameter: {arg}", DevConsole.LogLevel.WARNING);
                break;
        }
    }

    [ConsoleCommand("gravity_ramp_get", "Shows current gravity ramp values")]
    public void Cc_GravityRampGet()
    {
        _console?.Log($"GravityRampStart: {S.GravityRampStart}, GravityRampMultiplier: {S.GravityRampMultiplier}, GravityMinScale: {S.GravityMinScale}, GravityMaxScale: {S.GravityMaxScale}");
    }

    [ConsoleCommand("movement_get", "Shows current movement values")]
    public void Cc_MovementGet()
    {
        _console?.Log(
            $"JumpModifier: {S.JumpModifier}, Gravity: {S.Gravity}, Sensitivity: {S.Sensitivity},\n " +
            $"Acceleration: {S.Acceleration}, AirControl: {S.AirControl}, MaxVelocityAir: {S.MaxVelocityAir},\n " +
            $"MaxVelocityGround: {S.MaxVelocityGround}, JumpBufferingTime: {S.JumpBufferingTime}, CoyoteTime: {S.CoyoteTime}"
        );
    }

    #endregion

}