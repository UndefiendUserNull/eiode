using Godot;
using System;
using System.Collections.Generic;

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
            return default;
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
    /// <typeparam name="T">Child Node Type</typeparam>
    /// <returns></returns>
    public static T GetChildWithName<T>(string name, Node parent, bool caseSensitive = false) where T : Node
    {
        if (parent == null)
        {
            GD.PushError("Couldn't find parent " + nameof(parent));
            return default;
        }

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


        GD.PushError($"Couldn't find child with name {name} in {parent}");
        return default;
    }

    /// <summary>
    /// Searches for all the children that's inside the parent recursively within given type
    /// </summary>
    /// <typeparam name="T">The type node you're searching for</typeparam>
    /// <param name="parent">The parent :D</param>
    /// <returns></returns>
    public static List<T> GetAllChildren<T>(Node parent) where T : Node
    {
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

