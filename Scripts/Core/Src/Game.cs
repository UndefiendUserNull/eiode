using EIODE.Scenes.Player;
using EIODE.Core.Console;
using EIODE.Utils;
using Godot;

namespace EIODE.Scripts.Core;

public partial class Game : Node
{
    [Export] public PackedScene FirstLevelToLoad;
    private bool _playerReady = false;
    private bool _isMouseShowed = false;
    private bool _firstLevelLoaded = false;
    private readonly Vector3 PLAYER_SPAWN_POSITION = new(0, 15, 0);

    public PlayerMovement Player { get; private set; }
    public static readonly string Location = "/root/Game";
    public override void _Ready()
    {
        SpawnPlayer();
        LoadFirstLevel();
        HideMouse();
        ConsoleCommandSystem.Initialize();
        GD.Print("Game _Ready finished");
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
        return placeHolder.GetNode<Game>(Location);
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed(InputHash.K_ESC))
        {
            _isMouseShowed = !_isMouseShowed;
            if (_isMouseShowed) HideMouse();
            else ShowMouse();
        }
        if (Input.IsActionJustPressed(InputHash.D_RELOAD_SCENE)) { GetTree().ReloadCurrentScene(); }
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
        GD.Print("Player ready");
    }
}
