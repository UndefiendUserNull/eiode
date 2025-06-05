using System;

namespace EIODE.Core.Console;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ConsoleCommandAttribute : Attribute
{
    public string Command { get; }
    public string Description { get; }
    public bool IsCheat { get; }

    public ConsoleCommandAttribute(string command, string description, bool isCheat = false)
    {
        Command = command;
        Description = description;
        IsCheat = isCheat;
    }
}
