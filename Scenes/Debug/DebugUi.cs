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
        //_playerMovement = _player.GetScript().As<PlayerMovement>();
    }

    public override void _Process(double delta)
    {
        _sb.Clear();
        _sb.Append($"Delta : {delta}\n");
        _sb.Append($"Time In Air : {_player._timeInAir}\n");
        _sb.Append($"Time Since Last Jump : {_player._timeSinceLastJumpInput}\n");
        _label.Text = _sb.ToString();
    }
}
