using Godot;

namespace Limbo;

public partial class PressurePlate : Area2D
{
    [Export] public NodePath[] Targets { get; set; } = System.Array.Empty<NodePath>();

    private int _bodyCount;
    private bool _isActive;
    private ColorRect _visual;

    public override void _Ready()
    {
        _visual = GetNode<ColorRect>("Visual");

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupPlayer) || body.IsInGroup(Constants.GroupPushable))
        {
            _bodyCount++;
            if (_bodyCount == 1)
            {
                SetActive(true);
            }
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupPlayer) || body.IsInGroup(Constants.GroupPushable))
        {
            _bodyCount--;
            if (_bodyCount <= 0)
            {
                _bodyCount = 0;
                SetActive(false);
            }
        }
    }

    private void SetActive(bool active)
    {
        if (_isActive == active)
            return;

        _isActive = active;

        // Visual feedback: compress when active
        if (_visual != null)
        {
            _visual.Scale = _isActive ? new Vector2(1.0f, 0.5f) : new Vector2(1.0f, 1.0f);
        }

        // Notify all targets
        foreach (NodePath targetPath in Targets)
        {
            if (targetPath == null)
                continue;

            Node targetNode = GetNode(targetPath);
            if (targetNode is IActivatable activatable)
            {
                if (_isActive)
                    activatable.Activate();
                else
                    activatable.Deactivate();
            }
        }
    }
}
