using EIODE.Core.Console;
using Godot;
using System;

namespace EIODE.Scripts.Core;

public partial class LevelLoader : Node
{
    public static LevelLoader Instance { get; private set; }
    public Node CurrentLevel { get; set; } = null;
    public const string LEVELS_PATH = "res://Scenes/Levels/";
    private DevConsole _console;

    public override void _EnterTree()
    {
        Instance = this;
    }
    public static PackedScene LoadLevel(string path)
    {
        try
        {
            GD.Print($"Loading level {path} ...");
            return ResourceLoader.Load<PackedScene>(path);
        }
        catch (Exception e)
        {
            GD.PushError($"Error while loading level {e}");
            throw;
        }
    }

    public void ChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
    {
        Game.GetGame(this).Console?.Log($"Changing Level to {newLevel}");
        CallDeferred(MethodName.DeferredChangeLevel, newLevel, freeCurrentLevel, movePlayer);
    }

    private void DeferredChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
    {
        if (newLevel == null)
        {
            GD.PushError("Given level to change to is null");
            return;
        }
        if (freeCurrentLevel) CurrentLevel.QueueFree();

        if (newLevel != null)
        {
            CurrentLevel = newLevel.Instantiate();
            GetTree().Root.AddChild(CurrentLevel);
            GetTree().CurrentScene = CurrentLevel;
        }

        if (movePlayer)
        {
            Game.GetGame(this).Console?.Log($"Moving player to {newLevel} ...");
            var player = Game.GetGame(this).GetPlayer();
            player.Position = Game.PLAYER_SPAWN_POSITION;
            player.GetHead().Rotation = Vector3.Zero;
            player.Lock();
            player.Velocity = Vector3.Zero;
            player.Reparent(CurrentLevel);
            if (Game.GetGame(this).Console != null)
                if (!Game.GetGame(this).Console.IsShown)
                    player.UnLock();
        }
    }

    [ConsoleCommand("list_levels", "Lists all levels in the Scenes//Levels folder")]
    public static void Cc_ListLevels()
    {
        string levelsFoundString = "\n";
        string[] levelsFound;

        // If inside the editor, search in "res://"
        if (Engine.IsEditorHint())
        {
            using var dir = DirAccess.Open(LEVELS_PATH);
            levelsFound = dir?.GetFiles() ?? Array.Empty<string>();
        }
        //else, search in the packed resources
        else
        {
            levelsFound = ResourceLoader.ListDirectory(LEVELS_PATH);
        }

        foreach (var level in levelsFound)
        {
            levelsFoundString += level + "\n";
        }

        Game.GetGame(Instance).Console?.Log($"{levelsFound.Length} Scenes Found : {levelsFoundString}");
    }
}
