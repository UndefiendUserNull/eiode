# How To Create A Command

You use attributes in order to make a command

## Methods

## 1. Static Method

If your method is static (public or private) it's very simple to add to the console command system

Simply add the `[ConsoleCommand(command, description (optional), isCheat (optional, defaults to false))]` before your method and you're done, this also works with methods that has args

### Example :
```CS
[ConsoleCommand("list_levels", "Lists all levels in the Scenes//Levels folder")]
public static void Cc_ListLevels()
{
	string scenesFoundString = "\n";
	int scenesFoundLength = 0;
	foreach (string item in DirAccess.Open(LEVELS_PATH).GetFiles())
	{
		scenesFoundString += $"{item}\n";
		scenesFoundLength++;
	}
	Game.GetGame(Instance).Console.Log($"{scenesFoundLength} Scenes Found : {scenesFoundString}");
}
```

This is an actual command used in the game, the method name doesn't matter, Cc stands for Console Command, you can use it as a hint

## 2. Non-Static Method (Instance)

The same as before add the `[ConsoleCommand()]` attribute, except you have to register the instance to the ConsoleSystem using `ConsoleCommandSystem.RegisterInstance(this);` in your `_Ready`

### Example :

Here's a command from the PlayerMovement class

```CS
[ConsoleCommand("player_move", "Moves Player To Given Position (x, y, z)", true)]
public void MovePosition(float x, float y, float z)
{
	Position = new Vector3(x, y, z);
}
```

In player's Init function you can see `ConsoleCommandSystem.RegisterInstance(this);` here

```CS
private void Init()
{
	Validation();
	// Should be unlocked from outside
	Lock();
	_feet = NodeUtils.GetChildWithNodeType<RayCast3D>(this);
	if (!_feet.Name.ToString().Equals("feet", System.StringComparison.CurrentCultureIgnoreCase)) GD.PushWarning("Raycast's name found in player is not \"feet\"");
	_head = GetChild<Node3D>(0);
	_headSrc = _head as Head;
	_jumpHeight = Mathf.Sqrt(2 * S._gravity * S._jumpModifier);
	Input.MouseMode = Input.MouseModeEnum.Captured;
--> ConsoleCommandSystem.RegisterInstance(this); <--
}
```

*this could be outdated*

You must register the class instance in order for the command to work, or else you will get the error `Non-static method requires a target`

### CAUTION : As for now some Singleton classes has problems with having non-static commands throwing the error `Non-static method requires a target` even when registering the Instance