using Godot;

namespace Limbo;

public partial class Checkpoint : Area2D
{
    [Export] public Color ActiveColor { get; set; } = new Color(0.4f, 0.4f, 0.4f, 1f);

    private bool _activated;
    private ColorRect? _visual;

    public override void _Ready()
    {
        AddToGroup(Constants.GroupCheckpoint);
        _visual = GetNodeOrNull<ColorRect>("Visual");
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupPlayer))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LastCheckpointPosition = GlobalPosition;
            }

            if (!_activated)
            {
                _activated = true;
                ShowActivation();
            }
        }
    }

    private void ShowActivation()
    {
        if (_visual == null) return;

        // Brief flash then settle to active color
        Tween tween = CreateTween();
        tween.TweenProperty(_visual, "color", new Color(1f, 1f, 1f, 0.8f), 0.1f);
        tween.TweenProperty(_visual, "color", ActiveColor, 0.4f);
    }
}
