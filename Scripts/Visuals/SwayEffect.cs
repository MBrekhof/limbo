using Godot;

namespace Limbo;

public partial class SwayEffect : Node2D
{
    [Export] public float SwayAmount = 3.0f; // degrees
    [Export] public float SwaySpeed = 1.5f;

    private float _timeOffset;

    public override void _Ready()
    {
        _timeOffset = (float)GD.RandRange(0, Mathf.Tau);
    }

    public override void _Process(double delta)
    {
        RotationDegrees = Mathf.Sin((float)Time.GetTicksMsec() / 1000.0f * SwaySpeed + _timeOffset) * SwayAmount;
    }
}
