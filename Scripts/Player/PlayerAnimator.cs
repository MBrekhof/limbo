using Godot;

namespace Limbo;

public partial class PlayerAnimator : Node2D
{
    private PlayerController _player = null!;
    private float _animTimer;
    private int _walkFrame;
    private float _breatheTimer;

    // Character dimensions (matching collision shape: 24x56)
    private const float Width = 24f;
    private const float Height = 56f;
    private const float HeadRadius = 7f;
    private const float HeadY = -21f; // center of head near top of body

    public override void _Ready()
    {
        _player = GetParent<PlayerController>();
    }

    public override void _Process(double delta)
    {
        _animTimer += (float)delta;
        _breatheTimer += (float)delta;

        // Walk frame cycling
        if (_player.CurrentAnimState == AnimState.Walk)
        {
            if (_animTimer > 0.12f)
            {
                _walkFrame = (_walkFrame + 1) % 4;
                _animTimer = 0;
            }
        }
        else
        {
            _walkFrame = 0;
            _animTimer = 0;
        }

        // Flip based on facing direction
        Scale = new Vector2(_player.FacingDirection >= 0 ? 1 : -1, 1);

        QueueRedraw();
    }

    public override void _Draw()
    {
        var color = Colors.Black;

        switch (_player.CurrentAnimState)
        {
            case AnimState.Idle:
                DrawIdle(color);
                break;
            case AnimState.Walk:
                DrawWalk(color, _walkFrame);
                break;
            case AnimState.Jump:
                DrawJump(color);
                break;
            case AnimState.Fall:
                DrawFall(color);
                break;
            case AnimState.Death:
                DrawDeath(color);
                break;
        }
    }

    // ----------------------------------------------------------------
    //  IDLE — standing still with subtle breathing
    // ----------------------------------------------------------------
    private void DrawIdle(Color color)
    {
        float breathe = Mathf.Sin(_breatheTimer * 2f) * 0.5f;

        // Head
        DrawCircle(new Vector2(0, HeadY + breathe), HeadRadius, color);

        // Neck
        DrawPolygon(new Vector2[]
        {
            new(-2, -14 + breathe),
            new( 2, -14 + breathe),
            new( 2, -11 + breathe),
            new(-2, -11 + breathe),
        }, new Color[] { color });

        // Body — torso trapezoid (shoulders wide, waist narrower)
        DrawPolygon(new Vector2[]
        {
            new(-6, -11 + breathe),
            new( 6, -11 + breathe),
            new( 4,   8 + breathe),
            new(-4,   8 + breathe),
        }, new Color[] { color });

        // Left arm — hanging at side
        DrawPolygon(new Vector2[]
        {
            new(-6, -11 + breathe),
            new(-8, -11 + breathe),
            new(-9,   4 + breathe),
            new(-6,   4 + breathe),
        }, new Color[] { color });

        // Right arm — hanging at side
        DrawPolygon(new Vector2[]
        {
            new(6, -11 + breathe),
            new(8, -11 + breathe),
            new(9,   4 + breathe),
            new(6,   4 + breathe),
        }, new Color[] { color });

        // Left leg
        DrawPolygon(new Vector2[]
        {
            new(-4, 8),
            new(-1, 8),
            new(-1, 26),
            new(-4, 26),
        }, new Color[] { color });

        // Left foot
        DrawPolygon(new Vector2[]
        {
            new(-5, 26),
            new( 0, 26),
            new( 0, 28),
            new(-5, 28),
        }, new Color[] { color });

        // Right leg
        DrawPolygon(new Vector2[]
        {
            new(1, 8),
            new(4, 8),
            new(4, 26),
            new(1, 26),
        }, new Color[] { color });

        // Right foot
        DrawPolygon(new Vector2[]
        {
            new(0, 26),
            new(5, 26),
            new(5, 28),
            new(0, 28),
        }, new Color[] { color });
    }

