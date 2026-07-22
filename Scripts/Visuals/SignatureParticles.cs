using Godot;

namespace Limbo;

public enum ParticleMode { None, Fireflies, Embers, Drips }

/// <summary>
/// Screen-space ambient particle layer with one signature look per level theme.
/// Configured by AtmosphereController; all textures are generated procedurally.
/// </summary>
public partial class SignatureParticles : GpuParticles2D
{
    public void Configure(ParticleMode mode)
    {
        if (mode == ParticleMode.None)
        {
            Emitting = false;
            Visible = false;
            return;
        }

        var mat = new ParticleProcessMaterial
        {
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(1100f, 560f, 1f),
            Gravity = Vector3.Zero,
        };

        switch (mode)
        {
            case ParticleMode.Fireflies:
                Amount = 18;
                Lifetime = 9.0f;
                Texture = MakeRadialDot(64);
                Material = MakeAdditive();
                mat.Direction = new Vector3(0, -1, 0);
                mat.Spread = 180f;
                mat.InitialVelocityMin = 4f;
                mat.InitialVelocityMax = 12f;
                mat.ScaleMin = 0.08f;
                mat.ScaleMax = 0.16f;
                mat.Color = new Color(0.95f, 0.95f, 0.9f, 0.5f);
                mat.ColorRamp = MakeFadeInOutRamp();
                break;

            case ParticleMode.Embers:
                Amount = 30;
                Lifetime = 6.0f;
                Texture = MakeRadialDot(32);
                Material = MakeAdditive();
                mat.Direction = new Vector3(0, -1, 0);
                mat.Spread = 25f;
                mat.InitialVelocityMin = 20f;
                mat.InitialVelocityMax = 55f;
                mat.Gravity = new Vector3(4f, -10f, 0f);
                mat.ScaleMin = 0.05f;
                mat.ScaleMax = 0.12f;
                mat.Color = new Color(0.85f, 0.8f, 0.75f, 0.45f);
                mat.ColorRamp = MakeFadeInOutRamp();
                break;

            case ParticleMode.Drips:
                Amount = 10;
                Lifetime = 1.6f;
                Texture = MakeDripStreak();
                mat.Direction = new Vector3(0, 1, 0);
                mat.Spread = 2f;
                mat.InitialVelocityMin = 180f;
                mat.InitialVelocityMax = 260f;
                mat.Gravity = new Vector3(0f, 500f, 0f);
                mat.ScaleMin = 0.7f;
                mat.ScaleMax = 1.1f;
                mat.Color = new Color(0.7f, 0.75f, 0.8f, 0.35f);
                break;
        }

        ProcessMaterial = mat;
        Preprocess = 5.0f;
        Emitting = true;
    }

    /// <summary>Soft white dot: opaque center fading to a transparent edge.</summary>
    public static GradientTexture2D MakeRadialDot(int size)
    {
        var grad = new Gradient();
        grad.SetColor(0, new Color(1, 1, 1, 1));
        grad.SetColor(1, new Color(1, 1, 1, 0));
        return new GradientTexture2D
        {
            Gradient = grad,
            Width = size,
            Height = size,
            Fill = GradientTexture2D.FillEnum.Radial,
            FillFrom = new Vector2(0.5f, 0.5f),
            FillTo = new Vector2(0.5f, 0f),
        };
    }

    private static GradientTexture2D MakeDripStreak()
    {
        var grad = new Gradient();
        grad.SetColor(0, new Color(1, 1, 1, 0));
        grad.SetColor(1, new Color(1, 1, 1, 0.8f));
        return new GradientTexture2D
        {
            Gradient = grad,
            Width = 3,
            Height = 28,
            Fill = GradientTexture2D.FillEnum.Linear,
            FillFrom = new Vector2(0.5f, 0f),
            FillTo = new Vector2(0.5f, 1f),
        };
    }

    private static GradientTexture1D MakeFadeInOutRamp()
    {
        var grad = new Gradient();
        grad.SetColor(0, new Color(1, 1, 1, 0));
        grad.SetColor(1, new Color(1, 1, 1, 0));
        grad.AddPoint(0.25f, new Color(1, 1, 1, 1));
        grad.AddPoint(0.75f, new Color(1, 1, 1, 1));
        return new GradientTexture1D { Gradient = grad };
    }

    private static CanvasItemMaterial MakeAdditive() =>
        new() { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
}
