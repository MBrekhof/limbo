using Godot;

namespace Limbo;

public partial class SwayEffect : Node2D
{
    [Export] public float SwayAmount { get; set; } = 3.0f;
    [Export] public float SwaySpeed { get; set; } = 1.5f;

    private float _timeAccumulator;
    private float _timeOffset;

    public override void _Ready()
    {
        _timeOffset = (float)GD.RandRange(0, Mathf.Tau);
    }

    public override void _Process(double delta)
    {
        _timeAccumulator += (float)delta;
        RotationDegrees = Mathf.Sin(_timeAccumulator * SwaySpeed + _timeOffset) * SwayAmount;
    }
}
