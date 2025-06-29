using EIODE.Scenes;
using EIODE.Core.Console;
using EIODE.Scenes.Debug;
using EIODE.Utils;
using EIODE.Resources;
using EIODE.Scenes.Triggers;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace EIODE.Core;

public partial class Game : Node
{
    [Export] public PackedScene FirstLevelToLoad;
    [Export] public bool Disabled { get; set; }
    public static Vector3 PlayerSpawnPosition { get; private set; } = Vector3.Zero;
    private bool _playerReady = false;
    private bool _isMouseShowed = false;
    private DebugUi _debugUi = null;
    public bool FirstLevelLoaded { get; private set; } = false;

    public Player Player { get; private set; }
    public DevConsole Console { get; private set; }
    public static readonly string Location = "/root/Game";
    private const string WEAPONS_PATH = "res://Resources/Gun Types/";
    private bool _initCommands = true;
    private static Dictionary<string, WeaponConfig> _weaponsLookup = [];

    public override void _Ready()
    {
        ParseArgs();

        if (Disabled)
        {
            GD.Print("Game scene is disabled.");
            return;
        }

        if (_initCommands) ConsoleCommandSystem.Initialize();

        SetConsole();
        SpawnPlayer();
        LoadFirstLevel();
        SpawnDebugUI(false);
        _weaponsLookup = LoadAllWeapons();
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

    public void SetPlayerSpawnPosition(Vector3 newPos)
    {
        PlayerSpawnPosition = newPos;
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

    public static Dictionary<string, WeaponConfig> LoadAllWeapons()
    {
        Dictionary<string, WeaponConfig> loadedWeapons = [];
        foreach (var weaponPath in ResourceLoader.ListDirectory(WEAPONS_PATH))
        {
            var currentWeapon = ResourceLoader.Load<WeaponConfig>(Path.Combine(WEAPONS_PATH, weaponPath));
            loadedWeapons.Add(currentWeapon.Name, currentWeapon);
        }
        return loadedWeapons;
    }

    public WeaponConfig FindWeapon(string name)
    {
        // Lazy initialize the lookup if empty
        _weaponsLookup ??= LoadAllWeapons();

        return _weaponsLookup.TryGetValue(name, out var weapon) ? weapon : null;
    }

    public WeaponConfig[] FindWeapons(params string[] names)
    {
        var result = new List<WeaponConfig>();

        foreach (var item in _weaponsLookup)
        {
            Console.Log(item.ToString());
        }

        foreach (var name in names)
        {
            if (_weaponsLookup.TryGetValue(name.ToLower(), out var weapon))
            {
                result.Add(weapon);
                Console?.Log(weapon.Name + " was Found");
            }
        }

        return result.ToArray();
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

    private void ParseArgs()
    {
        var args = System.Environment.GetCommandLineArgs();
        string arg = "";
        if (args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                arg = args[i];

                switch (arg.ToLower())
                {
                    case "--disable-commands":
                        _initCommands = false;
                        GD.Print("Commands are disabled");
                        break;
                    case "--cache-levels":
                        LevelLoader.Cc_CacheAllLevels();
                        break;
                    case "--level":
                        FirstLevelToLoad = LevelLoader.LoadLevel(LevelLoader.Levelize(args[i + 1].Trim()));
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
    }

    #region CC



    [ConsoleCommand("triggers_visual_visible", "Show or hide all trigger visibility visual in current scene (1 | 0)")]
    public void Cc_HideTriggersVisual(int arg)
    {
        var triggersFound = NodeUtils.GetChildrenWithNodeType<TriggerVisibility>(LevelLoader.Instance.CurrentLevel);
        if (triggersFound.Length > 0)
        {
            foreach (var trigger in triggersFound)
            {
                switch (arg)
                {
                    case 0:
                        trigger.HideTriggerVisual();
                        break;
                    case 1:
                        trigger.ShowTriggerVisual();
                        break;
                    default:
                        Console?.Log($"Excepted either \"0 (hide)\" or \"1 (show)\", received {arg}");
                        break;
                }
            }
        }
        else
        {
            Console?.Log("Couldn't find any Triggers in current scene", DevConsole.LogLevel.ERROR);
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
