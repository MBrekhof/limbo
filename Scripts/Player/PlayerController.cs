using Godot;

namespace Limbo;

public partial class PlayerController : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 300f;
    [Export] public float Acceleration { get; set; } = 2000f;
    [Export] public float Friction { get; set; } = 1800f;
    [Export] public float AirControlMultiplier { get; set; } = 0.6f;
    [Export] public float JumpVelocity { get; set; } = -500f;
    [Export] public float GravityScale { get; set; } = 1f;
    [Export] public float CoyoteTime { get; set; } = 0.1f;
    [Export] public float JumpBufferTime { get; set; } = 0.1f;
    [Export] public float FallDeathThreshold { get; set; } = 2000f;

    private float _gravity;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool _isDead;
    private Area2D _hazardDetector;

    public override void _Ready()
    {
        AddToGroup(Constants.GroupPlayer);
        _gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");

        _hazardDetector = GetNode<Area2D>("HazardDetector");
        _hazardDetector.BodyEntered += OnHazardBodyEntered;
        _hazardDetector.AreaEntered += OnHazardAreaEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDead)
            return;

        float dt = (float)delta;
        Vector2 velocity = Velocity;

        // Apply gravity.
        if (!IsOnFloor())
        {
            velocity.Y += _gravity * GravityScale * dt;
        }

        // Track coyote time.
        if (IsOnFloor())
        {
            _coyoteTimer = CoyoteTime;
        }
        else
        {
            _coyoteTimer -= dt;
        }

        // Track jump buffer.
        if (Input.IsActionJustPressed(Constants.InputJump))
        {
            _jumpBufferTimer = JumpBufferTime;
        }
        else
        {
            _jumpBufferTimer -= dt;
        }

        // Jump: consume both coyote time and jump buffer.
        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            velocity.Y = JumpVelocity;
            _coyoteTimer = 0f;
            _jumpBufferTimer = 0f;
        }

        // Variable jump height: cut upward velocity when releasing jump early.
        if (Input.IsActionJustReleased(Constants.InputJump) && velocity.Y < 0f)
        {
            velocity.Y *= 0.5f;
        }

        // Horizontal movement with acceleration/deceleration.
        float inputDirection = Input.GetAxis(Constants.InputMoveLeft, Constants.InputMoveRight);
        float accelRate = IsOnFloor() ? 1f : AirControlMultiplier;

        if (Mathf.Abs(inputDirection) > 0.01f)
        {
            velocity.X = Mathf.MoveToward(velocity.X, inputDirection * Speed, Acceleration * accelRate * dt);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0f, Friction * accelRate * dt);
        }

        Velocity = velocity;
        MoveAndSlide();

        // Fall death check.
        if (Position.Y > FallDeathThreshold)
        {
            Die();
        }
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        Velocity = Vector2.Zero;
        SetPhysicsProcess(false);
        GameManager.Instance.OnPlayerDied();
    }

    public void Respawn(Vector2 position)
    {
        _isDead = false;
        GlobalPosition = position;
        Velocity = Vector2.Zero;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;
        SetPhysicsProcess(true);
    }

    private void OnHazardBodyEntered(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupHazard))
        {
            Die();
        }
    }

    private void OnHazardAreaEntered(Area2D area)
    {
        if (area.IsInGroup(Constants.GroupHazard))
        {
            Die();
        }
    }
}
