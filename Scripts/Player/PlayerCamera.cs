using Godot;

namespace Limbo;

public partial class PlayerCamera : Camera2D
{
    [Export] public float LookAheadDistance { get; set; } = 50f;
    [Export] public float SmoothingSpeed { get; set; } = 5f;
    [Export] public float LimitBottomValue { get; set; } = 1500f;

    private float _currentLookAhead;

    public override void _Ready()
    {
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = SmoothingSpeed;
        LimitBottom = (int)LimitBottomValue;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // Get the parent's velocity for look-ahead direction.
        float targetLookAhead = 0f;
        if (GetParent() is CharacterBody2D body)
        {
            float velocityX = body.Velocity.X;
            if (Mathf.Abs(velocityX) > 10f)
            {
                targetLookAhead = Mathf.Sign(velocityX) * LookAheadDistance;
            }
        }

        // Smoothly interpolate the look-ahead offset.
        _currentLookAhead = Mathf.Lerp(_currentLookAhead, targetLookAhead, dt * SmoothingSpeed);
        Offset = new Vector2(_currentLookAhead, Offset.Y);
    }
}
