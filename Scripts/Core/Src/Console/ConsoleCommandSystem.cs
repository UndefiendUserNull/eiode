using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace EIODE.Core.Console;

public static class ConsoleCommandSystem
{
    private static readonly Dictionary<string, (MethodInfo Method, object Target, string Description, bool IsCheat)>
        _commands = new(StringComparer.OrdinalIgnoreCase);

    public static void Initialize()
    {
        var methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(method => method.GetCustomAttribute<ConsoleCommandAttribute>() != null);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
            if (attribute != null)
            {
                // For instance methods, we need to register the object later
                _commands[attribute.Command] = (method, null, attribute.Description, attribute.IsCheat);
                GD.Print($"Registered command '{attribute.Command}'{(attribute.IsCheat ? " [CHEAT]" : "")}: {attribute.Description}");
            }
        }
        GD.Print($"Console Command System is Ready! {_commands.Count} commands found.");
    }

    // Register an instance object's methods (non-static)
    public static void RegisterInstance(object instance)
    {
        var methods = instance.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<ConsoleCommandAttribute>() != null);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
            _commands[attribute.Command] = (method, instance, attribute.Description, attribute.IsCheat);
        }
    }

    public static void ExecuteCommand(string input)
    {
        string[] parts = input.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            GD.PushError("No command entered.");
            return;
        }

        string commandName = parts[0];
        string[] args = [.. parts.Skip(1)];

        if (!_commands.TryGetValue(commandName, out var command))
        {
            GD.PushError($"Unknown command: '{commandName}'");
            return;
        }

        try
        {
            ParameterInfo[] parameters = command.Method.GetParameters();
            object[] parsedArgs = ParseArguments(parameters, args);
            command.Method.Invoke(command.Target, parsedArgs); // Works for both static and instance
        }
        catch (Exception ex)
        {
            GD.PushError($"Error executing '{commandName}': {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    private static object[] ParseArguments(ParameterInfo[] parameters, string[] args)
    {
        if (args.Length < parameters.Count(p => !p.IsOptional))
        {
            GD.PushError($"Not enough arguments. Expected: {parameters.Length}, Got: {args.Length}");
            throw new ArgumentException("Insufficient arguments");
        }

        if (args.Length > parameters.Length)
        {
            GD.PushError($"Too many arguments. Expected: {parameters.Length}, Got: {args.Length}");
            throw new ArgumentException("Too many arguments");
        }

        object[] parsedArgs = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            try
            {
                if (i < args.Length)
                {
                    parsedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
                }
                else if (parameters[i].IsOptional)
                {
                    parsedArgs[i] = parameters[i].DefaultValue;
                }
                else
                {
                    throw new ArgumentException($"Missing required argument: '{parameters[i].Name}'");
                }
            }
            catch (Exception ex)
            {
                GD.PushError($"Failed to parse argument {i + 1} ({parameters[i].Name}): {ex.Message}");
                throw;
            }
        }
        return parsedArgs;
    }

    [ConsoleCommand("help", "Helps you :D")]
    public static void Help()
    {
        foreach (var cmd in _commands)
        {
            GD.Print($"{cmd.Key}: {cmd.Value.Description}");
        }
    }
}