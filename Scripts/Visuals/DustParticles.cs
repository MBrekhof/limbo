using Godot;

namespace Limbo;

public partial class DustParticles : GpuParticles2D
{
    [Export] public int ParticleCount { get; set; } = 50;

    public override void _Ready()
    {
        Amount = ParticleCount;
        Lifetime = 8.0f;
        Preprocess = 4.0f;

        if (ProcessMaterial is ParticleProcessMaterial material)
        {
            material.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
            material.EmissionBoxExtents = new Vector3(960f, 540f, 1f);
            material.Direction = new Vector3(0f, -1f, 0f);
            material.Spread = 180f;
            material.Gravity = Vector3.Zero;
            material.InitialVelocityMin = 5f;
            material.InitialVelocityMax = 15f;
            material.ScaleMin = 0.5f;
            material.ScaleMax = 1.5f;
        }
    }
}
