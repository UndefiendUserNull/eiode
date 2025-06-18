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

    private PlayerMovementSettings S = null;
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

        _jumpHeight = Mathf.Sqrt(2 * S._gravity * S._jumpModifier);

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

        if (onFloor) _timeInAir = 0.0f;
        else
        {
            _timeInAir += (float)delta;
            Velocity -= new Vector3(0, S._gravity * (float)delta, 0);
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
        Vector3 desiredVelocity = direction * S._maxVelocityGround;
        return Velocity.Lerp(desiredVelocity, 1f - Mathf.Exp(-S._acceleration * (float)delta));
    }



    private Vector3 UpdateVelocityAir(Vector3 direction, double delta)
    {
        Vector3 desiredVelocity = direction * S._maxVelocityAir;
        return Velocity.Lerp(desiredVelocity, 1f - Mathf.Exp(-S._airControl * (float)delta));
    }

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
        _timeSinceLastJumpInput = S._jumpBufferingTime + 1; // Ensure no immediate re-jump
    }

    private bool CanJump()
    {
        bool hasBufferedJump = _timeSinceLastJumpInput < S._jumpBufferingTime;
        bool canCoyoteJump = !IsOnFloor() && (_timeInAir < S._coyoteTime);

        return (IsOnFloor() && hasBufferedJump) || canCoyoteJump;
    }

    private void CameraRotation(InputEventMouseMotion e)
    {
        // horizontal
        RotateY(Mathf.DegToRad(-e.Relative.X * S._sensitivity));

        // vertical
        float newPitch = _head.Rotation.X + Mathf.DegToRad(-e.Relative.Y * S._sensitivity);
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
                var _prevJumpMod = S._jumpModifier;
                S._jumpModifier = value;
                _console?.Log($"Changed _jumpModifier from {_prevJumpMod} to {S._jumpModifier}");
                break;
            case "grav":
                var _prevGrav = S._gravity;
                if (value < 0f) _game.Console?.Log("Gravity value should be positive.", DevConsole.LogLevel.WARNING);
                S._gravity = value;
                _console?.Log($"Changed _gravity from {_prevGrav} to {S._gravity}");
                break;
            case "sens":
                var _prevSens = S._sensitivity;
                S._sensitivity = value;
                _console?.Log($"Changed _sensitivity from {_prevSens} to {S._sensitivity}");
                break;
            case "accel":
                var _prevAccel = S._acceleration;
                S._acceleration = value;
                _console?.Log($"Changed _acceleration from {_prevAccel} to {S._acceleration}");
                break;
            case "airctrl":
                var _prevAirCtrl = S._airControl;
                S._airControl = value;
                _console?.Log($"Changed _airControl from {_prevAirCtrl} to {S._airControl}");
                break;
            case "maxair":
                var _prevMaxAir = S._maxVelocityAir;
                S._maxVelocityAir = value;
                _console?.Log($"Changed _maxVelocityAir from {_prevMaxAir} to {S._maxVelocityAir}");
                break;
            case "maxground":
                var _prevMaxGround = S._maxVelocityGround;
                S._maxVelocityGround = value;
                _console?.Log($"Changed _maxVelocityGround from {_prevMaxGround} to {S._maxVelocityGround}");
                break;
            case "jumpbuffer":
                var _prevJumpBuffer = S._jumpBufferingTime;
                S._jumpBufferingTime = value;
                _console?.Log($"Changed _jumpBufferingTime from {_prevJumpBuffer} to {S._jumpBufferingTime}");
                break;
            case "coyote":
                var _prevCoyote = S._coyoteTime;
                S._coyoteTime = value;
                _console?.Log($"Changed _coyoteTime from {_prevCoyote} to {S._coyoteTime}");
                break;
            case "help":
                _console?.Log("jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote");
                break;
            default:
                _console?.Log($"Unknown parameter: {arg}", DevConsole.LogLevel.WARNING);
                break;
        }
        _jumpHeight = Mathf.Sqrt(2 * S._gravity * S._jumpModifier);
    }

    [ConsoleCommand("movement_reset", "Resets given movement setting \"jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote\"")]
    public void Cc_MovementReset(string arg)
    {
        switch (arg)
        {
            case "jumpmod":
                S._jumpModifier = _res_playerSettings._jumpModifier;
                _console?.Log($"_jumpModifier Reset to {S._jumpModifier}");
                break;
            case "grav":
                S._gravity = _res_playerSettings._gravity;
                _console?.Log($"_gravity Reset to {S._gravity}");
                break;
            case "sens":
                S._sensitivity = _res_playerSettings._sensitivity;
                _console?.Log($"_sensitivity Reset to {S._sensitivity}");
                break;
            case "accel":
                S._acceleration = _res_playerSettings._acceleration;
                _console?.Log($"_acceleration Reset to {S._acceleration}");
                break;
            case "airctrl":
                S._airControl = _res_playerSettings._airControl;
                _console?.Log($"_airControl Reset to {S._airControl}");
                break;
            case "maxair":
                S._maxVelocityAir = _res_playerSettings._maxVelocityAir;
                _console?.Log($"_maxVelocityAir Reset to {S._maxVelocityAir}");
                break;
            case "maxground":
                S._maxVelocityGround = _res_playerSettings._maxVelocityGround;
                _console?.Log($"_maxVelocityGround Reset to {S._maxVelocityGround}");
                break;
            case "jumpbuffer":
                S._jumpBufferingTime = _res_playerSettings._jumpBufferingTime;
                _console?.Log($"_jumpBufferingTime Reset to {S._jumpBufferingTime}");
                break;
            case "coyote":
                S._coyoteTime = _res_playerSettings._coyoteTime;
                _console?.Log($"_coyoteTime Reset to {S._coyoteTime}");
                break;
            case "help":
                _console?.Log("Available reset parameters: jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote");
                break;
            default:
                _console?.Log($"Unknown reset parameter: {arg}");
                break;
        }
        _jumpHeight = Mathf.Sqrt(2 * S._gravity * S._jumpModifier);

    }

    [ConsoleCommand("reset", "Resets player's position", true)]
    public void Cc_ResetPlayerPosition()
    {
        Position = Game.PLAYER_SPAWN_POSITION;
        Velocity = Vector3.Zero;
        _console?.Log("Player set position to spawn Reset");
    }
    #endregion
}
