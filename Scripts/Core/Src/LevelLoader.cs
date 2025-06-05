using Godot;
using System.IO;

namespace EIODE.Scripts.Core;

public partial class LevelLoader : Node
{
    public static LevelLoader Instance { get; private set; }
    public Node CurrentLevel { get; set; } = null;
    private const string LEVELS_PATH = "res://Scenes/Levels/";
    public override void _EnterTree()
    {
        Instance = this;
    }
    public static PackedScene LoadLevel(string path)
    {
        try
        {
            GD.Print($"Loading level {path}");
            return ResourceLoader.Load<PackedScene>(path);
        }
        catch (System.Exception e)
        {
            GD.PushError($"Error while loading level {e}");
            throw;
        }
    }
    public void ChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
    {
        GD.Print($"Changing Level to {newLevel}");
        CallDeferred(MethodName.DeferredChangeLevel, newLevel, freeCurrentLevel, movePlayer);
    }
    private void DeferredChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
    {
        // TODO: Levels in the future should have their own class
        if (freeCurrentLevel) CurrentLevel.QueueFree();
        CurrentLevel = newLevel.Instantiate();
        GetTree().Root.AddChild(CurrentLevel);
        GetTree().CurrentScene = CurrentLevel;

        if (movePlayer)
        {
            var player = Game.GetGame(this).Player;
            player.Lock();
            player.Reparent(CurrentLevel);
            // TODO: Also move player global position to the level's starting position, it should be a public Vector3 in the level class
            player.UnLock();
        }
    }
    public void cc_ChangeLevel(string levelName)
    {
        var path = Path.Combine(LEVELS_PATH, levelName + ".tscn");
        if (File.Exists(path))
        {
            PackedScene level = LoadLevel(path);
            ChangeLevel(level);
        }
        else
        {
            GD.PrintErr("Level was not found");
        }
    }
    public void cc_ListLevels()
    {
        foreach (string item in Directory.EnumerateFiles(LEVELS_PATH))
        {
            GD.Print(item + '\n');
        }
    }
}
