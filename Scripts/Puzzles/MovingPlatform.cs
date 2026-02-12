using Godot;

namespace Limbo;

public partial class MovingPlatform : AnimatableBody2D, IActivatable
{
    [Export] public Vector2[] Waypoints { get; set; } = System.Array.Empty<Vector2>();
    [Export] public float Speed { get; set; } = 100.0f;
    [Export] public bool AutoStart { get; set; } = true;

    public bool IsActive { get; private set; }

    private int _currentWaypointIndex;
    private bool _movingForward = true;
    private bool _moving;
    private Vector2 _startPosition;

    public override void _Ready()
    {
        SyncToPhysics = true;
        _startPosition = Position;
        _moving = AutoStart;
        IsActive = AutoStart;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_moving || Waypoints == null || Waypoints.Length == 0)
            return;

        Vector2 targetLocal = Waypoints[_currentWaypointIndex];
        Vector2 targetGlobal = _startPosition + targetLocal;
        Vector2 direction = (targetGlobal - Position).Normalized();
        float distance = Position.DistanceTo(targetGlobal);
        float step = Speed * (float)delta;

        if (step >= distance)
        {
            Position = targetGlobal;
            AdvanceWaypoint();
        }
        else
        {
            Position += direction * step;
        }
    }

    private void AdvanceWaypoint()
    {
        if (_movingForward)
        {
            _currentWaypointIndex++;
            if (_currentWaypointIndex >= Waypoints.Length)
            {
                _currentWaypointIndex = Waypoints.Length - 1;
                _movingForward = false;

                // If only one waypoint, just stop or reverse to start
                if (Waypoints.Length <= 1)
                {
                    _currentWaypointIndex = 0;
                }
            }
        }
        else
        {
            _currentWaypointIndex--;
            if (_currentWaypointIndex < 0)
            {
                _currentWaypointIndex = 0;
                _movingForward = true;
            }
        }
    }

    public void Activate()
    {
        IsActive = true;
        _moving = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        _moving = false;
    }
}
