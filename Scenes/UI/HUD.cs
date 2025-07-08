using EIODE.Core;
using EIODE.Resources;
using EIODE.Utils;
using EIODE.Core.Console;
using Godot;
using EIODE.Components;

namespace EIODE.Scenes.UI;
public partial class HUD : Control
{
    private Head _head = null;
    private VBoxContainer _Vcontainer = null;
    private HBoxContainer _Hcontainer = null;
    private Label _label_ammo = null;
    private Label _label_weaponName = null;
    private Label _label_reloading = null;
    private int _currentAmmo = 0;
    private int _currentMaxAmmo = 0;
    private string _text_ammo = string.Empty;
    private Game _game = null;
    private ProgressBar _progressBar_chargeable = null;
    private bool _holdingMelee = false;
    private bool _holdingChargeable = false;
    private ChargeableComponent _chargeable = null;
    public override void _Ready()
    {
        _game = Game.GetGame(this);

        _head = _game.GetPlayer().GetHead();
        _Vcontainer = NodeUtils.GetChildWithName<VBoxContainer>("V", this);
        _Hcontainer = NodeUtils.GetChildWithName<HBoxContainer>("H", _Vcontainer);

        _head.AmmoChanged += Head_AmmoChanged;
        _head.WeaponChanged += Head_WeaponChanged;

        _label_reloading = NodeUtils.GetChildWithName<Label>("l_reloading", _Vcontainer);
        _progressBar_chargeable = NodeUtils.GetChildWithName<ProgressBar>("prog_chargeable", _Vcontainer);
        _label_ammo = NodeUtils.GetChildWithName<Label>("l_ammo", _Hcontainer);
        _label_weaponName = NodeUtils.GetChildWithName<Label>("l_weaponName", _Hcontainer);

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
    private void ApplyAmmo(WeaponAmmoData weaponWithAmmo)
    {
        _currentAmmo = weaponWithAmmo.CurrentAmmo;
        _currentMaxAmmo = weaponWithAmmo.CurrentMaxAmmo;
        _text_ammo = $"{_currentAmmo} / {_currentMaxAmmo}";
        _label_ammo.Text = _text_ammo;
    }
    private void Head_WeaponChanged(WeaponBase current)
    {
        if (NodeUtils.GetChildWithNodeType<ChargeableComponent>(current) == null)
        {
            _chargeable = null;
            _progressBar_chargeable.Hide();
        }
    }

    private void Head_AmmoChanged(WeaponBase weapon)
    {
        //RefreshWeapon(weapon);
    }

    public override void _Process(double delta)
    {
        if (_head.CurrentWeapon.GetWeaponType() == WeaponType.MELEE)
        {
            RefreshWeapon(_head.CurrentWeapon);
            return;
        }

        if (_head.CurrentWeapon is IWeaponWithAmmo weaponWithAmmo)
        {
            _label_reloading.Visible = weaponWithAmmo.IsReloading();
            RefreshWeapon(_head.CurrentWeapon);
            ApplyAmmo(_head.CurrentWeapon.GetWeaponAmmoData());
        }
    }

    private void RefreshWeapon(WeaponBase weapon)
    {
        if (weapon == null) return;

        _label_weaponName.Text = weapon.GetWeaponName().Capitalize();

        if (_head.CurrentWeapon.GetWeaponType() == WeaponType.MELEE)
        {
            _label_reloading.Hide();
            _label_ammo.Text = string.Empty;
            _label_weaponName.Text = string.Empty;
            _label_ammo.Hide();
            _label_weaponName.Hide();
            _progressBar_chargeable.Hide();
            return;
        }

        // Makes sure weapon name is visible
        if (!_label_weaponName.Visible)
        {
            _label_weaponName.Show();
        }

        // Holding weapon that has ammo
        if (weapon is IWeaponWithAmmo weaponWithAmmo)
        {
            _label_ammo.Show();
        }

        // Holding chargeable weapon
        if (_chargeable == null)
            _holdingChargeable = NodeUtils.GetChildWithNodeType<ChargeableComponent>(weapon, out _chargeable);

        if (_chargeable != null)
        {
            //if (_progressBar_chargeable.MaxValue == _chargeable.FullChargeDuration) return;
            _progressBar_chargeable.Show();
            _progressBar_chargeable.MinValue = 0;
            _progressBar_chargeable.MaxValue = _chargeable.FullChargeDuration;
            _progressBar_chargeable.Value = Mathf.Lerp(_progressBar_chargeable.Value, _chargeable.CurrentCharge, 0.5f);
        }
        else
        {
            _progressBar_chargeable.Hide();
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
