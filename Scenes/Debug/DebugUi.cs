using EIODE.Scenes.Player;
using EIODE.Scripts.Core;
using Godot;
using System.Text;

namespace EIODE.Scenes.Debug;

public partial class DebugUi : Control
{
    private PlayerMovement _player;
    private Head _playerHead;
    private Label _label;
    private readonly StringBuilder _sb = new();

    public override void _Ready()
    {
        _player = GetNode<Game>(Game.Location).GetPlayer();
        _playerHead = _player.GetHead();
        _label = GetChild<Label>(0);
    }

    public override void _Process(double delta)
    {
        _playerHead ??= _player.GetHead();
        _sb.Clear();
        _sb.Append($"FPS : {Engine.GetFramesPerSecond()}\n");
        _sb.Append($"Delta : {delta}\n");
        _sb.Append($"Time In Air : {_player._timeInAir}\n");
        _sb.Append($"Time Since Last Jump : {_player._timeSinceLastJumpInput}\n");
        _sb.Append($"Shooting Time : {_playerHead._shootingTime}\n");
        _sb.Append($"Current Fire Rate : {_playerHead.G.fireRate}\n");
        _sb.Append($"Current Ammo : {_playerHead._reloading}\n");
        _sb.Append($"Magazine Size : {_playerHead.G.magazineSize}\n");
        _sb.Append($"Magazine Empty : {_playerHead._magazineEmpty}\n");
        _sb.Append($"Current Ammo : {_playerHead.CurrentAmmo}\n");
        _sb.Append($"Reloading Timer : {_playerHead._reloadingTimer}\n");
        _sb.Append($"Current Max Ammo : {_playerHead.CurrentMaxAmmo}\n");
        _sb.Append($"Shooting : {_playerHead._shooting}\n");
        _sb.Append($"Hitbox Enabled : {_playerHead._hitboxEnabled}\n");
        _label.Text = _sb.ToString();
    }
}
