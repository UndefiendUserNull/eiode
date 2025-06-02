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

    public static Node3D GetPlayerFromRoot(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is PlayerMovement playerFound)
                return playerFound;

            if (child.GetChildCount() > 0)
            {
                var foundInChildren = GetPlayerFromRoot(child);
                if (foundInChildren != null)
                    return foundInChildren;
            }
        }
        return null; // Player not found
    }
}