    // ----------------------------------------------------------------
    //  WALK — 4-frame cycle with arm/leg swing
    // ----------------------------------------------------------------
    private void DrawWalk(Color color, int frame)
    {
        // Slight body bob
        float bob = (frame == 1 || frame == 3) ? -1f : 0f;

        // Head
        DrawCircle(new Vector2(1, HeadY + bob), HeadRadius, color);

        // Neck (leaning forward slightly)
        DrawPolygon(new Vector2[]
        {
            new(-1, -14 + bob),
            new( 3, -14 + bob),
            new( 2, -11 + bob),
            new(-1, -11 + bob),
        }, new Color[] { color });

        // Body — slight forward lean
        DrawPolygon(new Vector2[]
        {
            new(-5, -11 + bob),
            new( 7, -11 + bob),
            new( 5,   8 + bob),
            new(-3,   8 + bob),
        }, new Color[] { color });

        // Legs and arms depend on frame
        switch (frame)
        {
            case 0: // Left leg forward, right leg back (stride)
                // Left leg — forward
                DrawPolygon(new Vector2[]
                {
                    new(-3, 8),
                    new( 0, 8),
                    new( 3, 18),
                    new( 0, 26),
                    new(-3, 26),
                    new(-3, 18),
                }, new Color[] { color });
                // Left foot
                DrawPolygon(new Vector2[]
                {
                    new(-3, 26),
                    new( 2, 26),
                    new( 2, 28),
                    new(-4, 28),
                }, new Color[] { color });

                // Right leg — back
                DrawPolygon(new Vector2[]
                {
                    new(1, 8),
                    new(4, 8),
                    new(2, 18),
                    new(-1, 26),
                    new(-4, 26),
                    new( 0, 18),
                }, new Color[] { color });
                // Right foot
                DrawPolygon(new Vector2[]
                {
                    new(-5, 26),
                    new( 0, 26),
                    new( 0, 28),
                    new(-5, 28),
                }, new Color[] { color });

                // Left arm — swings back
                DrawPolygon(new Vector2[]
                {
                    new(-5, -11 + bob),
                    new(-7, -11 + bob),
                    new(-10,  2 + bob),
                    new(-7,   2 + bob),
                }, new Color[] { color });

                // Right arm — swings forward
                DrawPolygon(new Vector2[]
                {
                    new(7, -11 + bob),
                    new(9, -11 + bob),
                    new(11, -2 + bob),
                    new( 8, -2 + bob),
                }, new Color[] { color });
                break;

            case 1: // Passing — legs together, slightly bent
                // Left leg
                DrawPolygon(new Vector2[]
                {
                    new(-3, 8),
                    new( 0, 8),
                    new( 0, 25),
                    new(-3, 25),
                }, new Color[] { color });
                // Left foot
                DrawPolygon(new Vector2[]
                {
                    new(-4, 25),
                    new( 1, 25),
                    new( 1, 28),
                    new(-4, 28),
                }, new Color[] { color });

                // Right leg
                DrawPolygon(new Vector2[]
                {
                    new(1, 8),
                    new(3, 8),
                    new(3, 25),
                    new(1, 25),
                }, new Color[] { color });
                // Right foot
                DrawPolygon(new Vector2[]
                {
                    new(0, 25),
                    new(5, 25),
                    new(5, 28),
                    new(0, 28),
                }, new Color[] { color });

                // Arms — neutral
                DrawPolygon(new Vector2[]
                {
                    new(-5, -11 + bob),
                    new(-7, -11 + bob),
                    new(-8,   2 + bob),
                    new(-5,   2 + bob),
                }, new Color[] { color });
                DrawPolygon(new Vector2[]
                {
                    new(7, -11 + bob),
                    new(9, -11 + bob),
                    new(9,   2 + bob),
                    new(7,   2 + bob),
                }, new Color[] { color });
                break;

            case 2: // Right leg forward, left leg back (stride)
                // Right leg — forward
                DrawPolygon(new Vector2[]
                {
                    new(1, 8),
                    new(4, 8),
                    new(7, 18),
                    new(4, 26),
                    new(1, 26),
                    new(1, 18),
                }, new Color[] { color });
                // Right foot
                DrawPolygon(new Vector2[]
                {
                    new(1, 26),
                    new(6, 26),
                    new(6, 28),
                    new(0, 28),
                }, new Color[] { color });

                // Left leg — back
                DrawPolygon(new Vector2[]
                {
                    new(-3, 8),
                    new( 0, 8),
                    new(-2, 18),
                    new(-5, 26),
                    new(-7, 26),
                    new(-4, 18),
                }, new Color[] { color });
                // Left foot
                DrawPolygon(new Vector2[]
                {
                    new(-8, 26),
                    new(-3, 26),
                    new(-3, 28),
                    new(-8, 28),
                }, new Color[] { color });

                // Right arm — swings back
                DrawPolygon(new Vector2[]
                {
                    new(7, -11 + bob),
                    new(9, -11 + bob),
                    new(6,   2 + bob),
                    new(4,   2 + bob),
                }, new Color[] { color });

                // Left arm — swings forward
                DrawPolygon(new Vector2[]
                {
                    new(-5, -11 + bob),
                    new(-7, -11 + bob),
                    new(-4, -2 + bob),
                    new(-2, -2 + bob),
                }, new Color[] { color });
                break;

            case 3: // Passing — legs together
                // Same as frame 1
                DrawPolygon(new Vector2[]
                {
                    new(-3, 8),
                    new( 0, 8),
                    new( 0, 25),
                    new(-3, 25),
                }, new Color[] { color });
                DrawPolygon(new Vector2[]
                {
                    new(-4, 25),
                    new( 1, 25),
                    new( 1, 28),
                    new(-4, 28),
                }, new Color[] { color });
                DrawPolygon(new Vector2[]
                {
                    new(1, 8),
                    new(3, 8),
                    new(3, 25),
                    new(1, 25),
                }, new Color[] { color });
                DrawPolygon(new Vector2[]
                {
                    new(0, 25),
                    new(5, 25),
                    new(5, 28),
                    new(0, 28),
                }, new Color[] { color });

                // Arms — neutral
                DrawPolygon(new Vector2[]
                {
                    new(-5, -11 + bob),
                    new(-7, -11 + bob),
                    new(-8,   2 + bob),
                    new(-5,   2 + bob),
                }, new Color[] { color });
                DrawPolygon(new Vector2[]
                {
                    new(7, -11 + bob),
                    new(9, -11 + bob),
                    new(9,   2 + bob),
                    new(7,   2 + bob),
                }, new Color[] { color });
                break;
        }
    }

