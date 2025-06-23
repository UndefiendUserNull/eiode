using EIODE.Scenes.Player;
using EIODE.Core.Console;
using EIODE.Scenes.Debug;
using EIODE.Utils;
using System.IO;
using Godot;

namespace EIODE.Scripts.Core;

public partial class Game : Node
{
    [Export] public PackedScene FirstLevelToLoad;
    [Export] public bool Disabled { get; set; }
    private bool _playerReady = false;
    private bool _isMouseShowed = false;
    private DebugUi _debugUi = null;
    public bool FirstLevelLoaded { get; private set; } = false;
    public static Vector3 PlayerSpawnPosition = new(0, 5, 0);

    public Player Player { get; private set; }
    public DevConsole Console { get; private set; }
    public static readonly string Location = "/root/Game";

    public override void _Ready()
    {
        var args = System.Environment.GetCommandLineArgs();

        if (args.Length > 0)
        {
            foreach (var arg in args)
            {
                switch (arg.ToLower())
                {
                    case "--cache-levels":
                        LevelLoader.Cc_CacheAllLevels();
                        break;
                    case "--disable-game":
                        Disabled = true;
                        break;
                    case "--1080p":
                        DisplayServer.WindowSetSize(new Vector2I(1920, 1080));
                        break;
                }
            }
        }

        if (Disabled)
        {
            GD.Print("Game scene is disabled.");
            return;
        }

        ConsoleCommandSystem.Initialize();

        SetConsole();
        SpawnPlayer();
        LoadFirstLevel();
        SpawnDebugUI(false);
        HideMouse();

        ConsoleCommandSystem.RegisterInstance(this);

        Console?.Log("Game _Ready finished");
    }

    private void SetConsole()
    {
        Console = DevConsole.Instance;
    }

    private void LoadFirstLevel()
    {
        DevConsole.Instance?.Log("Loading first level.");
        LevelLoader.Instance.ChangeLevel(FirstLevelToLoad, false);
        // idk why the fuck that works instead of setting the position directly
        Player.SetDeferred(Node3D.PropertyName.GlobalPosition, PlayerSpawnPosition);
    }
    public Player GetPlayer()
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
            if (_isMouseShowed) ShowMouse();
            else HideMouse();
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
        Player = playerScene.Instantiate<Player>();
        GetTree().Root.CallDeferred(MethodName.AddChild, Player);
        _playerReady = true;
        Player.Name = "Player";
        Console?.Log("Player ready");
    }

    private void SpawnDebugUI(bool enableOnSpawn)
    {
        _debugUi = ResourceLoader.Load<PackedScene>(ScenesHash.DEBUG_UI_SCENE).Instantiate<DebugUi>();
        _debugUi.Name = "Debug UI";

        if (enableOnSpawn) _debugUi.EnableUI();
        else _debugUi.DisableUI();

        GetTree().Root.CallDeferred(MethodName.AddChild, _debugUi);
        Console?.Log("Debug UI Created.");
    }

    #region CC

    [ConsoleCommand("change_level", "Changes levels to given level name (string)")]
    public void Cc_ChangeLevel(string levelName)
    {
        var path = Path.Combine(LevelLoader.LEVELS_PATH, levelName.EndsWith(".tscn") ? levelName : levelName + ".tscn");


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

    [ConsoleCommand("toggle_debug_ui", "Creates the standard debug ui (int).")]
    public void Cc_ToggleDebugUi(int toggle = 1)
    {
        if (toggle == 1)
        {
            if (_debugUi == null)
            {
                SpawnDebugUI(true);
            }
            else _debugUi.EnableUI();

            Console?.Log("Debug UI Enabled.");
        }
        else if (toggle == 0)
        {
            if (_debugUi == null)
            {
                Console?.Log("There's no Debug ui, use \"toggle_debug_ui 1\" to create another one", DevConsole.LogLevel.WARNING);
                return;
            }
            _debugUi.DisableUI();
            Console?.Log("Debug UI Disabled.");
        }
    }

    #endregion
}
