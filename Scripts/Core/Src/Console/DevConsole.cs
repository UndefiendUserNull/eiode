using Godot;
using EIODE.Utils;
using EIODE.Scripts.Core;
using System.Text;
using System;
using System.Collections.Generic;

namespace EIODE.Core.Console;
public partial class DevConsole : Control
{
    [Export] private int _currentHistoryIndex = 0;
    private LineEdit _input = null;
    private RichTextLabel _log = null;
    private bool _isShown = false;
    private Game _game = null;
    private AutoCompleter completer = null;
    //private readonly StringBuilder _sb = new();
    private readonly List<string> _history = [];

    public override void _Ready()
    {
        completer = new(ConsoleCommandSystem.GetCommands().Keys);
        _input = GetChild<LineEdit>(1);
        _log = GetChild<Panel>(0).GetChild<RichTextLabel>(0);
        _game = Game.GetGame(this);
        ConsoleCommandSystem.RegisterInstance(this);
        this.Hide();
        _input.TextSubmitted += Input_TextSubmitted;
        _input.TextChanged += Input_TextChanged;
        _log.Clear();
        _log.PushFontSize(14);
        _log.PushOutlineSize(8);
        _log.PushOutlineColor(Color.Color8(0, 0, 0));
        //_log.AppendText(_sb.ToString());
    }

    private void Input_TextChanged(string newText)
    {
        foreach (var suggestion in completer.GetSuggestions(newText))
        {
        }
    }

    public override void _ExitTree()
    {
        _input.TextSubmitted -= Input_TextSubmitted;
        _input.TextChanged -= Input_TextChanged;
    }

    private void Input_TextSubmitted(string newText)
    {
        if (_isShown)
        {
            string command = _input.Text.Trim('\n');
            if (!string.IsNullOrEmpty(command))
            {
                _game.Console.Log(command, LogLevel.BLANK);
                ConsoleCommandSystem.ExecuteCommand(command);
                _history.Add(command);
            }
            _input.Clear();
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputHash.TOGGLE_CONSOLE))
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
        if (_isShown)
        {
            if (Input.IsActionJustPressed(InputHash.UP))
            {
                if (_currentHistoryIndex + 1 < _history.Count)
                    _currentHistoryIndex++;
                else
                    _currentHistoryIndex = 0;

                _input.Text = _history[_currentHistoryIndex];
            }
            if (Input.IsActionJustPressed(InputHash.DOWN))
            {
                if (_currentHistoryIndex - 1 > 0)
                    _currentHistoryIndex--;
                else
                    _currentHistoryIndex = _history.Count - 1;

                if (_currentHistoryIndex >= 0)
                    _input.Text = _history[_currentHistoryIndex];
            }
            if (Input.IsActionJustPressed(InputHash.K_TAB))
            {
                var suggestions = completer.GetSuggestions(_input.Text);

                if (suggestions.Count > 0)
                    _input.Text = suggestions[0];
            }
        }
    }

    private void ShowConsole()
    {
        this.Show();
        _input.GrabFocus();
        _game.GetPlayer().Lock();
        Game.ShowMouse();
    }

    private void HideConsole()
    {
        this.Hide();
        _input.ReleaseFocus();
        _game.GetPlayer().UnLock();
        Game.HideMouse();
    }

    public void Log(string msg, LogLevel logLevel = LogLevel.INFO)
    {
        switch (logLevel)
        {
            case LogLevel.INFO:
                Info(msg);
                break;
            case LogLevel.WARNING:
                Warn(msg);
                break;
            case LogLevel.ERROR:
                Error(msg);
                break;
            case LogLevel.BLANK:
                BlankLog(msg);
                break;
        }
    }

    // Too lazy to change all the GD.Print to a Log one, DON'T USE THIS, Use Log instead
    [Obsolete]
    public void Print(string msg)
    {
        Info(msg);
    }

    // Too lazy to change all the GD.PushErr to a Log one, DON'T USE THIS, Use Log instead
    [Obsolete]
    public void PushErr(string msg)
    {
        Error(msg);
    }
    public enum LogLevel
    {
        INFO,
        WARNING,
        ERROR,
        BLANK
    }
    public void BlankLog(string msg)
    {
        var line = $"{msg}";
        AddLogLine(line);
    }
    public void Info(string msg)
    {
        var line = $"[color=green][INFO] : {msg}[/color]";
        AddLogLine(line);

    }
    public void Warn(string msg)
    {
        var line = $"[color=yellow][WARNING] : {msg}[/color]";
        AddLogLine(line);
    }

    public void Error(string msg)
    {
        var line = $"[color=red][ERROR] : {msg}[/color]";
        AddLogLine(line);
    }
    private void AddLogLine(string line)
    {
        _log?.AppendText(line + '\n');
    }

    [ConsoleCommand("help", "Helps you :D")]
    public void Help()
    {
        foreach (var cmd in ConsoleCommandSystem.GetCommands())
        {
            Log($"{cmd.Key}: {cmd.Value.Description}");
        }
    }
}