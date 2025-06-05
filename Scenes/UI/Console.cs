using Godot;
using EIODE.Core.Console;
using EIODE.Utils;
using EIODE.Scripts.Core;

namespace EIODE.Scenes.UI;
public partial class Console : Control
{
    private LineEdit _input = null;
    private bool _isShown = false;
    private Game _game;
    public override void _Ready()
    {
        _input = GetChild<LineEdit>(0);
        _input.Hide();
        _game = Game.GetGame(this);
        _input.TextSubmitted += Input_TextSubmitted;

    }

    private void Input_TextSubmitted(string newText)
    {
        if (_isShown)
        {
            string command = _input.Text.Trim('\n');
            if (!string.IsNullOrEmpty(command))
            {
                GD.Print($"Executing: {command}");
                ConsoleCommandSystem.ExecuteCommand(command);
            }
            _input.Clear();
        }
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