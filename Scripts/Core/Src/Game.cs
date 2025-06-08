using EIODE.Scenes.Player;
using EIODE.Core.Console;
using EIODE.Utils;
using Godot;

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
    public override void _Ready()
    {
        if (Disabled)
        {
            GD.Print("Game scene is disabled.");
            return;
        }

        ConsoleCommandSystem.Initialize();
        SpawnConsole();
        SpawnPlayer();
        LoadFirstLevel();
        HideMouse();
        ConsoleCommandSystem.RegisterInstance(this);
        Console.Print("Game _Ready finished");
    }

    private void SpawnConsole()
    {
        var consoleScene = ResourceLoader.Load<PackedScene>(ScenesHash.CONSOLE_SCENE);
        Console = consoleScene.Instantiate<DevConsole>();
        GetTree().Root.CallDeferred(MethodName.AddChild, Console);
        Console.Name = "Console";
        Console.Log("Console ready", DevConsole.LogLevel.INFO);
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
    [ConsoleCommand("ReloadScene", "Reloads Current Scene")]
    public void ReloadCurrentScene()
    {
        GetTree().ReloadCurrentScene();
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
        Console.Print("Player ready");
    }

}
