using Godot;

namespace Limbo;

public partial class LevelTransition : Area2D
{
    [Export] public string NextLevelPath { get; set; } = "";

    public override void _Ready()
    {
        // Layer 5 (Triggers), mask 2 (Player).
        CollisionLayer = Constants.MaskTriggers;
        CollisionMask = Constants.MaskPlayer;

        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupPlayer))
        {
            GameManager.Instance?.RequestLevelTransition(NextLevelPath);
        }
    }
}
