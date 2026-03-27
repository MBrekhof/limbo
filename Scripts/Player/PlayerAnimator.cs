using Godot;

namespace Limbo;

public partial class PlayerAnimator : Node2D
{
    private PlayerController _player = null!;
    private float _animTimer;
    private int _walkFrame;
    private float _breatheTimer;

    // Character dimensions (matching collision shape: 24x56)
    private const float HeadRadius = 7f;
    private const float HeadY = -21f;

    public override void _Ready()
    {
        _player = GetParent<PlayerController>();
    }

    public override void _Process(double delta)
    {
        _animTimer += (float)delta;
        _breatheTimer += (float)delta;

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

    // -- Shared body part helpers --

    private void DrawHead(Color color, float x, float y)
    {
        DrawCircle(new Vector2(x, y), HeadRadius, color);
    }

    private void DrawRect(Color color, float x1, float y1, float x2, float y2)
    {
        DrawPolygon(new Vector2[] { new(x1, y1), new(x2, y1), new(x2, y2), new(x1, y2) }, new Color[] { color });
    }

    private void DrawQuad(Color color, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        DrawPolygon(new Vector2[] { a, b, c, d }, new Color[] { color });
    }

    private void DrawPoly(Color color, params Vector2[] points)
    {
        DrawPolygon(points, new Color[] { color });
    }

    // -- IDLE --

    private void DrawIdle(Color color)
    {
        float b = Mathf.Sin(_breatheTimer * 2f) * 0.5f;

        DrawHead(color, 0, HeadY + b);
        DrawRect(color, -2, -14 + b, 2, -11 + b);                         // Neck
        DrawQuad(color, new(-6, -11 + b), new(6, -11 + b),
                        new(4, 8 + b), new(-4, 8 + b));                   // Torso
        DrawQuad(color, new(-6, -11 + b), new(-8, -11 + b),
                        new(-9, 4 + b), new(-6, 4 + b));                  // Left arm
        DrawQuad(color, new(6, -11 + b), new(8, -11 + b),
                        new(9, 4 + b), new(6, 4 + b));                    // Right arm
        DrawRect(color, -4, 8, -1, 26);                                    // Left leg
        DrawRect(color, -5, 26, 0, 28);                                    // Left foot
        DrawRect(color, 1, 8, 4, 26);                                      // Right leg
        DrawRect(color, 0, 26, 5, 28);                                     // Right foot
    }

    // -- WALK --

    private void DrawWalk(Color color, int frame)
    {
        float bob = (frame == 1 || frame == 3) ? -1f : 0f;

        DrawHead(color, 1, HeadY + bob);
        DrawQuad(color, new(-1, -14 + bob), new(3, -14 + bob),
                        new(2, -11 + bob), new(-1, -11 + bob));            // Neck
        DrawQuad(color, new(-5, -11 + bob), new(7, -11 + bob),
                        new(5, 8 + bob), new(-3, 8 + bob));               // Torso

        switch (frame)
        {
            case 0: // Left leg forward, right leg back
                DrawWalkStride(color, bob, forward: true);
                break;
            case 1:
            case 3: // Passing — legs together
                DrawWalkPassing(color, bob);
                break;
            case 2: // Right leg forward, left leg back
                DrawWalkStride(color, bob, forward: false);
                break;
        }
    }

    private void DrawWalkPassing(Color color, float bob)
    {
        DrawRect(color, -3, 8, 0, 25);                                     // Left leg
        DrawRect(color, -4, 25, 1, 28);                                    // Left foot
        DrawRect(color, 1, 8, 3, 25);                                      // Right leg
        DrawRect(color, 0, 25, 5, 28);                                     // Right foot
        DrawQuad(color, new(-5, -11 + bob), new(-7, -11 + bob),
                        new(-8, 2 + bob), new(-5, 2 + bob));              // Left arm
        DrawQuad(color, new(7, -11 + bob), new(9, -11 + bob),
                        new(9, 2 + bob), new(7, 2 + bob));                // Right arm
    }

    private void DrawWalkStride(Color color, float bob, bool forward)
    {
        if (forward)
        {
            // Left leg forward
            DrawPoly(color, new(-3, 8), new(0, 8), new(3, 18), new(0, 26), new(-3, 26), new(-3, 18));
            DrawQuad(color, new(-3, 26), new(2, 26), new(2, 28), new(-4, 28));
            // Right leg back
            DrawPoly(color, new(1, 8), new(4, 8), new(2, 18), new(-1, 26), new(-4, 26), new(0, 18));
            DrawRect(color, -5, 26, 0, 28);
            // Left arm back
            DrawQuad(color, new(-5, -11 + bob), new(-7, -11 + bob),
                            new(-10, 2 + bob), new(-7, 2 + bob));
            // Right arm forward
            DrawQuad(color, new(7, -11 + bob), new(9, -11 + bob),
                            new(11, -2 + bob), new(8, -2 + bob));
        }
        else
        {
            // Right leg forward
            DrawPoly(color, new(1, 8), new(4, 8), new(7, 18), new(4, 26), new(1, 26), new(1, 18));
            DrawQuad(color, new(1, 26), new(6, 26), new(6, 28), new(0, 28));
            // Left leg back
            DrawPoly(color, new(-3, 8), new(0, 8), new(-2, 18), new(-5, 26), new(-7, 26), new(-4, 18));
            DrawRect(color, -8, 26, -3, 28);
            // Right arm back
            DrawQuad(color, new(7, -11 + bob), new(9, -11 + bob),
                            new(6, 2 + bob), new(4, 2 + bob));
            // Left arm forward
            DrawQuad(color, new(-5, -11 + bob), new(-7, -11 + bob),
                            new(-4, -2 + bob), new(-2, -2 + bob));
        }
    }

    // -- JUMP --

    private void DrawJump(Color color)
    {
        DrawHead(color, 0, HeadY - 1);
        DrawRect(color, -2, -15, 2, -12);                                  // Neck
        DrawQuad(color, new(-6, -12), new(6, -12),
                        new(4, 4), new(-4, 4));                            // Torso
        // Arms reaching up
        DrawQuad(color, new(-6, -12), new(-8, -12), new(-10, -20), new(-7, -20));
        DrawQuad(color, new(6, -12), new(8, -12), new(10, -20), new(7, -20));
        // Left leg tucked
        DrawPoly(color, new(-4, 4), new(-1, 4), new(-1, 12), new(-6, 18), new(-8, 16), new(-4, 10));
        DrawRect(color, -9, 16, -5, 18);
        // Right leg tucked
        DrawPoly(color, new(1, 4), new(4, 4), new(4, 10), new(8, 16), new(6, 18), new(1, 12));
        DrawRect(color, 5, 16, 9, 18);
    }

    // -- FALL --

    private void DrawFall(Color color)
    {
        DrawHead(color, 0, HeadY);
        DrawRect(color, -2, -14, 2, -11);                                  // Neck
        DrawQuad(color, new(-6, -11), new(6, -11),
                        new(4, 10), new(-4, 10));                          // Torso
        // Left arm — out and dangling
        DrawQuad(color, new(-6, -11), new(-8, -11), new(-12, -6), new(-10, -4));
        DrawQuad(color, new(-12, -6), new(-10, -4), new(-11, 2), new(-13, 0));
        // Right arm — out and dangling
        DrawQuad(color, new(6, -11), new(8, -11), new(12, -6), new(10, -4));
        DrawQuad(color, new(12, -6), new(10, -4), new(11, 2), new(13, 0));
        // Left leg dangling
        DrawQuad(color, new(-4, 10), new(-1, 10), new(-3, 20), new(-6, 20));
        DrawQuad(color, new(-6, 20), new(-3, 20), new(-2, 28), new(-5, 28));
        DrawRect(color, -6, 26, -1, 28);
        // Right leg dangling
        DrawQuad(color, new(1, 10), new(4, 10), new(6, 20), new(3, 20));
        DrawQuad(color, new(3, 20), new(6, 20), new(5, 28), new(2, 28));
        DrawRect(color, 1, 26, 6, 28);
    }

    // -- DEATH --

    private void DrawDeath(Color color)
    {
        DrawHead(color, 6, 22);
        // Body crumpled
        DrawQuad(color, new(-8, 18), new(2, 16), new(4, 22), new(-6, 24));
        DrawQuad(color, new(2, 16), new(6, 15), new(6, 22), new(4, 22));
        // Arms
        DrawQuad(color, new(-8, 18), new(-12, 16), new(-12, 19), new(-8, 21));
        DrawQuad(color, new(4, 18), new(8, 20), new(8, 23), new(4, 22));
        // Legs
        DrawQuad(color, new(-8, 22), new(-5, 22), new(-6, 28), new(-9, 28));
        DrawQuad(color, new(-2, 24), new(1, 24), new(4, 28), new(0, 28));
        // Feet
        DrawRect(color, -10, 26, -5, 28);
        DrawRect(color, 2, 26, 7, 28);
    }
}
