using EIODE.Resources.Src;
using EIODE.Utils;
using Godot;
using System.Threading.Tasks;

namespace EIODE.Scenes.Player;
public partial class Head : Node3D
{
    [Export] public float ShootingRayLength { get; set; } = -1000;
    [Export] private Gun _currentGunSettings = null;

    private bool _shooting = false;
    public bool _reloading = false;
    // used in special cases
    private bool _magazineFull = false;
    public bool _magazineEmpty = false;
    public float _shootingTime = 0.0f;
    public int _currentAmmo = 0;
    public int _currentMaxAmmo = 0;
    public float _reloadingTimer = 0f;
    private RayCast3D _shootingRay = null;
    private Node3D _parent = null;
    public Gun G => _currentGunSettings;

    public override void _Ready()
    {
        if (_currentGunSettings == null) GD.PushError("No gun settings was given to player");
        _shootingRay = GetChild<RayCast3D>(1);
        _shootingRay.TargetPosition = new Vector3(0, 0, ShootingRayLength);
        _shootingRay.Enabled = false;
        _currentAmmo = G.magazineSize;
        _currentMaxAmmo = G.maxAmmo;
        _parent = GetParent<Node3D>();
    }

    public override void _Process(double delta)
    {
        HandleShootingAsync(delta);
    }


    private void HandleShootingAsync(double delta)
    {
        if (!_shooting && _shootingTime <= G.fireRate)
        {
            _shootingTime += (float)delta;
        }

        _shooting = (G.auto ? Input.IsActionPressed(InputHash.SHOOT) : Input.IsActionJustPressed(InputHash.SHOOT)) && CanShoot();

        _magazineEmpty = _currentAmmo <= 0;
        _magazineFull = _currentAmmo == G.magazineSize;

        if (Input.IsActionJustPressed(InputHash.REALOAD) && CanReload())
            _reloading = true;

        // Reload
        if (_reloading)
        {
            _reloadingTimer += (float)delta;
            if (_reloadingTimer >= G.reloadTime)
            {
                _reloading = false;
                int ammoNeeded = G.magazineSize - _currentAmmo;
                int ammoToTake = Mathf.Min(ammoNeeded, _currentMaxAmmo);
                _currentAmmo += ammoToTake;
                _currentMaxAmmo -= ammoToTake;
                _reloadingTimer = 0f;
            }
        }
        if (_shooting)
        {
            _shootingRay.Enabled = true;
            var line = G.lineTracer.Instantiate<Node3D>();
            line.Rotation = new Vector3(Rotation.X, _parent.Rotation.Y, 0);
            line.Position = new Vector3(_parent.Position.X, _parent.Position.Y + 1.5f, _parent.Position.Z);
            GetTree().Root.AddChild(line);
            _shootingTime = 0;
            _currentAmmo--;
            _shootingRay.Enabled = false;
        }
    }
    private bool CanShoot()
    {
        return !_reloading && _shootingTime >= G.fireRate && !_magazineEmpty;
    }

    private bool CanReload()
    {
        return !_reloading && !_magazineFull && _currentMaxAmmo > 0;
    }
}
