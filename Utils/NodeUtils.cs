using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public static T GetChildWithNodeType<T>(Node parent, bool recursive = false) where T : Node
    {

        if (!Check(parent)) return default;

        foreach (Node child in parent.GetChildren())
        {
            if (child is T foundChild)
            {
                return foundChild;
            }

            if (recursive)
            {
                var recursiveResult = GetChildWithNodeType<T>(child);

                if (recursiveResult != null)
                {
                    return recursiveResult;
                }
            }
        }

        return default;
    }

    public static bool GetChildWithNodeType<T>(Node parent, out T found, bool recursive = false) where T : Node
    {
        if (GetChildWithNodeType<T>(parent) != null)
        {
            found = GetChildWithNodeType<T>(parent, recursive);
            return true;
        }

        found = null;
        return false;
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

    public static Array<T> SortByDistance<[MustBeVariant] T>(Array<T> nodes, Vector3 globalPosition) where T : Node3D
    {
        T[] array = nodes.ToArray();

        System.Array.Sort(array, (x, y) =>
        {
            float distX = x.GlobalPosition.DistanceSquaredTo(globalPosition);
            float distY = y.GlobalPosition.DistanceSquaredTo(globalPosition);
            return distX.CompareTo(distY);
        });

        Array<T> result = [];
        foreach (T node in array)
        {
            result.Add(node);
        }

        return result;
    }

    public static Array<T> RemoveDuplicates<[MustBeVariant] T>(Array<T> array)
    {
        if (array.Count == 0) return array;

        List<T> tList = [];
        int tListCount = tList.Count;

        for (int i = 0; i < tListCount; i++)
        {
            for (int j = i + 1; j < tListCount; j++)
            {
                if (tList[i].Equals(tList[j]))
                {
                    tList.RemoveAt(i);
                    j--;
                }
            }
        }

        return ToGodotArray(tList);
    }

    public static Array<T> ToGodotArray<[MustBeVariant] T>(List<T> list)
    {
        Array<T> result = [];
        foreach (var item in list)
        {
            result.Add(item);
        }
        return result;
    }

    /// <summary>
    /// Modifies given array instead of returning a new one, uses less memory 
    /// </summary>
    public static void SortByDistanceModified<[MustBeVariant] T>(ref Array<T> nodes, Vector3 globalPosition) where T : Node3D
    {
        T[] array = nodes.ToArray();
        System.Array.Sort(array, (x, y) =>
        {
            float distX = x.GlobalPosition.DistanceSquaredTo(globalPosition);
            float distY = y.GlobalPosition.DistanceSquaredTo(globalPosition);
            return distX.CompareTo(distY);
        });

        nodes = [.. array];
    }

}

