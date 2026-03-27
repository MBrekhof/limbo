using Godot;

namespace Limbo;

public partial class FallingPlatform : StaticBody2D
{
    [Export] public float ShakeTime { get; set; } = 0.5f;
    [Export] public float RespawnDelay { get; set; } = 3.0f;
    [Export] public float FallSpeed { get; set; } = 400.0f;

    private Vector2 _originalPosition;
    private bool _triggered;
    private bool _falling;
    private float _shakeTimer;
    private float _respawnTimer;
    private CollisionShape2D? _collisionShape;
    private ColorRect? _visual;
    private Area2D _detectionArea = null!;

    public override void _Ready()
    {
        _originalPosition = Position;
        _collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape");
        _visual = GetNodeOrNull<ColorRect>("Visual");
        _detectionArea = GetNodeOrNull<Area2D>("DetectionArea")!;

        _detectionArea.BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!_triggered && body.IsInGroup(Constants.GroupPlayer))
        {
            _triggered = true;
            _shakeTimer = ShakeTime;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_triggered && !_falling)
        {
            _shakeTimer -= (float)delta;

            // Shake effect
            float shakeOffset = (float)GD.RandRange(-2.0, 2.0);
            Position = new Vector2(_originalPosition.X + shakeOffset, _originalPosition.Y);

            if (_shakeTimer <= 0.0f)
            {
                _falling = true;
                Position = _originalPosition;
            }
        }
        else if (_falling)
        {
            Position += new Vector2(0, FallSpeed * (float)delta);

            // Check if fallen off-screen
            if (Position.Y > _originalPosition.Y + 1000.0f)
            {
                _falling = false;
                _triggered = false;
                SetVisualVisible(false);
                if (_collisionShape != null)
                    _collisionShape.Disabled = true;
                _detectionArea.Monitoring = false;
                _respawnTimer = RespawnDelay;
            }
        }
        else if (_respawnTimer > 0.0f)
        {
            _respawnTimer -= (float)delta;
            if (_respawnTimer <= 0.0f)
            {
                Respawn();
            }
        }
    }

    private void Respawn()
    {
        Position = _originalPosition;
        SetVisualVisible(true);
        if (_collisionShape != null)
            _collisionShape.Disabled = false;
        _detectionArea.Monitoring = true;
    }

    private void SetVisualVisible(bool visible)
    {
        if (_visual != null)
            _visual.Visible = visible;
    }
}
