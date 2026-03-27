using Godot;

namespace Limbo;

public partial class LeverSwitch : StaticBody2D, IInteractable
{
    [Export] public NodePath[] Targets { get; set; } = [];

    private bool _isOn;
    private ColorRect? _visual;
    private IActivatable[] _resolvedTargets = [];

    public override void _Ready()
    {
        AddToGroup(Constants.GroupInteractable);
        _visual = GetNodeOrNull<ColorRect>("Visual");
        _resolvedTargets = TargetResolver.ResolveTargets(this, Targets);
    }

    public void Interact(Node2D interactor)
    {
        _isOn = !_isOn;

        if (_visual != null)
        {
            _visual.RotationDegrees = _isOn ? 45.0f : 0.0f;
        }

        TargetResolver.SetTargets(_resolvedTargets, _isOn);
    }
}
