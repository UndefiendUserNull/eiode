using EIODE.Scenes.Player;
using Godot;

namespace EIODE.Utils;

public static class NodeUtils
{
    /// <summary>
    /// Searches parent's children for any child that's of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Searches parent's children for any node that's named <paramref name="name"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
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

