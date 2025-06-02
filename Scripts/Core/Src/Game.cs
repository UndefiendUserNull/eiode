using EIODE.Scenes.Player;
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
        Input.MouseMode = Input.MouseModeEnum.Captured;
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
    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed(InputHash.K_ESC))
        {
            _isMouseShowed = !_isMouseShowed;
            Input.MouseMode = !_isMouseShowed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        }
        if (Input.IsActionJustPressed(InputHash.D_RELOAD_SCENE)) { GetTree().ReloadCurrentScene(); }
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
