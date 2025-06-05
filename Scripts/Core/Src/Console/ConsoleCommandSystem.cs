using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace EIODE.Core.Console;

public static class ConsoleCommandSystem
{
    private static readonly Dictionary<string, (MethodInfo Method, string Description, bool IsCheat)> _commands = new(StringComparer.OrdinalIgnoreCase);

    public static void Initialize()
    {
        // Find all methods with the ConsoleCommandAttribute in all assemblies
        var methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(method => method.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() != null);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
            if (attribute != null)
            {
                _commands[attribute.Command] = (method, attribute.Description, attribute.IsCheat);
                GD.Print($"Registered command '{attribute.Command}'{(attribute.IsCheat ? " [CHEAT]" : "")}: {attribute.Description}");
            }
        }
        GD.Print($"Console Command Is Ready !, {_commands.Count} Commands found");
    }
    public static void ExecuteCommand(string command)
    {
        if (_commands.TryGetValue(command, out var commandInfo))
        {
            try
            {
                // Invoke the method with null as the instance (since they're static)
                commandInfo.Method.Invoke(null, null);
            }
            catch (Exception ex)
            {
                GD.PushError($"Error executing command '{command}': {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        else
        {
            GD.PushWarning($"Unknown command: {command}");
        }
    }
    [ConsoleCommand("help", "help :D")]
    public static void Help()
    {
        foreach (var command in _commands)
        {
            GD.Print($"{command.Key} | {command.Value.Description}");
        }
    }
}