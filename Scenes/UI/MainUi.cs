using EIODE.Scenes.Player;
using EIODE.Scripts.Core;
using EIODE.Resources.Src;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.UI;
public partial class MainUi : Control
{
    private Head _head = null;
    private VBoxContainer _container = null;
    private Label _label_ammo = null;
    private Label _label_reloading = null;
    private int _currentAmmo = 0;
    private int _currentMaxAmmo = 0;
    private string _text_ammo = string.Empty;
    public override void _Ready()
    {
        _container = NodeUtils.GetChildWithName<VBoxContainer>("V", this);
        _head = GetNode<Game>(Game.Location).GetPlayer().GetHead();

        _head.AmmoChanged += Head_AmmoChanged;
        _head.StartedReloading += Head_StartedReloading;
        _head.EndedReloading += Head_EndedReloading;
        _head.GunSettingsChanged += Head_GunSettingsChanged;

        _label_reloading = _container.GetChild<Label>(0);
        _label_ammo = _container.GetChild<Label>(1);

        _currentAmmo = _head.CurrentAmmo;
        RefreshAmmo(_head.CurrentAmmo, _head.CurrentMaxAmmo);
        _label_reloading.Hide();
    }

    private void Head_GunSettingsChanged(Gun previous, Gun current)
    {
        throw new System.NotImplementedException();
    }

    private void Head_EndedReloading()
    {
        _label_reloading.Hide();
    }

    private void Head_StartedReloading()
    {
        _label_reloading.Show();
    }

    private void Head_AmmoChanged(int currentAmmo, int currentMaxAmmo)
    {
        RefreshAmmo(currentAmmo, currentMaxAmmo);
    }

    private void RefreshAmmo(int currentAmmo, int currentMaxAmmo)
    {
        _currentAmmo = currentAmmo;
        _currentMaxAmmo = currentMaxAmmo;
        _text_ammo = $"{_currentAmmo} / {_currentMaxAmmo}";
        _label_ammo.Text = _text_ammo;
    }
}
