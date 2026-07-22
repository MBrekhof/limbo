using Godot;

namespace Limbo;

/// <summary>
/// Per-level atmosphere settings, applied to the EnvironmentTemplate children.
/// Levels override these exports on their Environment instance node.
/// </summary>
public partial class AtmosphereController : Node
{
    [Export] public Color AmbientColor { get; set; } = new(0.6f, 0.65f, 0.7f);
    [Export] public float FogDensity { get; set; } = 0.3f;
    [Export] public float GodRayStrength { get; set; } = 0.35f;
    [Export] public ParticleMode Signature { get; set; } = ParticleMode.Fireflies;

    public override void _Ready()
    {
        // Children run _Ready before the parent, so values set here win.
        if (GetNodeOrNull<CanvasModulate>("AmbientLight") is { } ambient)
            ambient.Color = AmbientColor;

        if (GetNodeOrNull<FogLayer>("FogOverlay") is { } fog)
            fog.Density = FogDensity;

        if (GetNodeOrNull<ColorRect>("GodRays/GodRaysRect")?.Material is ShaderMaterial rays)
            rays.SetShaderParameter("intensity", GodRayStrength);

        if (GetNodeOrNull<SignatureParticles>("SignatureParticlesLayer/SignatureParticles") is { } particles)
            particles.Configure(Signature);
    }
}
