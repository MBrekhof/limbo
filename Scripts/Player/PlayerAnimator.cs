using Godot;

namespace Limbo;

public partial class PlayerAnimator : Node2D
{
    private PlayerController _player = null!;
    private float _animTimer;
    private int _walkFrame;
    private float _breatheTimer;
    private float _blinkTimer;
    private Vector2 _squash = Vector2.One;
    private AnimState _prevState = AnimState.Idle;
    private GpuParticles2D _footDust = null!;

    // Character dimensions (matching collision shape: 24x56).
    // Child proportions: oversized head, narrow sloped shoulders, short tapered limbs.
    private const float HeadRadius = 8.5f;
    private const float HeadY = -19f;
    private const float FeetY = 28f;
    private const float BlinkInterval = 4.2f;
    private const float BlinkDuration = 0.13f;

    // The game's single saturated accent — reserved for the player's cap (see CLAUDE.md Visual Style).
    private static readonly Color CapColor = new(0.95f, 0.76f, 0.12f);

    public override void _Ready()
    {
        _player = GetParent<PlayerController>();

        // Foot dust lives on the player (not this node) so it ignores flip/squash.
        _footDust = CreateFootDust();
        _player.CallDeferred(Node.MethodName.AddChild, _footDust);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _animTimer += dt;
        _breatheTimer += dt;
        _blinkTimer += dt;
        if (_blinkTimer > BlinkInterval)
            _blinkTimer = 0;

        if (_player.CurrentAnimState == AnimState.Walk)
        {
            if (_animTimer > 0.12f)
            {
                _walkFrame = (_walkFrame + 1) % 4;
                _animTimer = 0;
                if (_walkFrame % 2 == 0)
                    EmitFootDust(0.4f);
            }
        }
        else
        {
            _walkFrame = 0;
            _animTimer = 0;
        }

        // Squash & stretch impulses on state transitions.
        AnimState state = _player.CurrentAnimState;
        if (state != _prevState)
        {
            if (state == AnimState.Jump)
            {
                _squash = new Vector2(0.94f, 1.08f);
            }
            else if (_prevState == AnimState.Fall && state is AnimState.Idle or AnimState.Walk)
            {
                _squash = new Vector2(1.12f, 0.88f);
                EmitFootDust(1f);
            }
            _prevState = state;
        }
        _squash = _squash.Lerp(Vector2.One, 1f - Mathf.Exp(-10f * dt));

        Scale = new Vector2((_player.FacingDirection >= 0 ? 1 : -1) * _squash.X, _squash.Y);
        // Keep the feet planted while squashing (scale pivots at the body center).
        Position = new Vector2(0, FeetY * (1f - _squash.Y));

        QueueRedraw();
    }

    private void EmitFootDust(float amountRatio)
    {
        if (!_footDust.IsInsideTree())
            return;
        _footDust.AmountRatio = amountRatio;
        _footDust.Restart();
    }

