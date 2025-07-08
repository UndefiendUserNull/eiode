
using EIODE.Resources;

namespace EIODE.Scenes;

public interface IWeaponWithAmmo
{
    public WeaponAmmoData AmmoData { get; set; }
    public void ReloadPressed();
    public bool IsReloading();
}
