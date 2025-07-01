# How To Create A Command

You use attributes in order to make a command

## Methods

## 1. Static Method

If your method is static (public or private) it's very simple to add to the console command system

Simply add the `[ConsoleCommand(command, description (optional), isCheat (optional))]` before your method and you're done, this also works with methods that has args

### Example :
```CS
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
```

This is an actual command used in the game, the method name doesn't matter, Cc stands for Console Command, you can use it as a hint

## 2. Non-Static Method (Instance)

The same as before add the `[ConsoleCommand()]` attribute, except you have to register the instance to the ConsoleSystem using `ConsoleCommandSystem.RegisterInstance(this);` in your `_Ready`

### Example :

Here's a command from the Player class

```CS
[ConsoleCommand("player_move", "Moves Player To Given Position (x, y, z)", true)]
public void MovePosition(float x, float y, float z)
{
	Position = new Vector3(x, y, z);
}
```

In the Init function you can see `ConsoleCommandSystem.RegisterInstance(this);` here

```CS
private void Init()
{
	private void Init()
{
    // Should be unlocked from outside
    Lock();

    Conf = (PlayerMovementConfig)_res_playerMovementConfig.Duplicate();

    _variableGravity = Conf.Gravity;

    _feet = NodeUtils.GetChildWithName<Area3D>("Feet", this);
    _feet.AreaEntered += Feet_AreaEntered;

    _head = GetChild<Head>(0);

    _camera = _head.Camera;

    _jumpHeight = Mathf.Sqrt(2 * Conf.Gravity * Conf.JumpModifier);

    Input.MouseMode = Input.MouseModeEnum.Captured;

    _game = Game.GetGame(this);

    Validation();

    --> ConsoleCommandSystem.RegisterInstance(this); <--


    _console = _game.Console;

}
}
```

*this could be outdated*

You must register the class instance in order for the command to work, or else you will get the error `Non-static method requires a target`