    // ----------------------------------------------------------------
    //  JUMP — tucked legs, arms reaching upward
    // ----------------------------------------------------------------
    private void DrawJump(Color color)
    {
        // Head
        DrawCircle(new Vector2(0, HeadY - 1), HeadRadius, color);

        // Neck
        DrawPolygon(new Vector2[]
        {
            new(-2, -15),
            new( 2, -15),
            new( 2, -12),
            new(-2, -12),
        }, new Color[] { color });

        // Body — slightly compressed
        DrawPolygon(new Vector2[]
        {
            new(-6, -12),
            new( 6, -12),
            new( 4,   4),
            new(-4,   4),
        }, new Color[] { color });

        // Left arm — reaching up
        DrawPolygon(new Vector2[]
        {
            new(-6, -12),
            new(-8, -12),
            new(-10, -20),
            new(-7, -20),
        }, new Color[] { color });

        // Right arm — reaching up
        DrawPolygon(new Vector2[]
        {
            new(6, -12),
            new(8, -12),
            new(10, -20),
            new( 7, -20),
        }, new Color[] { color });

        // Left leg — tucked up, knee bent
        DrawPolygon(new Vector2[]
        {
            new(-4, 4),
            new(-1, 4),
            new(-1, 12),
            new(-6, 18),
            new(-8, 16),
            new(-4, 10),
        }, new Color[] { color });

        // Left foot tucked
        DrawPolygon(new Vector2[]
        {
            new(-9, 16),
            new(-5, 16),
            new(-5, 18),
            new(-9, 18),
        }, new Color[] { color });

        // Right leg — tucked up, knee bent
        DrawPolygon(new Vector2[]
        {
            new(1, 4),
            new(4, 4),
            new(4, 10),
            new(8, 16),
            new(6, 18),
            new(1, 12),
        }, new Color[] { color });

        // Right foot tucked
        DrawPolygon(new Vector2[]
        {
            new(5, 16),
            new(9, 16),
            new(9, 18),
            new(5, 18),
        }, new Color[] { color });
    }

