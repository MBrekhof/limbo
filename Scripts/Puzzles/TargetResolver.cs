using Godot;
using System.Collections.Generic;

namespace Limbo;

public static class TargetResolver
{
    public static IActivatable[] ResolveTargets(Node owner, NodePath[]? targets)
    {
        if (targets == null || targets.Length == 0)
            return [];

        var resolved = new List<IActivatable>();
        foreach (NodePath path in targets)
        {
            if (path == null) continue;
            Node? node = owner.GetNodeOrNull(path);
            if (node is IActivatable activatable)
            {
                resolved.Add(activatable);
            }
            else if (node != null)
            {
                GD.PushWarning($"{owner.Name}: Target '{path}' is not IActivatable.");
            }
            else
            {
                GD.PushWarning($"{owner.Name}: Target '{path}' not found.");
            }
        }
        return resolved.ToArray();
    }

    public static void SetTargets(IActivatable[] targets, bool active)
    {
        foreach (var target in targets)
        {
            if (active)
                target.Activate();
            else
                target.Deactivate();
        }
    }
}
