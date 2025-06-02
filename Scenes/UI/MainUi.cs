using EIODE.Scenes.Player;
using EIODE.Scripts.Core;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.UI;
public partial class MainUi : Control
{
    private Head _head = null;
    private Label _label_ammo = null;
    private int _currentAmmo;
    private int _currentMaxAmmo;
    private string _text_ammo;
    public override void _Ready()
    {
        _head = GetNode<Game>(Game.Location).GetPlayer().GetHead();
        _label_ammo = NodeUtils.GetChildWithNodeType<Label>(this);
        _head.AmmoChanged += Head_AmmoChanged;
        _currentAmmo = _head.CurrentAmmo;
    }

    private void Head_AmmoChanged(int currentAmmo, int currentMaxAmmo)
    {
        _currentAmmo = currentAmmo;
        _currentMaxAmmo = currentMaxAmmo;
        _text_ammo = $"{_currentAmmo} / {_currentMaxAmmo}";
        _label_ammo.Text = _text_ammo;
    }
}