    // ----------------------------------------------------------------
    //  FALL — dangling legs, arms out for balance
    // ----------------------------------------------------------------
    private void DrawFall(Color color)
    {
        // Head
        DrawCircle(new Vector2(0, HeadY), HeadRadius, color);

        // Neck
        DrawPolygon(new Vector2[]
        {
            new(-2, -14),
            new( 2, -14),
            new( 2, -11),
            new(-2, -11),
        }, new Color[] { color });

        // Body — slightly stretched
        DrawPolygon(new Vector2[]
        {
            new(-6, -11),
            new( 6, -11),
            new( 4,  10),
            new(-4,  10),
        }, new Color[] { color });

        // Left arm — out to the side and slightly up
        DrawPolygon(new Vector2[]
        {
            new(-6, -11),
            new(-8, -11),
            new(-12, -6),
            new(-10, -4),
        }, new Color[] { color });
        // Left forearm dangling down
        DrawPolygon(new Vector2[]
        {
            new(-12, -6),
            new(-10, -4),
            new(-11,  2),
            new(-13,  0),
        }, new Color[] { color });

        // Right arm — out to the side and slightly up
        DrawPolygon(new Vector2[]
        {
            new(6, -11),
            new(8, -11),
            new(12, -6),
            new(10, -4),
        }, new Color[] { color });
        // Right forearm dangling down
        DrawPolygon(new Vector2[]
        {
            new(12, -6),
            new(10, -4),
            new(11,  2),
            new(13,  0),
        }, new Color[] { color });

        // Left leg — dangling, slightly apart and bent
        DrawPolygon(new Vector2[]
        {
            new(-4, 10),
            new(-1, 10),
            new(-3, 20),
            new(-6, 20),
        }, new Color[] { color });
        // Left lower leg — slight kick
        DrawPolygon(new Vector2[]
        {
            new(-6, 20),
            new(-3, 20),
            new(-2, 28),
            new(-5, 28),
        }, new Color[] { color });
        // Left foot
        DrawPolygon(new Vector2[]
        {
            new(-6, 26),
            new(-1, 26),
            new(-1, 28),
            new(-6, 28),
        }, new Color[] { color });

        // Right leg — dangling, slightly apart
        DrawPolygon(new Vector2[]
        {
            new(1, 10),
            new(4, 10),
            new(6, 20),
            new(3, 20),
        }, new Color[] { color });
        // Right lower leg
        DrawPolygon(new Vector2[]
        {
            new(3, 20),
            new(6, 20),
            new(5, 28),
            new(2, 28),
        }, new Color[] { color });
        // Right foot
        DrawPolygon(new Vector2[]
        {
            new(1, 26),
            new(6, 26),
            new(6, 28),
            new(1, 28),
        }, new Color[] { color });
    }

    // ----------------------------------------------------------------
    //  DEATH — collapsed crumpled figure on the ground
    // ----------------------------------------------------------------
    private void DrawDeath(Color color)
    {
        // Head — fallen to the right, resting near ground
        DrawCircle(new Vector2(6, 22), HeadRadius, color);

        // Body — horizontal / crumpled on ground
        DrawPolygon(new Vector2[]
        {
            new(-8, 18),
            new( 2, 16),
            new( 4, 22),
            new(-6, 24),
        }, new Color[] { color });

        // Torso extension toward head
        DrawPolygon(new Vector2[]
        {
            new(2, 16),
            new(6, 15),
            new(6, 22),
            new(4, 22),
        }, new Color[] { color });

        // Left arm — sprawled out
        DrawPolygon(new Vector2[]
        {
            new(-8, 18),
            new(-12, 16),
            new(-12, 19),
            new(-8, 21),
        }, new Color[] { color });

        // Right arm — crumpled near body
        DrawPolygon(new Vector2[]
        {
            new(4, 18),
            new(8, 20),
            new(8, 23),
            new(4, 22),
        }, new Color[] { color });

        // Left leg — crumpled behind body
        DrawPolygon(new Vector2[]
        {
            new(-8, 22),
            new(-5, 22),
            new(-6, 28),
            new(-9, 28),
        }, new Color[] { color });

        // Right leg — sprawled forward
        DrawPolygon(new Vector2[]
        {
            new(-2, 24),
            new( 1, 24),
            new( 4, 28),
            new( 0, 28),
        }, new Color[] { color });

        // Left foot
        DrawPolygon(new Vector2[]
        {
            new(-10, 26),
            new(-5, 26),
            new(-5, 28),
            new(-10, 28),
        }, new Color[] { color });

        // Right foot
        DrawPolygon(new Vector2[]
        {
            new(2, 26),
            new(7, 26),
            new(7, 28),
            new(2, 28),
        }, new Color[] { color });
    }
}
