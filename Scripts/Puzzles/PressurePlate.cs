using Godot;

namespace Limbo;

public partial class PressurePlate : Area2D
{
    [Export] public NodePath[] Targets { get; set; } = [];

    private int _bodyCount;
    private bool _isActive;
    private ColorRect? _visual;
    private IActivatable[] _resolvedTargets = [];

    public override void _Ready()
    {
        _visual = GetNodeOrNull<ColorRect>("Visual");
        _resolvedTargets = TargetResolver.ResolveTargets(this, Targets);

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

        if (_visual != null)
        {
            _visual.Scale = _isActive ? new Vector2(1.0f, 0.5f) : new Vector2(1.0f, 1.0f);
        }

        TargetResolver.SetTargets(_resolvedTargets, _isActive);
    }
}
