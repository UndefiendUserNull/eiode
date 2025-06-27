using EIODE.Core;
using EIODE.Core.Console;
using EIODE.Resources.Src;
using EIODE.Utils;
using EIODE.Scenes.Objects;
using EIODE.Scenes.Triggers;
using System.Collections.Generic;
using System;
using Godot;
using System.Linq;

namespace EIODE.Scenes;
public partial class Player : CharacterBody3D, ITriggerable
{
    [Export] private PlayerMovementConfig _res_playerMovementConfig;

    public PlayerMovementConfig Conf { get; set; } = default;
    private float _jumpHeight = 0.0f;
    public Vector3 _direction = Vector3.Zero;
    private bool _wantToJump = false;
    private Head _head = null;
    private Area3D _feet = null; // :D
    private Game _game = null;
    private Camera3D _camera = null;
    private DevConsole _console;
    private float _cameraZRotation = 0f;
    public float JumpPadForce { get; set; } = 0f;
    public List<JumpPad> PrevJumpPads { get; set; } = [];
    public float _variableGravity = 0f;

    public Vector2 InputDirection { get; private set; } = Vector2.Zero;
    private bool _noClip = false;
    private bool _canTrigger = true;

    private const float DEFAULT_HEAD_Y_POSITION = 1.3f;
    private const float PLAYER_COLLISION_RADIUS = 0.3f;
    private const float PLAYER_COLLISION_HEIGHT = 3.0f;

    #region Timers
    public float _timeInAir { get; private set; } = 0f;
    public float _timeSinceLastJumpInput { get; private set; } = 0f;
    #endregion

    public override void _EnterTree() => Init();

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

        Conf = (PlayerMovementConfig)_res_playerMovementConfig.Duplicate();

        _variableGravity = Conf.Gravity;

        _feet = NodeUtils.GetChildWithName<Area3D>("Feet", this);
        _feet.AreaEntered += Feet_AreaEntered;

        _head = GetChild<Head>(0);

        _camera = _head.Camera;

        _jumpHeight = Mathf.Sqrt(2 * Conf.Gravity * Conf.JumpModifier);

        Input.MouseMode = Input.MouseModeEnum.Captured;

        _game = Game.GetGame(this);

        Validation();

        ConsoleCommandSystem.RegisterInstance(this);


