using EIODE.Scenes.Player;
using Godot;

namespace EIODE.Utils;

public static class NodeUtils
{
    public static T GetChildWithNodeType<T>(Node parent) where T : Node
    {
        if (parent == null)
        {
            GD.PushError("Null " + nameof(parent));
        }

        foreach (Node child in parent.GetChildren())
        {
            if (child is T foundChild)
            {
                return foundChild;
            }

            var recursiveResult = GetChildWithNodeType<T>(child);
            if (recursiveResult != null)
            {
                return recursiveResult;
            }
        }
        return default;
    }

    public static T GetChildWithName<T>(string name, Node parent) where T : Node
    {
        if (parent == null)
        {
            GD.PushError("Null " + nameof(parent));
        }

        foreach (Node child in parent.GetChildren())
        {
            if (child.Name == name && child is T childFound)
            {
                return childFound;
            }

        }
        return default;
    }
}

