using EIODE.Components;
using Godot;
using System;

namespace EIODE.Utils
{
    public static class ComponentsUtils
    {
        public static void CreateHealthComponent(Node parent, int maxHealth, float critMultiplier, bool canRegenerate = false, float regenerationDelay = 0f, float regenerationRate = 0f)
        {
            HealthComponent healthComponent = new(maxHealth, critMultiplier, canRegenerate, regenerationDelay, regenerationRate);
            healthComponent.Reparent(parent);
        }

        public static void CreateHealthComponent(Node parent, HealthComponent healthComponent)
        {
            healthComponent.Reparent(parent);
        }

        public static T GetChildWithComponent<T>(Node parent) where T : IComponent
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

                // Recursively search through children
                var recursiveResult = GetChildWithComponent<T>(child);
                if (recursiveResult != null)
                {
                    return recursiveResult;
                }
            }
            return default;
        }
    }
}