    private static GpuParticles2D CreateFootDust()
    {
        var mat = new ParticleProcessMaterial
        {
            Direction = new Vector3(0, -1, 0),
            Spread = 70f,
            Gravity = new Vector3(0, -20, 0),
            InitialVelocityMin = 15f,
            InitialVelocityMax = 40f,
            ScaleMin = 0.05f,
            ScaleMax = 0.12f,
            Color = new Color(0.45f, 0.45f, 0.45f, 0.5f),
        };
        return new GpuParticles2D
        {
            Position = new Vector2(0, FeetY - 2),
            OneShot = true,
            Emitting = false,
            Explosiveness = 1f,
            Amount = 8,
            Lifetime = 0.45f,
            LocalCoords = false,
            ProcessMaterial = mat,
            Texture = SignatureParticles.MakeRadialDot(32),
        };
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

    /// <summary>Two glowing eyes on the facing side of the head; halo + core reads as glow without bloom.</summary>
    private void DrawEyes(float headX, float headY)
    {
        if (_blinkTimer < BlinkDuration)
            return;

        var halo = new Color(0.92f, 0.94f, 0.97f, 0.35f);
        var core = new Color(0.92f, 0.94f, 0.97f);
        Vector2 back = new(headX + 2.6f, headY - 1.4f);
        Vector2 front = new(headX + 6.0f, headY - 1.4f);
        DrawCircle(back, 1.9f, halo);
        DrawCircle(front, 1.7f, halo);
        DrawCircle(back, 1.05f, core);
        DrawCircle(front, 0.95f, core);
    }

    /// <summary>Yellow cap (the single accent color) hugging the top of the head, plus hair wisps at the back.</summary>
    private void DrawCapAndHair(float x, float y)
    {
        // Hair wisps poking out under the cap's back edge (drawn first, behind the cap).
        DrawPoly(Colors.Black,
            new(x - 7.5f, y - 2.5f), new(x - 11f, y - 1f), new(x - 8f, y + 0.5f),
            new(x - 9.5f, y + 2.5f), new(x - 7f, y + 2f));
        // Dome sitting slightly proud of the scalp (head circle chord is at y - 3).
        DrawPoly(CapColor,
            new(x - 8.1f, y - 3f), new(x - 7.4f, y - 5.8f), new(x - 5.2f, y - 8.2f),
            new(x - 2f, y - 9.6f), new(x + 2f, y - 9.6f), new(x + 5.2f, y - 8.2f),
            new(x + 7.4f, y - 5.8f), new(x + 8.1f, y - 3f));
        // Brim pointing the way the player faces.
        DrawQuad(CapColor, new(x + 6.5f, y - 4.2f), new(x + 12.5f, y - 3.6f),
                           new(x + 12.5f, y - 2.4f), new(x + 6.5f, y - 2.6f));
    }

    // -- IDLE --

    private void DrawIdle(Color color)
    {
        float b = Mathf.Sin(_breatheTimer * 2f) * 0.5f;
        const float hx = 1.5f; // hunch: head sits forward of the spine

        DrawHead(color, hx, HeadY + b);
        DrawQuad(color, new(-0.5f, -12f + b), new(3.5f, -12f + b),
                        new(3f, -9f + b), new(0f, -9f + b));               // Neck (mostly behind head)
        DrawPoly(color, new(-4f, -10.5f + b), new(5f, -10.5f + b), new(5.5f, -6f + b),
                        new(4f, 10f + b), new(-4f, 10f + b), new(-4.8f, -6f + b)); // Torso, sloped shoulders
        DrawQuad(color, new(-4.2f, -9f + b), new(-6.2f, -9f + b),
                        new(-6.8f, 7f + b), new(-5.4f, 7f + b));           // Left arm (tapered)
        DrawQuad(color, new(5f, -9f + b), new(7f, -9f + b),
                        new(7.4f, 7f + b), new(6f, 7f + b));               // Right arm (tapered)
        DrawQuad(color, new(-3.6f, 10f), new(-0.8f, 10f),
                        new(-1.2f, 26f), new(-3.4f, 26f));                 // Left leg (short, tapered)
        DrawRect(color, -4.5f, 26f, 0f, 28f);                              // Left foot
        DrawQuad(color, new(0.8f, 10f), new(3.6f, 10f),
                        new(3.4f, 26f), new(1.2f, 26f));                   // Right leg
        DrawRect(color, 0.5f, 26f, 5f, 28f);                               // Right foot
        DrawCapAndHair(hx, HeadY + b);
        DrawEyes(hx, HeadY + b);
    }

    // -- WALK --

    private void DrawWalk(Color color, int frame)
    {
        float bob = (frame == 1 || frame == 3) ? -1f : 0f;
        const float hx = 2.5f; // stronger forward lean while moving

        DrawHead(color, hx, HeadY + bob);
        DrawQuad(color, new(0.5f, -12f + bob), new(4.5f, -12f + bob),
                        new(4f, -9f + bob), new(1f, -9f + bob));           // Neck
        DrawPoly(color, new(-3f, -10.5f + bob), new(6f, -10.5f + bob), new(6f, -6f + bob),
                        new(4.5f, 10f + bob), new(-3.5f, 10f + bob), new(-4f, -6f + bob)); // Torso leaning in

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

        DrawCapAndHair(hx, HeadY + bob);
        DrawEyes(hx, HeadY + bob);
    }

    private void DrawWalkPassing(Color color, float bob)
    {
        DrawQuad(color, new(-3f, 10f), new(-0.5f, 10f), new(-1f, 25f), new(-3f, 25f));   // Left leg
        DrawRect(color, -4f, 25f, 1f, 28f);                                              // Left foot
        DrawQuad(color, new(1f, 10f), new(3.5f, 10f), new(3f, 25f), new(1.2f, 25f));     // Right leg
        DrawRect(color, 0f, 25f, 5f, 28f);                                               // Right foot
        DrawQuad(color, new(-3.2f, -9f + bob), new(-5.2f, -9f + bob),
                        new(-6f, 3f + bob), new(-4.6f, 3f + bob));                       // Left arm
        DrawQuad(color, new(6f, -9f + bob), new(8f, -9f + bob),
                        new(8.2f, 3f + bob), new(6.6f, 3f + bob));                       // Right arm
    }

    private void DrawWalkStride(Color color, float bob, bool forward)
    {
        if (forward)
        {
            // Left leg forward
            DrawPoly(color, new(-3f, 10f), new(0f, 10f), new(3f, 18f), new(0f, 26f), new(-3f, 26f), new(-3f, 18f));
            DrawQuad(color, new(-3f, 26f), new(2f, 26f), new(2f, 28f), new(-4f, 28f));
            // Right leg back
            DrawPoly(color, new(1f, 10f), new(4f, 10f), new(2f, 18f), new(-1f, 26f), new(-4f, 26f), new(0f, 18f));
            DrawRect(color, -5f, 26f, 0f, 28f);
            // Left arm back
            DrawQuad(color, new(-3.2f, -9f + bob), new(-5.2f, -9f + bob),
                            new(-8f, 2f + bob), new(-5.6f, 2f + bob));
            // Right arm forward
            DrawQuad(color, new(6f, -9f + bob), new(8f, -9f + bob),
                            new(10.5f, -2f + bob), new(7.5f, -2f + bob));
        }
        else
        {
            // Right leg forward
            DrawPoly(color, new(1f, 10f), new(4f, 10f), new(7f, 18f), new(4f, 26f), new(1f, 26f), new(1f, 18f));
            DrawQuad(color, new(1f, 26f), new(6f, 26f), new(6f, 28f), new(0f, 28f));
            // Left leg back
            DrawPoly(color, new(-3f, 10f), new(0f, 10f), new(-2f, 18f), new(-5f, 26f), new(-7f, 26f), new(-4f, 18f));
            DrawRect(color, -8f, 26f, -3f, 28f);
            // Right arm back
            DrawQuad(color, new(6f, -9f + bob), new(8f, -9f + bob),
                            new(5f, 2f + bob), new(3f, 2f + bob));
            // Left arm forward
            DrawQuad(color, new(-3.2f, -9f + bob), new(-5.2f, -9f + bob),
                            new(-2.5f, -2f + bob), new(-0.5f, -2f + bob));
        }
    }

    // -- JUMP --

    private void DrawJump(Color color)
    {
        DrawHead(color, 1f, HeadY - 1);
        DrawQuad(color, new(-0.5f, -13f), new(3.5f, -13f), new(3f, -10f), new(0f, -10f)); // Neck
        DrawQuad(color, new(-4f, -11.5f), new(5f, -11.5f), new(4f, 6f), new(-3.5f, 6f));  // Torso
        // Arms reaching up (tapered)
        DrawQuad(color, new(-4f, -11f), new(-6f, -11f), new(-8.5f, -20f), new(-6.8f, -20f));
        DrawQuad(color, new(5f, -11f), new(7f, -11f), new(9.5f, -20f), new(7.8f, -20f));
        // Left leg tucked
        DrawPoly(color, new(-3.5f, 6f), new(-0.5f, 6f), new(-0.5f, 13f), new(-5.5f, 19f), new(-7.5f, 17f), new(-3.5f, 11f));
        DrawRect(color, -8.5f, 17f, -4.5f, 19f);
        // Right leg tucked
        DrawPoly(color, new(0.5f, 6f), new(3.5f, 6f), new(3.5f, 11f), new(7.5f, 17f), new(5.5f, 19f), new(0.5f, 13f));
        DrawRect(color, 4.5f, 17f, 8.5f, 19f);
        DrawCapAndHair(1f, HeadY - 1);
        DrawEyes(1f, HeadY - 1);
    }

    // -- FALL --

    private void DrawFall(Color color)
    {
        DrawHead(color, 0.5f, HeadY);
        DrawQuad(color, new(-1f, -12.5f), new(3f, -12.5f), new(2.5f, -9.5f), new(-0.5f, -9.5f)); // Neck
        DrawQuad(color, new(-4.2f, -10.5f), new(5f, -10.5f), new(4f, 11f), new(-3.8f, 11f));     // Torso
        // Left arm — out and dangling
        DrawQuad(color, new(-4.2f, -10.5f), new(-6.2f, -10.5f), new(-11f, -5.5f), new(-9.2f, -3.5f));
        DrawQuad(color, new(-11f, -5.5f), new(-9.2f, -3.5f), new(-10f, 2.5f), new(-12f, 0.5f));
        // Right arm — out and dangling
        DrawQuad(color, new(5f, -10.5f), new(7f, -10.5f), new(11.8f, -5.5f), new(10f, -3.5f));
        DrawQuad(color, new(11.8f, -5.5f), new(10f, -3.5f), new(11f, 2.5f), new(13f, 0.5f));
        // Left leg dangling
        DrawQuad(color, new(-3.8f, 11f), new(-1f, 11f), new(-2.6f, 20f), new(-5.4f, 20f));
        DrawQuad(color, new(-5.4f, 20f), new(-2.6f, 20f), new(-1.8f, 28f), new(-4.6f, 28f));
        DrawRect(color, -5.8f, 26f, -1.2f, 28f);
        // Right leg dangling
        DrawQuad(color, new(1f, 11f), new(3.8f, 11f), new(5.4f, 20f), new(2.6f, 20f));
        DrawQuad(color, new(2.6f, 20f), new(5.4f, 20f), new(4.6f, 28f), new(1.8f, 28f));
        DrawRect(color, 1.2f, 26f, 5.8f, 28f);
        DrawCapAndHair(0.5f, HeadY);
        DrawEyes(0.5f, HeadY);
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
        // Cap knocked off, lying upside-down behind the body.
        DrawPoly(CapColor, new(-17f, 28f), new(-15.5f, 24.5f), new(-12.5f, 23.5f),
                           new(-10f, 25f), new(-9.5f, 28f));
    }
}
