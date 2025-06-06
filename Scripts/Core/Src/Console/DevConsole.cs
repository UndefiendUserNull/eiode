using Godot;
using EIODE.Utils;
using EIODE.Scripts.Core;
using System.Text;
using System;
using System.Collections.Generic;

namespace EIODE.Core.Console;
public partial class DevConsole : Control
{
    private LineEdit _input = null;
    private RichTextLabel _log = null;
    private bool _isShown = false;
    private Game _game = null;
    private StringBuilder _sb = new();
    private List<string> _history = [];
    private int _currentHistoryIndex = -1;
    public override void _Ready()
    {
        _input = GetChild<LineEdit>(1);
        _log = GetChild<Panel>(0).GetChild<RichTextLabel>(0);
        _game = Game.GetGame(this);
        _input.TextSubmitted += Input_TextSubmitted;
        _log.Clear();
        ConsoleCommandSystem.RegisterInstance(this);
        this.Hide();
        _log.Text = _sb.ToString();
    }

    public override void _ExitTree()
    {
        _input.TextSubmitted -= Input_TextSubmitted;
    }

    private void Input_TextSubmitted(string newText)
    {
        if (_isShown)
        {
            string command = _input.Text.Trim('\n');
            if (!string.IsNullOrEmpty(command))
            {
                _game.Console.Print($"Executing: {command}");
                ConsoleCommandSystem.ExecuteCommand(command);
                _history.Add(command);
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
        if (_isShown)
        {
            if (Input.IsActionJustPressed(InputHash.UP))
            {
                if (_currentHistoryIndex <= _history.Count)
                    _currentHistoryIndex++;
                else
                    _currentHistoryIndex = 0;

                _input.Text = _history[_currentHistoryIndex];
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
        }
        if (_log != null && _sb != null)
            _log.Text = _sb.ToString();
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
        ERROR
    }

    public void Info(string msg)
    {
        _sb.AppendLine($"[INFO] : {msg}");
    }

    public void Warn(string msg)
    {
        _sb.AppendLine($"[WARNING] : {msg}");
    }

    public void Error(string msg)
    {
        _sb.AppendLine($"[ERROR] : {msg}");
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