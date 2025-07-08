using EIODE.Core;
using EIODE.Resources;
using EIODE.Utils;
using EIODE.Core.Console;
using Godot;

namespace EIODE.Scenes.UI;
public partial class HUD : Control
{
    private Head _head = null;
    private VBoxContainer _container = null;
    private Label _label_ammo = null;
    private Label _label_weaponName = null;
    private Label _label_reloading = null;
    private int _currentAmmo = 0;
    private int _currentMaxAmmo = 0;
    private Game _game = null;
    private string _text_ammo = string.Empty;
    private bool _holdingMelee = false;
    private WeaponAmmoData _currentAmmoData = null;
    public override void _Ready()
    {
        _game = Game.GetGame(this);

        _head = _game.GetPlayer().GetHead();
        _container = NodeUtils.GetChildWithName<VBoxContainer>("V", this);

        _head.AmmoChanged += Head_AmmoChanged;
        _head.WeaponChanged += Head_WeaponChanged;

        _label_reloading = NodeUtils.GetChildWithName<Label>("l_reloading", _container);
        _label_ammo = NodeUtils.GetChildWithName<Label>("l_ammo", _container);
        _label_weaponName = NodeUtils.GetChildWithName<Label>("l_weaponName", _container);

        RefreshWeapon(_head.CurrentWeapon);
        _label_reloading.Hide();

        ConsoleCommandSystem.RegisterInstance(this);
    }

    public override void _ExitTree()
    {
        // Check if the game loaded first level first before unsubscribing, this prevents unsubscribing twice
        if (!_game.FirstLevelLoaded)
            return;

        _head.AmmoChanged -= Head_AmmoChanged;
        _head.WeaponChanged -= Head_WeaponChanged;
    }
    private void Head_WeaponChanged(WeaponBase current)
    {
        RefreshWeapon(current);
    }

    private void Head_EndedReloading()
    {
        _label_reloading.Hide();
    }

    private void Head_StartedReloading()
    {
        _label_reloading.Show();
    }

    private void Head_AmmoChanged(WeaponBase weapon)
    {
        RefreshWeapon(weapon);
    }

    private void RefreshWeapon(WeaponBase weapon)
    {
        if (weapon == null) return;

        _holdingMelee = weapon.GetWeaponType() == WeaponType.MELEE;
        _label_weaponName.Text = weapon.GetWeaponName().Capitalize();

        if (_holdingMelee)
        {
            _label_reloading.Hide();
            _label_ammo.Hide();
            _label_weaponName.Hide();
            return;
        }
        else
        {
            if (!(_label_ammo.Visible || _label_weaponName.Visible))
            {
                _label_ammo.Show();
                _label_weaponName.Show();
            }
        }

        if (weapon is IWeaponWithAmmo weaponWithAmmo)
        {
            _currentAmmoData = weaponWithAmmo.AmmoData;

            _currentAmmo = _currentAmmoData.CurrentAmmo;
            _currentMaxAmmo = _currentAmmoData.CurrentMaxAmmo;

            _text_ammo = $"{_currentAmmo} / {_currentMaxAmmo}";

            _label_ammo.Text = _text_ammo;
        }
    }

    #region CC

    [ConsoleCommand("draw_hud", "Weather to draw the hud or not (0 | 1)")]
    public void Cc_DrawHud(int i = 1)
    {
        if (i == 0) Hide();
        else if (i == 1) Show();
    }

    #endregion
}
