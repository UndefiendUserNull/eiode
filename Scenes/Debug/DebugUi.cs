using EIODE.Scenes.Player;
using Godot;
using System.Text;

namespace EIODE.Scenes.Debug;

public partial class DebugUi : Control
{
    [Export] private PlayerMovement _player;
    private PlayerMovement _playerMovement;
    private Label _label;
    private readonly StringBuilder _sb = new();

    public override void _Ready()
    {
        _label = GetChild<Label>(0);
    }

    public override void _Process(double delta)
    {
        _sb.Clear();
        _sb.Append($"FPS : {Engine.GetFramesPerSecond()}\n");
        _sb.Append($"Delta : {delta}\n");
        _sb.Append($"Time In Air : {_player._timeInAir}\n");
        _sb.Append($"Time Since Last Jump : {_player._timeSinceLastJumpInput}\n");
        _sb.Append($"Shooting Time : {_player._headSrc._shootingTime}\n");
        _sb.Append($"Current Fire Rate : {_player._headSrc.G.fireRate}\n");
        _sb.Append($"Current Ammo : {_player._headSrc._reloading}\n");
        _sb.Append($"Magazine Size : {_player._headSrc.G.magazineSize}\n");
        _sb.Append($"Magazine Empty : {_player._headSrc._magazineEmpty}\n");
        _sb.Append($"Current Ammo : {_player._headSrc._currentAmmo}\n");
        _sb.Append($"Reloading Timer : {_player._headSrc._reloadingTimer}\n");
        _sb.Append($"Current Max Ammo : {_player._headSrc._currentMaxAmmo}\n");
        _label.Text = _sb.ToString();
    }
}
