using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EIODE.Utils;

public static class NodeUtils
{
    /// <summary>
    /// Checks if tha parent has any children
    /// </summary>
    public static bool CheckForChildren(Node parent)
    {
        if (parent.GetChildCount() == 0)
        {
            return false;
        }
        return true;
    }
    private static bool CheckIfNull(Node node)
    {
        if (node == null)
        {
            return false;
        }
        return true;
    }

    private static bool Check(Node node)
    {
        return CheckIfNull(node) && CheckForChildren(node);
    }

    /// <summary>
    /// Searches parent's children for the first child it founds that's of type <typeparamref name="T"/>
    /// </summary>
    public static T GetChildWithNodeType<T>(Node parent) where T : Node
    {

        if (!Check(parent)) return default;

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
    /// Searches parent's children for all children that's of type <typeparamref name="T"/>
    /// </summary>
    public static T[] GetChildrenWithNodeType<T>(Node parent) where T : Node
    {
        if (!Check(parent)) return default;

        List<T> result = [];

        foreach (Node child in parent.GetChildren())
        {
            if (child is T foundChild)
            {
                result.Add(foundChild);
            }

            var recursiveResult = GetChildrenWithNodeType<T>(child);

            if (recursiveResult != null)
            {
                return recursiveResult;
            }
        }
        return result.Count > 0 ? [.. result] : default;
    }

    /// <summary>
    /// Searches parent's children for any node that's named <paramref name="name"/>
    /// </summary>
    public static T GetChildWithName<T>(string name, Node parent, bool caseSensitive = false) where T : Node
    {
        if (!Check(parent)) return default;

        if (caseSensitive)
        {
            foreach (var child in parent.GetChildren())
            {
                if (child.Name == name && child is T TChild) return TChild;
            }
        }
        else
        {
            foreach (var child in parent.GetChildren())
            {
                if (child.Name.ToString().Equals(name, StringComparison.CurrentCultureIgnoreCase) && child is T TChild) return TChild;
            }
        }


        GD.PushWarning($"Couldn't find child with name {name} in {parent}");
        return default;
    }

    /// <summary>
    /// Searches for all the children that's inside the parent recursively within given type
    /// </summary>
    public static List<T> GetAllChildren<T>(Node parent) where T : Node
    {
        if (Check(parent)) return default;

        List<T> result = [];
        foreach (Node child in parent.GetChildren())
        {
            if (child is T foundChild)
            {
                result.Add(foundChild);
            }

            var recursiveResult = GetAllChildren<T>(child);

            if (recursiveResult != null)
            {
                foreach (var item in recursiveResult)
                {
                    result.Add(item);
                }
            }
        }
        if (result.Count > 0)
        {
            return result;
        }
        else
        {
            GD.PrintErr("Couldn't find any child.");
            return default;
        }
    }
}

