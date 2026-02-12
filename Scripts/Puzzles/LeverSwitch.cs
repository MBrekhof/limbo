using Godot;

namespace Limbo;

public partial class LeverSwitch : StaticBody2D, IInteractable
{
    [Export] public NodePath[] Targets { get; set; } = System.Array.Empty<NodePath>();

    private bool _isOn;
    private ColorRect _visual;

    public override void _Ready()
    {
        AddToGroup(Constants.GroupInteractable);
        _visual = GetNode<ColorRect>("Visual");
    }

    public void Interact(Node2D interactor)
    {
        _isOn = !_isOn;

        // Visually rotate the lever
        if (_visual != null)
        {
            _visual.RotationDegrees = _isOn ? 45.0f : 0.0f;
        }

        // Activate or deactivate all targets
        foreach (NodePath targetPath in Targets)
        {
            if (targetPath == null)
                continue;

            Node targetNode = GetNode(targetPath);
            if (targetNode is IActivatable activatable)
            {
                if (_isOn)
                    activatable.Activate();
                else
                    activatable.Deactivate();
            }
        }
    }
}
