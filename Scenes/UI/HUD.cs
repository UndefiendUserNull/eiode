using EIODE.Scenes;
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
    private WeaponConfig _hands = null;
    private bool _holdingHands = false;

    public override void _Ready()
    {
        _game = Game.GetGame(this);
        _hands = _game.FindWeapon("hands");

        _head = _game.GetPlayer().GetHead();
        _container = NodeUtils.GetChildWithName<VBoxContainer>("V", this);

        _head.AmmoChanged += Head_AmmoChanged;
        _head.StartedReloading += Head_StartedReloading;
        _head.EndedReloading += Head_EndedReloading;
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
        if (!_game.FirstLevelLoaded)
            return;

        _head.AmmoChanged -= Head_AmmoChanged;
        _head.StartedReloading -= Head_StartedReloading;
        _head.EndedReloading -= Head_EndedReloading;
        _head.WeaponChanged -= Head_WeaponChanged;
    }
    private void Head_WeaponChanged(WeaponConfig current)
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

    private void Head_AmmoChanged(WeaponConfig weapon)
    {
        RefreshWeapon(weapon);
    }

    private void RefreshWeapon(WeaponConfig weapon)
    {
        _holdingHands = weapon == _hands;

        if (_holdingHands)
        {
            _label_reloading.Hide();
            _label_ammo.Hide();
            _label_weaponName.Hide();
        }
        else
        {
            if (!(_label_ammo.Visible || _label_weaponName.Visible))
            {
                _label_ammo.Show();
                _label_weaponName.Show();
            }
        }

        int currentAmmo = _head.CurrentWeapon.CurrentAmmo;
        int currentMaxAmmo = _head.CurrentWeapon.CurrentMaxAmmo;

        _currentAmmo = currentAmmo;
        _currentMaxAmmo = currentMaxAmmo;

        _text_ammo = $"{_currentAmmo} / {_currentMaxAmmo}";

        _label_weaponName.Text = weapon.Name;
        _label_ammo.Text = _text_ammo;
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
