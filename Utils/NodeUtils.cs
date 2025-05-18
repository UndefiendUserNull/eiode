using Godot;

namespace EIODE.Utils;

public static class NodeUtils
{
    public static T GetChildWithNode<T>(Node parent) where T : Node
    {
        if (parent == null)
        {
            GD.PrintErr("Null " + nameof(parent));
        }

        foreach (Node child in parent.GetChildren())
        {
            if (child is T foundChild)
            {
                return foundChild;
            }

            var recursiveResult = GetChildWithNode<T>(child);
            if (recursiveResult != null)
            {
                return recursiveResult;
            }
        }
        return default;
    }
}

