using EIODE.Scenes.Player;
using EIODE.Core.Console;
using EIODE.Utils;
using Godot;
using System.IO;

namespace EIODE.Scripts.Core;

public partial class Game : Node
{
    [Export] public PackedScene FirstLevelToLoad;
    [Export] public bool Disabled { get; set; }
    private bool _playerReady = false;
    private bool _isMouseShowed = false;
    public bool FirstLevelLoaded { get; private set; } = false;
    public static readonly Vector3 PLAYER_SPAWN_POSITION = new(0, 5, 0);

    public PlayerMovement Player { get; private set; }
    public DevConsole Console { get; private set; }
    public static readonly string Location = "/root/Game";
    public bool InitSpawnConsole { get; private set; } = true;

    public override void _Ready()
    {
        var args = System.Environment.GetCommandLineArgs();
        if (args.Length > 0)
        {
            foreach (var arg in args)
            {
                switch (arg.ToLower())
                {
                    case "--disable-game":
                        Disabled = true;
                        break;
                    case "--no-console":
                        InitSpawnConsole = false;
                        break;
                    case "--1080p":
                        DisplayServer.WindowSetSize(new Vector2I(1920, 1080));
                        break;
                }
            }
        }
        var zebby = "";
        foreach (var item in args)
        {
            zebby += item + ", ";
        }
        zebby += "  " + args.Length;
        GD.Print("ARGS: " + zebby);
        if (Disabled)
        {
            GD.Print("Game scene is disabled.");
            return;
        }
        if (InitSpawnConsole == true)
        {
            GD.Print("Console Is Enabled");
            ConsoleCommandSystem.Initialize();
            SpawnConsole();
        }
        else
        {
            GD.Print("Console Is Disabaled");
        }
        SpawnPlayer();
        LoadFirstLevel();
        HideMouse();
        ConsoleCommandSystem.RegisterInstance(this);
        Console?.Log("Game _Ready finished");
    }

    private void SpawnConsole()
    {
        var consoleScene = ResourceLoader.Load<PackedScene>(ScenesHash.CONSOLE_SCENE);
        Console = consoleScene.Instantiate<DevConsole>();
        GetTree().Root.CallDeferred(MethodName.AddChild, Console);
        Console.Name = "Console";
        Console?.Log("Console ready", DevConsole.LogLevel.INFO);
    }

    private void LoadFirstLevel()
    {
        LevelLoader.Instance.ChangeLevel(FirstLevelToLoad, false);
        // idk why the fuck that works instead of setting the position directly
        Player.SetDeferred(Node3D.PropertyName.GlobalPosition, PLAYER_SPAWN_POSITION);
    }
    public PlayerMovement GetPlayer()
    {
        if (Player == null)
        {
            GD.PushError("Player is null");
            return null;
        }
        return Player;
    }

    // Pass 'this' for the placeHolder, it does nothing
    public static Game GetGame(Node placeHolder)
    {
        return placeHolder.GetTree().Root.GetNode<Game>(Location);
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed(InputHash.K_ESC))
        {
            _isMouseShowed = !_isMouseShowed;
            if (_isMouseShowed) HideMouse();
            else ShowMouse();
        }
    }

    public static void ShowMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public static void HideMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void SpawnPlayer()
    {
        var playerScene = ResourceLoader.Load<PackedScene>(ScenesHash.PLAYER_SCENE);
        Player = playerScene.Instantiate<PlayerMovement>();
        GetTree().Root.CallDeferred(MethodName.AddChild, Player);
        _playerReady = true;
        Player.Name = "Player";
        Console?.Log("Player ready");
    }

    [ConsoleCommand("change_level", "Changes levels to given level name (string)")]
    public void Cc_ChangeLevel(string levelName)
    {
        string path;

        if (!levelName.EndsWith(".tscn"))
            path = Path.Combine(LevelLoader.LEVELS_PATH, levelName + ".tscn");
        else
            path = Path.Combine(LevelLoader.LEVELS_PATH, levelName);

        Console?.Log(path);

        if (Engine.IsEditorHint())
        {
            if (Godot.FileAccess.FileExists(path))
            {
                PackedScene level = LevelLoader.LoadLevel(path);
                LevelLoader.Instance.ChangeLevel(level);
            }
            else
            {
                Console?.Log($"Level at {path} was not found", DevConsole.LogLevel.ERROR);
            }
        }
        else
        {
            if (ResourceLoader.Exists(path))
            {
                PackedScene level = LevelLoader.LoadLevel(path);
                LevelLoader.Instance.ChangeLevel(level);
            }
            else
            {
                Console?.Log($"Level at {path} was not found", DevConsole.LogLevel.ERROR);
            }
        }
    }
}
