using EIODE.Scenes.Player;
using EIODE.Core;
using EIODE.Utils;
using Godot;
using System.Text;

namespace EIODE.Scenes.Debug;

public partial class DebugUi : Control
{
    private Player.Player _player = null;
    private Head _playerHead = null;
    private Label _label = null;
    private bool _isShown = false;
    private readonly StringBuilder _sb = new();

    public override void _Ready()
    {
        _player = Game.GetGame(this).GetPlayer();
        _playerHead = _player.GetHead();
        _label = GetChild<Label>(0);
        DisableUI();
    }
    public override void _Process(double delta)
    {
        _playerHead ??= _player.GetHead();

        if (Input.IsActionJustPressed(InputHash.D_TOGGLE_DEBUG_UI))
        {
            _isShown = !_isShown;
            if (_isShown) Show();
            else Hide();
        }

        if (!_isShown) return;

        _sb.Clear();
        _sb.Append($"FPS : {Engine.GetFramesPerSecond()}\n");
        _sb.Append($"Delta : {delta}\n");
        _sb.Append($"Player Position : {_player.Position}\n");
        _sb.Append($"Direction : {_player._direction}\n");
        _sb.Append($"Velocity : {_player.Velocity.Length()}\n");
        _sb.Append($"YVelocity : {_player.Velocity.Y}\n");
        _sb.Append($"Variable Gravity : {_player._variableGravity}\n");
        _sb.Append($"Lunch Pad Force : {_player.JumpPadForce}\n");
        _sb.Append($"Time In Air : {_player._timeInAir}\n");
        _sb.Append($"PrevJumpPads.Count : {_player.PrevJumpPads.Count}\n");
        _sb.Append($"Time Since Last Jump : {_player._timeSinceLastJumpInput}\n");
        _sb.Append($"Shooting Time : {_playerHead._shootingTime}\n");
        _sb.Append($"Current Fire Rate : {_playerHead.W.fireRate}\n");
        _sb.Append($"Current Ammo : {_playerHead._reloading}\n");
        _sb.Append($"Magazine Size : {_playerHead.W.magazineSize}\n");
        _sb.Append($"Magazine Empty : {_playerHead._magazineEmpty}\n");
        _sb.Append($"Current Ammo : {_playerHead.CurrentAmmo}\n");
        _sb.Append($"Reloading Timer : {_playerHead._reloadingTimer}\n");
        _sb.Append($"Current Max Ammo : {_playerHead.CurrentMaxAmmo}\n");
        _sb.Append($"Shooting : {_playerHead._shooting}\n");
        _sb.Append($"Hitbox Enabled : {_playerHead._hitboxEnabled}\n");
        _label.Text = _sb.ToString();
    }

    public void EnableUI()
    {
        Show();
        _isShown = true;
        ProcessMode = ProcessModeEnum.Inherit;
        _sb.Clear();
    }

    public void DisableUI()
    {
        Hide();
        _isShown = false;
        _sb.Clear();
    }
}
