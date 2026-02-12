using Godot;

namespace Limbo;

public partial class Door : StaticBody2D, IActivatable
{
    [Export] public float OpenOffset { get; set; } = -128.0f;
    [Export] public float OpenSpeed { get; set; } = 2.0f;

    public bool IsActive { get; private set; }

    private Vector2 _closedPosition;
    private Vector2 _openPosition;
    private Tween _tween;
    private CollisionShape2D _collisionShape;

    public override void _Ready()
    {
        _closedPosition = Position;
        _openPosition = Position + new Vector2(0, OpenOffset);
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape");
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AnimateDoor(_openPosition, true);
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AnimateDoor(_closedPosition, false);
    }

    private void AnimateDoor(Vector2 targetPosition, bool disableCollision)
    {
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(this, "position", targetPosition, 1.0 / OpenSpeed);

        if (disableCollision && _collisionShape != null)
        {
            _tween.TweenCallback(Callable.From(() =>
            {
                _collisionShape.Disabled = true;
            }));
        }
        else if (!disableCollision && _collisionShape != null)
        {
            _collisionShape.Disabled = false;
        }
    }
}
