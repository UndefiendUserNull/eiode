using Godot;
using EIODE.Utils;
using EIODE.Scripts.Core;
using System;
using System.Collections.Generic;

namespace EIODE.Core.Console;
public partial class DevConsole : Control
{
    [Export] private int _currentHistoryIndex = -1;
    private LineEdit _input = null!;
    private RichTextLabel _log = null!;
    private Game _game = null!;
    private AutoCompleter _completer = null!;
    private readonly List<string> _history = [];

    public bool IsShown { get; private set; } = false;

    public override void _Ready()
    {
        _game = Game.GetGame(this);
        if (!_game.InitSpawnConsole)
        {
            QueueFree();
            return;
        }

        _input = GetNode<LineEdit>("Input");
        _log = GetNode<RichTextLabel>("Panel/Logger");

        _completer = new AutoCompleter(ConsoleCommandSystem.GetCommands().Keys);

        ConsoleCommandSystem.RegisterInstance(this);
        Hide();

        _input.TextSubmitted += OnTextSubmitted;
        _input.TextChanged += OnTextChanged;

        _log.Clear();
        _log.PushFontSize(14);
        _log.PushOutlineSize(8);
        _log.PushOutlineColor(Color.Color8(0, 0, 0));
    }

    public override void _ExitTree()
    {
        _input.TextSubmitted -= OnTextSubmitted;
        _input.TextChanged -= OnTextChanged;
    }

    private void OnTextSubmitted(string text)
    {
        if (!IsShown) return;

        string command = text.Trim();
        if (!string.IsNullOrEmpty(command))
        {
            Log(command, LogLevel.BLANK);
            ConsoleCommandSystem.ExecuteCommand(command);
            _history.Insert(0, command); // Insert at front for easy cycling
            _currentHistoryIndex = -1;
        }
        _input.Clear();
    }

    private void OnTextChanged(string text)
    {
        // TODO: Show auto-complete UI
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputHash.TOGGLE_CONSOLE))
        {
            ToggleConsole();
        }

        if (!IsShown) return;

        if (Input.IsActionJustPressed(InputHash.UP))
        {
            CycleHistory(1);
        }
        else if (Input.IsActionJustPressed(InputHash.DOWN))
        {
            CycleHistory(-1);
        }
        else if (Input.IsActionJustPressed(InputHash.K_TAB))
        {
            AutoComplete();
        }
    }

    private void ToggleConsole()
    {
        IsShown = !IsShown;

        if (IsShown)
        {
            ShowConsole();
        }
        else
        {
            HideConsole();
        }
    }

    private void ShowConsole()
    {
        Show();
        _input.Clear();
        _input.GrabFocus();
        _currentHistoryIndex = -1;
        _game.GetPlayer().Lock();
        Game.ShowMouse();
    }

    private void HideConsole()
    {
        Hide();
        _input.ReleaseFocus();
        _game.GetPlayer().UnLock();
        Game.HideMouse();
    }

    private void CycleHistory(int direction)
    {
        if (_history.Count == 0) return;

        _currentHistoryIndex += direction;

        if (_currentHistoryIndex < 0)
        {
            _currentHistoryIndex = 0;
        }
        else if (_currentHistoryIndex >= _history.Count)
        {
            _currentHistoryIndex = _history.Count - 1;
        }

        _input.Text = _history[_currentHistoryIndex];
        _input.CaretColumn = _input.Text.Length;
    }

    private void AutoComplete()
    {
        var suggestions = _completer.GetSuggestions(_input.Text);
        if (suggestions.Count > 0)
        {
            _input.Text = suggestions[0];
            _input.CaretColumn = _input.Text.Length;
        }
    }

    public void Log(string msg, LogLevel logLevel = LogLevel.INFO)
    {
        string line = logLevel switch
        {
            LogLevel.INFO => $"[color=green][INFO] : {msg}[/color]",
            LogLevel.WARNING => $"[color=yellow][WARNING] : {msg}[/color]",
            LogLevel.ERROR => $"[color=red][ERROR] : {msg}[/color]",
            LogLevel.BLANK => msg,
            _ => msg
        };

        AddLogLine(line);
    }

    private void AddLogLine(string line)
    {
        _log.AppendText(line + "\n");
    }

    [ConsoleCommand("help", "Shows all available console commands.")]
    public void Help()
    {
        foreach (var cmd in ConsoleCommandSystem.GetCommands())
        {
            Log($"{cmd.Key}: {cmd.Value.Description}");
        }
    }

    [Obsolete("Use Log() instead.")]
    public void Print(string msg) => Info(msg);

    [Obsolete("Use Log() instead.")]
    public void PushErr(string msg) => Error(msg);

    public void Info(string msg) => Log(msg, LogLevel.INFO);
    public void Warn(string msg) => Log(msg, LogLevel.WARNING);
    public void Error(string msg) => Log(msg, LogLevel.ERROR);

    public enum LogLevel
    {
        INFO,
        WARNING,
        ERROR,
        BLANK
    }
}
