using Godot;

namespace Limbo;

public partial class Checkpoint : Area2D
{
    public override void _Ready()
    {
        AddToGroup(Constants.GroupCheckpoint);
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupPlayer))
        {
            GameManager.Instance.LastCheckpointPosition = GlobalPosition;
        }
    }
}
