using Godot;

namespace EIODE.Resources.Src;
[GlobalClass]
public partial class Gun : Resource
{
    [Export] public int damagePerBullet = 10;
    [Export] public float fireRate = 0.5f;
    [Export] public int maxAmmo = 100;
    [Export] public int magazineSize = 25;
    // In seconds
    [Export] public float HitboxDuration = 0.03f;
    // In seconds
    [Export] public float reloadTime = 2f;
    [Export] public bool auto = false;
    [Export] public PackedScene lineTracer = null;
    public Gun() { }
}
