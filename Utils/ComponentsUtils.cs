using EIODE.Components;
using Godot;
using System;

namespace EIODE.Utils
{
    public static class ComponentsUtils
    {
        /// <summary>
        /// Creates A Health Component and attaches it to a parent
        /// </summary>
        /// <param name="parent">The parent should be a Hurtbox</param>
        /// <param name="maxHealth"></param>
        /// <param name="critMultiplier"></param>
        /// <param name="canRegenerate"></param>
        /// <param name="regenerationDelay"></param>
        /// <param name="regenerationRate"></param>
        public static void CreateHealthComponent(Node parent, int maxHealth, float critMultiplier, bool canRegenerate = false, float regenerationDelay = 0f, float regenerationRate = 0f)
        {
            HealthComponent healthComponent = new(maxHealth, critMultiplier, canRegenerate, regenerationDelay, regenerationRate);
            healthComponent.Reparent(parent);
        }

        public static void CreateHealthComponent(Node parent, HealthComponent healthComponent)
        {
            healthComponent.Reparent(parent);
        }

        /// <summary>
        /// Searches in all children of the <paramref name="parent"/> for a child that is a component of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
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
