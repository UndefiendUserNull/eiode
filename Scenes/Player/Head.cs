using EIODE.Components;
using EIODE.Resources.Src;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Player;
public partial class Head : Node3D
{
    [Export] public float ShootingRayLength { get; set; } = -1000;
    [Export] private Gun _currentGunSettings = null;

    public bool _shooting = false;
    public bool _reloading = false;
    // used in special cases
    private bool _magazineFull = false;
    public bool _magazineEmpty = false;
    public float _shootingTime = 0.0f;
    public int CurrentAmmo { get; private set; } = 0;
    public int CurrentMaxAmmo { get; private set; } = 0;
    public float _reloadingTimer = 0f;
    public bool _hitboxEnabled = false;
    private bool _hitboxTimerEnded = false;
    private HitboxComponent _hitbox = null;
    private Node3D _parent = null;
    private Timer _hitboxTimer = null;
    public Gun G => _currentGunSettings;

    [Signal] public delegate void AmmoChangedEventHandler(int currentAmmo, int currentMaxAmmo);

    public override void _Ready()
    {
        if (_currentGunSettings == null) GD.PushError("No gun settings was given to player");
        _hitbox = ComponentsUtils.GetChildWithComponent<HitboxComponent>(this);
        // There should be only one timer as a child for the "Head" node
        _hitboxTimer = NodeUtils.GetChildWithNodeType<Timer>(this);
        _hitbox.Damage = G.damagePerBullet;
        _hitbox.Disable();
        CurrentAmmo = G.magazineSize;
        CurrentMaxAmmo = G.maxAmmo;
        _parent = GetParent<Node3D>();
    }



    public override void _Process(double delta)
    {
        HandleShooting(delta);
    }


    private void HandleShooting(double delta)
    {
        if (!_shooting && _shootingTime <= G.fireRate)
        {
            _shootingTime += (float)delta;
        }
        if (_hitboxEnabled && _shootingTime >= G.HitboxDuration)
        {
            GD.Print("SD");
            _hitbox.Disable();
            _hitboxEnabled = false;
        }

        _magazineEmpty = CurrentAmmo <= 0;
        _magazineFull = CurrentAmmo == G.magazineSize;

        if (Input.IsActionJustPressed(InputHash.REALOAD) && CanReload())
            _reloading = true;

        if (_reloading)
        {
            Reload(delta);
        }
        if (GetShootingPressed())
        {
            Shoot();
        }
        if (_hitboxEnabled && _hitboxTimer.TimeLeft <= 0) _hitbox.Disable();
    }
    private void Shoot()
    {
        _shooting = true;
        _hitbox.Enable();
        _hitboxEnabled = true;
        _shootingTime = 0;
        CurrentAmmo--;
        EmitSignal(SignalName.AmmoChanged, CurrentAmmo, CurrentMaxAmmo);
        if (_hitboxTimer.IsStopped()) _hitboxTimer.Start();
        _shooting = false;
    }
    private void Reload(double delta)
    {
        _reloadingTimer += (float)delta;
        if (_reloadingTimer >= G.reloadTime)
        {
            _reloading = false;
            int ammoNeeded = G.magazineSize - CurrentAmmo;
            int ammoToTake = Mathf.Min(ammoNeeded, CurrentMaxAmmo);
            CurrentAmmo += ammoToTake;
            CurrentMaxAmmo -= ammoToTake;
            _reloadingTimer = 0f;
            EmitSignal(SignalName.AmmoChanged, CurrentAmmo, CurrentMaxAmmo);
        }
    }
    private void CreateLineTracer()
    {
        var line = G.lineTracer.Instantiate<Node3D>();
        line.Rotation = new Vector3(Rotation.X, _parent.Rotation.Y, 0);
        line.Position = new Vector3(_parent.Position.X, _parent.Position.Y + 1f, _parent.Position.Z);
        GetTree().Root.AddChild(line);
    }
    private bool CanShoot()
    {
        return !_reloading && _shootingTime >= G.fireRate && !_magazineEmpty;
    }

    private bool CanReload()
    {
        return !_reloading && !_magazineFull && CurrentMaxAmmo > 0;
    }
    private bool GetShootingPressed()
    {
        return (G.auto ? Input.IsActionPressed(InputHash.SHOOT) : Input.IsActionJustPressed(InputHash.SHOOT)) && CanShoot();
    }
}
