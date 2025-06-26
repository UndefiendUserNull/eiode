using EIODE.Utils;
using EIODE.Resources.Src;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace EIODE.Core.Console;

public partial class DevConsole : Control
{
    [Export] private int _currentHistoryIndex = -1;
    [Export] public DevConsoleSettings Settings { get; set; }

    private LineEdit _input = null!;
    private RichTextLabel _log = null!;
    private Game _game = null!;
    private AutoCompleter _completer = null!;
    private Panel _mainPanel = null;
    private Label _suggestionsLabel = null!;
    private ColorRect _suggestionsPanel = null!;
    private readonly List<string> _history = [];

    private List<string> _currentSuggestions = [];
    private int _currentSuggestionIndex = -1;
    private string _suggestionsLabelText = string.Empty;

    public bool IsShown { get; private set; } = false;

    public static DevConsole Instance { get; private set; }
    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _game = Game.GetGame(this);

        // This sucks, should define them in a better way soon
        _mainPanel = GetChild<Panel>(0);
        _log = _mainPanel.GetChild<RichTextLabel>(0);
        _suggestionsPanel = _mainPanel.GetChild<ColorRect>(1);
        _suggestionsLabel = _suggestionsPanel.GetChild<Label>(0);
        _input = GetChild<LineEdit>(1);

        _completer = new AutoCompleter(ConsoleCommandSystem.GetCommands().Keys);

        ConsoleCommandSystem.RegisterInstance(this);
        Hide();

        _input.TextSubmitted += OnTextSubmitted;
        _input.TextChanged += OnTextChanged;

        _suggestionsLabel.Text = string.Empty;
        _suggestionsPanel.Hide();

        _log.Clear();
        ApplyConsoleSettings();
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
            _history.Insert(0, command); // Newest at front
            _currentHistoryIndex = -1;
        }
        _input.Clear();
        _currentSuggestions.Clear();
        _currentSuggestionIndex = -1;
    }

    private void OnTextChanged(string text)
    {
        // Reset suggestions when text changes
        if (_input.Text.Trim() != string.Empty)
        {
            _suggestionsPanel.Show();
            _currentSuggestions = [.. _completer.GetSuggestions(text)];
            _currentSuggestionIndex = -1;
            _suggestionsLabelText = string.Empty;
            foreach (var suggestion in _currentSuggestions)
            {
                _suggestionsLabelText += suggestion + '\n';
            }
            _suggestionsLabel.Text = _suggestionsLabelText;
        }

        if (_currentSuggestions.Count <= 0) _suggestionsPanel.Hide();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputHash.TOGGLE_CONSOLE))
        {
            ToggleConsole();
        }

        if (!IsShown) return;

        if (_input.Text.Trim() == string.Empty) _suggestionsPanel.Hide();

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
            CycleAutoComplete();
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
        _currentSuggestions.Clear();
        _currentSuggestionIndex = -1;
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
            _currentHistoryIndex = 0;
        else if (_currentHistoryIndex >= _history.Count)
            _currentHistoryIndex = _history.Count - 1;

        _input.Text = _history[_currentHistoryIndex];
        _input.CaretColumn = _input.Text.Length;
    }

    private void CycleAutoComplete()
    {
        if (_currentSuggestions.Count == 0) return;

        _currentSuggestionIndex++;
        if (_currentSuggestionIndex >= _currentSuggestions.Count)
            _currentSuggestionIndex = 0;

        _input.Text = _currentSuggestions[_currentSuggestionIndex];
        _input.CaretColumn = _input.Text.Length;
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
        if (_log != null)
            _log.AppendText(line + "\n");
        else
            GD.Print(ExtractMessage(line));
    }



    [Obsolete("Use Log() instead.")]
    public void Print(string msg) => Info(msg);

    [Obsolete("Use Log() instead.")]
    public void PushErr(string msg) => Error(msg);

    public void Info(string msg) => Log(msg, LogLevel.INFO);
    public void Warn(string msg) => Log(msg, LogLevel.WARNING);
    public void Error(string msg) => Log(msg, LogLevel.ERROR);


    /// <summary>
    /// Extracts the {msg} part from a string formatted like "[color=green][INFO] : {msg}[/color]"
    /// </summary>
    public static string ExtractMessage(string formattedMessage)
    {
        // Remove color tags (e.g., [color=green] and [/color])
        string withoutColor = ColorRegex().Replace(formattedMessage, "");

        // Extract the part after "[LEVEL] : " (where LEVEL = INFO, WARNING, etc.)
        var match = LogLevelRegex().Match(withoutColor);

        return match.Success ? match.Groups[1].Value.Trim() : formattedMessage;
    }

    public enum LogLevel
    {
        INFO,
        WARNING,
        ERROR,
        BLANK
    }

    [ConsoleCommand("apply_console_settings", "Applies current console settings if changed.")]

    private void ApplyConsoleSettings()
    {
        _log.PopAll();
        _log.PushFontSize(Settings.FontSize);
        _log.PushOutlineSize(Settings.OutlineSize);
        _log.PushOutlineColor(Settings.OutlineColor);
    }

    #region CC

    [ConsoleCommand("help", "Shows all available console commands.")]
    public void Cc_Help()
    {
        foreach (var cmd in ConsoleCommandSystem.GetCommands())
        {
            Log($"{cmd.Key}: {cmd.Value.Description}");
        }
    }

    [ConsoleCommand("clear", "Clears the entire log")]
    public void Cc_Clear()
    {
        _log.Clear();
        ApplyConsoleSettings();
    }

    [ConsoleCommand("quit", "Quits the game")]
    public void Cc_Quit()
    {
        GetTree().Quit();
    }

    [ConsoleCommand("console_set", "Changes a console setting value (fontsize (int), outlinesize (int))")]
    public void Cc_ConsoleSet(string arg, int value)
    {
        int prevValue = 0;
        switch (arg)
        {
            case "fontsize":
                prevValue = Settings.FontSize;
                Settings.FontSize = value;
                break;
            case "outlinesize":
                prevValue = Settings.OutlineSize;
                Settings.OutlineSize = value;
                break;
            default:
                break;
        }
        ApplyConsoleSettings();
        Log($"Changed {arg} from {prevValue} to {value}");
    }
    #endregion

    #region Regex

    [GeneratedRegex(@"\[color=[^\]]+\]|\[\/color\]", RegexOptions.Compiled)]
    public static partial Regex ColorRegex();

    [GeneratedRegex(@"^\[[A-Z]+\]\s*:\s*(.*)$", RegexOptions.Compiled)]
    public static partial Regex LogLevelRegex();

    #endregion




}