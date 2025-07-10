namespace EIODE.Scenes;

public partial class WeaponPistol : RaycastWeaponBase
{
    public override bool IsAttacking()
    {
        return Hitbox.Enabled;
    }
}
