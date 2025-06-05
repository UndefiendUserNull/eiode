using Godot;
using EIODE.Core.Console;
using EIODE.Utils;
using EIODE.Scripts.Core;

namespace EIODE.Scenes.UI;
public partial class Console : Control
{
    private TextEdit _input = null;
    private bool _isShown = false;
    private Game _game;
    public override void _Ready()
    {
        _input = GetChild<TextEdit>(0);
        _input.Hide();
        _game = Game.GetGame(this);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputHash.OPEN_CONSOLE))
        {
            _isShown = !_isShown;

            if (_isShown)
            {
                ShowConsole();
                _input.Clear();
                _input.GrabFocus();
            }
            else
            {
                HideConsole();
                _input.Text = string.Empty;
            }
        }

        if (_isShown)
        {
            if (Input.IsActionJustPressed(InputHash.K_ENTER))
            {
                string command = _input.Text.Trim();
                if (!string.IsNullOrEmpty(command))
                {
                    GD.Print($"Executing: {command}");
                    ConsoleCommandSystem.ExecuteCommand(command);
                }
                _input.Text = string.Empty;
                _input.GrabFocus();
            }
        }
    }
    private void ShowConsole()
    {
        _input.Show();
        _input.GrabFocus();
        _game.GetPlayer().Lock();
        Game.ShowMouse();
    }

    private void HideConsole()
    {
        _input.Hide();
        _input.ReleaseFocus();
        _game.GetPlayer().UnLock();
        Game.HideMouse();
    }
}