        _console = _game.Console;

    }

    public override void _ExitTree()
    {
        _feet.AreaEntered -= Feet_AreaEntered;
    }

    private void Feet_AreaEntered(Area3D area)
    {
        if (area is JumpPad jp)
        {
            AddJumpPadForce(jp);
        }
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
        // No Clip
        if (_noClip)
        {
            Position += _direction * Conf.NoClipSpeed;
            if (Input.IsActionPressed(InputHash.NOCLIPUP))
            {
                Position += Vector3.Up * Conf.NoClipSpeed;
            }
            else if (Input.IsActionPressed(InputHash.NOCLUPDOWN))
            {
                Position += Vector3.Down * Conf.NoClipSpeed;
            }
            return;
        }

        Vector3 desiredDirection = _direction.LengthSquared() > 1f ? _direction.Normalized() : _direction;
        bool onFloor = IsOnFloor();

        if (_wantToJump)
            _timeSinceLastJumpInput += (float)delta;


        if (onFloor)
        {
            if (_timeInAir >= 0.02f)
            {
                JumpPadForce = 0f;
                _variableGravity = 0f;

                if (PrevJumpPads.Count > 0)
                    PrevJumpPads.Clear();
            }

            _timeInAir = 0.0f;
        }
        else
        {
            _timeInAir += (float)delta;

            _variableGravity = Conf.Gravity;

            if (_timeInAir > Conf.GravityRampStart)
            {
                float gravityScale = Conf.GravityMinScale + (_timeInAir - Conf.GravityRampStart) * Conf.GravityRampMultiplier;
                gravityScale = Mathf.Clamp(gravityScale, Conf.GravityMinScale, Conf.GravityMaxScale);
                _variableGravity *= gravityScale;
            }

            Velocity -= new Vector3(0, _variableGravity * (float)delta, 0);
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
        Vector3 desiredVelocity = direction * Conf.MaxVelocityGround;
        return Velocity.Lerp(desiredVelocity, 1f - Mathf.Exp(-Conf.Acceleration * (float)delta));
    }

    private Vector3 UpdateVelocityAir(Vector3 direction, double delta)
    {
        Vector3 desiredVelocity = direction * Conf.MaxVelocityAir;
        return Velocity.Lerp(desiredVelocity, 1f - Mathf.Exp(-Conf.AirControl * (float)delta));
    }

    private void Jump(Vector3 desiredDirection, double delta)
    {
        Vector3 horizontalVelocity = UpdateVelocityGround(desiredDirection, delta);
        Velocity = new Vector3(horizontalVelocity.X, _jumpHeight, horizontalVelocity.Z);
        _wantToJump = false;
        _timeSinceLastJumpInput = Conf.JumpBufferingTime + 1; // Ensure no immediate re-jump
    }

    private bool CanJump()
    {
        bool hasBufferedJump = _timeSinceLastJumpInput < Conf.JumpBufferingTime;
        bool canCoyoteJump = !IsOnFloor() && (_timeInAir < Conf.CoyoteTime);

        return (IsOnFloor() && hasBufferedJump) || canCoyoteJump;
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
    public void ForceSetVelocity(Vector3 newVelocity)
    {
        if (newVelocity == Vector3.Zero) return;
        Velocity = newVelocity;
        _wantToJump = false;
    }
    public void AddJumpPadForce(JumpPad jumpPad)
    {
        float jumpPower = jumpPad.JumpPower;

        if (jumpPower == 0) return;

        if (!PrevJumpPads.Any(x => x == jumpPad))
        {
            PrevJumpPads.Add(jumpPad);

            if (JumpPadForce >= Conf.MaxJumpPadPower)
            {
                ForceSetVelocity(Vector3.Up * Conf.MaxJumpPadPower);
            }
            else
            {
                _variableGravity = 0f;
                _timeInAir = 0f;

                if (!jumpPad.IgnoreMaxJumpPadPower)
                    JumpPadForce = Mathf.Min(Conf.MaxJumpPadPower, JumpPadForce + jumpPower);
                else
                    JumpPadForce += jumpPower;

                Velocity += new Vector3(0, JumpPadForce, 0);
            }
        }
        else
        {
            PrevJumpPads.Clear();
            JumpPadForce = jumpPower;
        }
        _wantToJump = false;
    }

    /// <summary>
    /// Resets player's Velocity, Rotation
    /// </summary>
    public void Reset()
    {
        Lock();
        GetHead().Rotation = Vector3.Zero;
        Velocity = Vector3.Zero;
        UnLock();
    }
    public bool CanTrigger()
    {
        return _canTrigger;
    }

    private void Validation()
    {
        if (Conf == null)
        {
            GD.PushError("PlayerMovementSettings resource not assigned.");
            return;
        }

        if (GetChild(0).GetChildOrNull<Camera3D>(0) == null) GD.PushError("The first child of the player doesn't have Camera3D as a first child");

        if (!Mathf.IsEqualApprox(GetChild<Node3D>(0).Position.Y, DEFAULT_HEAD_Y_POSITION)) GD.PushWarning($"Player's head position {_head.Position.Y} != {DEFAULT_HEAD_Y_POSITION}");
    }


    #region CC

    [ConsoleCommand("no_clip", "Weather to use No Clip or not (0 | 1)", true)]
    public void Cc_NoClip(int i = 1)
    {
        void On()
        {
            _noClip = true;
            _feet.Monitorable = false;
            _feet.Monitoring = false;
            _console?.Log("No Clip is on");
        }

        void Off()
        {
            _noClip = false;
            _feet.Monitorable = true;
            _feet.Monitoring = true;
            _console?.Log("No Clip is off");
        }

        switch (i)
        {
            // toggle if it's already on
            case 1:
                if (_noClip)
                {
                    Off();
                }
                else
                {
                    On();
                }
                break;
            case 0:
                Off();
                break;
        }
    }
    [ConsoleCommand("set_no_clip_speed", "(float)")]
    public void Cc_SetNoClipSpeed(float newSpeed)
    {
        _console?.Log($"Changed NoClipSpeed from {Conf.NoClipSpeed} to {newSpeed}");
        Conf.NoClipSpeed = newSpeed;
    }

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
                var _prevJumpMod = Conf.JumpModifier;
                Conf.JumpModifier = value;
                _console?.Log($"Changed _jumpModifier from {_prevJumpMod} to {Conf.JumpModifier}");
                break;
            case "grav":
                var _prevGrav = Conf.Gravity;
                if (value < 0f) _game.Console?.Log("Gravity value should be positive.", DevConsole.LogLevel.WARNING);
                Conf.Gravity = value;
                _console?.Log($"Changed _gravity from {_prevGrav} to {Conf.Gravity}");
                break;
            case "sens":
                var _prevSens = Conf.Sensitivity;
                Conf.Sensitivity = value;
                _console?.Log($"Changed _sensitivity from {_prevSens} to {Conf.Sensitivity}");
                break;
            case "accel":
                var _prevAccel = Conf.Acceleration;
                Conf.Acceleration = value;
                _console?.Log($"Changed _acceleration from {_prevAccel} to {Conf.Acceleration}");
                break;
            case "airctrl":
                var _prevAirCtrl = Conf.AirControl;
                Conf.AirControl = value;
                _console?.Log($"Changed _airControl from {_prevAirCtrl} to {Conf.AirControl}");
                break;
            case "maxair":
                var _prevMaxAir = Conf.MaxVelocityAir;
                Conf.MaxVelocityAir = value;
                _console?.Log($"Changed _maxVelocityAir from {_prevMaxAir} to {Conf.MaxVelocityAir}");
                break;
            case "maxground":
                var _prevMaxGround = Conf.MaxVelocityGround;
                Conf.MaxVelocityGround = value;
                _console?.Log($"Changed _maxVelocityGround from {_prevMaxGround} to {Conf.MaxVelocityGround}");
                break;
            case "jumpbuffer":
                var _prevJumpBuffer = Conf.JumpBufferingTime;
                Conf.JumpBufferingTime = value;
                _console?.Log($"Changed _jumpBufferingTime from {_prevJumpBuffer} to {Conf.JumpBufferingTime}");
                break;
            case "coyote":
                var _prevCoyote = Conf.CoyoteTime;
                Conf.CoyoteTime = value;
                _console?.Log($"Changed _coyoteTime from {_prevCoyote} to {Conf.CoyoteTime}");
                break;
            case "help":
                _console?.Log("jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote");
                break;
            default:
                _console?.Log($"Unknown parameter: {arg}", DevConsole.LogLevel.WARNING);
                break;
        }
        _jumpHeight = Mathf.Sqrt(2 * Conf.Gravity * Conf.JumpModifier);
    }

    [ConsoleCommand("movement_reset", "Resets given movement setting \"jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote\"")]
    public void Cc_MovementReset(string arg)
    {
        switch (arg)
        {
            case "jumpmod":
                Conf.JumpModifier = _res_playerMovementConfig.JumpModifier;
                _console?.Log($"_jumpModifier Reset to {Conf.JumpModifier}");
                break;
            case "grav":
                Conf.Gravity = _res_playerMovementConfig.Gravity;
                _console?.Log($"_gravity Reset to {Conf.Gravity}");
                break;
            case "sens":
                Conf.Sensitivity = _res_playerMovementConfig.Sensitivity;
                _console?.Log($"_sensitivity Reset to {Conf.Sensitivity}");
                break;
            case "accel":
                Conf.Acceleration = _res_playerMovementConfig.Acceleration;
                _console?.Log($"_acceleration Reset to {Conf.Acceleration}");
                break;
            case "airctrl":
                Conf.AirControl = _res_playerMovementConfig.AirControl;
                _console?.Log($"_airControl Reset to {Conf.AirControl}");
                break;
            case "maxair":
                Conf.MaxVelocityAir = _res_playerMovementConfig.MaxVelocityAir;
                _console?.Log($"_maxVelocityAir Reset to {Conf.MaxVelocityAir}");
                break;
            case "maxground":
                Conf.MaxVelocityGround = _res_playerMovementConfig.MaxVelocityGround;
                _console?.Log($"_maxVelocityGround Reset to {Conf.MaxVelocityGround}");
                break;
            case "jumpbuffer":
                Conf.JumpBufferingTime = _res_playerMovementConfig.JumpBufferingTime;
                _console?.Log($"_jumpBufferingTime Reset to {Conf.JumpBufferingTime}");
                break;
            case "coyote":
                Conf.CoyoteTime = _res_playerMovementConfig.CoyoteTime;
                _console?.Log($"_coyoteTime Reset to {Conf.CoyoteTime}");
                break;
            case "help":
                _console?.Log("Available reset parameters: jumpmod, grav, sens, accel, airctrl, maxair, maxground, jumpbuffer, coyote");
                break;
            default:
                _console?.Log($"Unknown reset parameter: {arg}");
                break;
        }
        _jumpHeight = Mathf.Sqrt(2 * Conf.Gravity * Conf.JumpModifier);

    }

    [ConsoleCommand("reset", "Resets player's position", true)]
    public void Cc_ResetPlayerPosition()
    {
        Position = Game.PlayerSpawnPosition;
        Velocity = Vector3.Zero;
        _console?.Log("Player set position to spawn Reset");
    }

    [ConsoleCommand("set_position_as_reset", "Sets PlayerSpawnPosition to the current position", true)]
    public void Cc_SetCurrentPositionAsReset()
    {
        Game.PlayerSpawnPosition = Position;
        Velocity = Vector3.Zero;
        _console?.Log($"Reset position set to {Game.PlayerSpawnPosition}");
    }

    [ConsoleCommand("gravity_ramp_set", "Sets gravity ramp values: \"start, mult, min, max\"")]
    public void Cc_GravityRampSet(string arg, float value = 0)
    {
        switch (arg)
        {
            case "start":
                Conf.GravityRampStart = value;
                _console?.Log($"GravityRampStart set to {Conf.GravityRampStart}");
                break;
            case "mult":
                Conf.GravityRampMultiplier = value;
                _console?.Log($"GravityRampMultiplier set to {Conf.GravityRampMultiplier}");
                break;
            case "min":
                Conf.GravityMinScale = value;
                _console?.Log($"GravityMinScale set to {Conf.GravityMinScale}");
                break;
            case "max":
                Conf.GravityMaxScale = value;
                _console?.Log($"GravityMaxScale set to {Conf.GravityMaxScale}");
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
        _console?.Log($"GravityRampStart: {Conf.GravityRampStart}, GravityRampMultiplier: {Conf.GravityRampMultiplier}, GravityMinScale: {Conf.GravityMinScale}, GravityMaxScale: {Conf.GravityMaxScale}");
    }

    [ConsoleCommand("movement_get", "Shows current movement values")]
    public void Cc_MovementGet()
    {
        _console?.Log(
            $"JumpModifier: {Conf.JumpModifier}, Gravity: {Conf.Gravity}, Sensitivity: {Conf.Sensitivity},\n " +
            $"Acceleration: {Conf.Acceleration}, AirControl: {Conf.AirControl}, MaxVelocityAir: {Conf.MaxVelocityAir},\n " +
            $"MaxVelocityGround: {Conf.MaxVelocityGround}, JumpBufferingTime: {Conf.JumpBufferingTime}, CoyoteTime: {Conf.CoyoteTime}"
        );
    }



    #endregion

}