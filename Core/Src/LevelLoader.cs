using EIODE.Core.Console;
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EIODE.Utils;

namespace EIODE.Core;

public partial class LevelLoader : Node
{
    public static LevelLoader Instance { get; private set; }
    public Node CurrentLevel { get; set; } = null;
    public const string LEVELS_PATH = "res://Scenes/Levels/";
    private DevConsole _console;
    private static readonly Dictionary<string, PackedScene> _allLevels = [];
    public override void _EnterTree()
    {
        Instance = this;
        _console = DevConsole.Instance;
    }

    public static PackedScene LoadLevel(string path)
    {
        try
        {
            if (_allLevels.TryGetValue(path, out var cachedLevel))
            {
                DevConsole.Instance?.Log($"Loading level {path} from cache ...");
                return cachedLevel;
            }
            else
            {
                DevConsole.Instance?.Log($"Loading level {path} ...");
                PackedScene level = ResourceLoader.Load<PackedScene>(path);
                if (level != null)
                {
                    _allLevels.Add(path, level);
                    return level;
                }
                else
                {
                    DevConsole.Instance?.Log($"Couldn't load level {path}", DevConsole.LogLevel.ERROR);
                    throw new NullReferenceException();
                }
            }
        }
        catch (Exception e)
        {
            DevConsole.Instance?.Log($"Error while loading level {e}", DevConsole.LogLevel.ERROR);
            throw;
        }
    }

    /// <summary>
    /// Combines <paramref name="levelName"/> with the LEVELS_PATH const and adds .tscn to the end if it doesn't already
    /// </summary>
    /// <param name="levelName"></param>
    /// <returns>res://Scenes/Levels/<paramref name="levelName"/></returns>
    public static string Levelize(string levelName)
    {
        if (levelName.EndsWith(".tscn"))
        {
            return Path.Combine(LEVELS_PATH, levelName);
        }
        else
        {
            return Path.Combine(LEVELS_PATH, levelName + ".tscn");
        }
    }

    public void ChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
    {
        _console?.Log($"Changing Level to {newLevel} ...");
        CallDeferred(MethodName.DeferredChangeLevel, newLevel, freeCurrentLevel, movePlayer);
        _console?.Log($"Changed current level to {newLevel}");

    }

    public void ChangeLevel(string newLevelPath, bool freeCurrentLevel = true, bool movePlayer = true)
    {
        _console?.Log($"Changing Level to {newLevelPath} ...");
        CallDeferred(MethodName.DeferredChangeLevel, LoadLevel(newLevelPath), freeCurrentLevel, movePlayer);
        _console?.Log($"Changed current level to {newLevelPath}");
    }

    private void DeferredChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
    {
        if (newLevel == null)
        {
            _console?.Log("Given level to change to is null", DevConsole.LogLevel.ERROR);
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
            _console?.Log($"Moving player to {newLevel} ...");
            var player = Game.GetGame(this).GetPlayer();

            if (NodeUtils.GetChildWithName<Node3D>("SPAWN", CurrentLevel) != null)
            {
                Game.GetGame(this).SetPlayerSpawnPosition(NodeUtils.GetChildWithName<Node3D>("SPAWN", CurrentLevel).GlobalPosition);
                player.GlobalPosition = Game.PlayerSpawnPosition;
            }
            else
            {
                player.GlobalPosition = new Vector3(0, 10, 0);
            }

            player.Lock();
            player.Reset();
            player.Reparent(CurrentLevel);

            if (Game.GetGame(this).Console != null)
                if (!Game.GetGame(this).Console.IsShown)
                    player.UnLock();
        }
    }
    public static string[] GetAllLevelsPath()
    {
        string[] levelsFound = [];

        // If inside the editor, search in "res://"
        if (Engine.IsEditorHint())
        {
            using var dir = DirAccess.Open(LEVELS_PATH);
            levelsFound = dir?.GetFiles() ?? [];
            return levelsFound;
        }
        //else, search in the packed resources
        else
        {
            levelsFound = ResourceLoader.ListDirectory(LEVELS_PATH);
            return levelsFound;
        }
    }


    #region CC
    [ConsoleCommand("change_level", "Changes levels to given level name (string)")]
    public static void Cc_ChangeLevel(string levelName)
    {
        var path = Path.Combine(LEVELS_PATH, levelName.EndsWith(".tscn") ? levelName : levelName + ".tscn");


        if (ResourceLoader.Exists(path))
        {
            PackedScene level = LoadLevel(path);
            Instance.ChangeLevel(level);
        }
        else
        {
            Game.GetGame(Instance).Console?.Log($"Level at {path} was not found", DevConsole.LogLevel.ERROR);
        }
    }

    [ConsoleCommand("list_levels", "Lists all levels in the Scenes//Levels folder")]
    public static void Cc_ListLevels()
    {
        // I heard StringBuilder is better in loops
        var levelsFoundString = new StringBuilder("\n");
        string[] levelsFound;

        // If inside the editor, search in "res://"
        if (Engine.IsEditorHint())
        {
            using var dir = DirAccess.Open(LEVELS_PATH);
            levelsFound = dir?.GetFiles() ?? Array.Empty<string>();
        }
        else
        {
            levelsFound = ResourceLoader.ListDirectory(LEVELS_PATH);
        }

        foreach (var level in levelsFound)
        {
            if (level.EndsWith(".tscn"))
            {
                levelsFoundString.AppendLine(level);
            }
        }

        DevConsole.Instance?.Log($"{levelsFound.Count(l => l.EndsWith(".tscn"))} Scenes Found: {levelsFoundString}");
    }

    [ConsoleCommand("cache_all_levels", "Loads all levels and cache them in a list")]
    public static void Cc_CacheAllLevels()
    {
        string[] levelsPath = GetAllLevelsPath();
        PackedScene currentLevel;
        foreach (var levelPath in levelsPath)
        {
            var levelPathCombined = Path.Combine(LEVELS_PATH, levelPath);
            if (!_allLevels.ContainsKey(levelPathCombined))
            {
                currentLevel = LoadLevel(levelPathCombined);
            }
        }

        DevConsole.Instance?.Log($"Finished loading {_allLevels.Count}");
    }

    [ConsoleCommand("cache_clear", "Clears both memory and file level caches")]
    public static void Cc_CacheClear()
    {
        _allLevels.Clear();

        DevConsole.Instance?.Log("Level cache cleared");
    }

    #endregion





}
