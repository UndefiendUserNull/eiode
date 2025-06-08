using EIODE.Core.Console;
using Godot;
using System.IO;

namespace EIODE.Scripts.Core;

public partial class LevelLoader : Node
{
    public static LevelLoader Instance { get; private set; }
    public Node CurrentLevel { get; set; } = null;
    private const string LEVELS_PATH = "res://Scenes/Levels/";
    private DevConsole _console;
    public override void _EnterTree()
    {
        ConsoleCommandSystem.RegisterInstance(this);

        Instance = this;
        _console = Game.GetGame(this).Console;
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
        Game.GetGame(this).Console.Log($"Changing Level to {newLevel}");
        CallDeferred(MethodName.DeferredChangeLevel, newLevel, freeCurrentLevel, movePlayer);
    }
    private void DeferredChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
    {
        if (freeCurrentLevel) CurrentLevel.QueueFree();
        CurrentLevel = newLevel.Instantiate();
        GetTree().Root.AddChild(CurrentLevel);
        GetTree().CurrentScene = CurrentLevel;

        if (movePlayer)
        {
            var player = Game.GetGame(this).Player;
            player.Lock();
            player.Reparent(CurrentLevel);
            player.UnLock();
        }
    }
    [ConsoleCommand("change_level", "Changes levels to given level name (string)")]
    public void Cc_ChangeLevel(string levelName)
    {
        string path = string.Empty;

        if (!levelName.EndsWith(".tscn"))
            path = Path.Combine(LEVELS_PATH, levelName + ".tscn");
        else
            path = Path.Combine(LEVELS_PATH, levelName);

        _console.Log(path);
        if (File.Exists(path))
        {
            _console.Log($"Changing Level to {path} ...");
            PackedScene level = LoadLevel(path);
            ChangeLevel(level);
        }
        else
        {
            _console.Log($"Level {path} was not found", DevConsole.LogLevel.ERROR);
        }
    }
    [ConsoleCommand("list_levels", "Lists all levels in the Scenes//Levels folder")]
    public static void Cc_ListLevels()
    {
        string scenesFoundString = "\n";
        int scenesFoundLength = 0;
        foreach (string item in DirAccess.Open(LEVELS_PATH).GetFiles())
        {
            if (item.EndsWith(".tscn"))
            {
                scenesFoundString += $"{item}\n";
                scenesFoundLength++;
            }
        }
        Game.GetGame(Instance).Console.Log($"{scenesFoundLength} Scenes Found : {scenesFoundString}");
    }
}
