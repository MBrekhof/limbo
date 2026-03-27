using Godot;

namespace Limbo;

public partial class FogLayer : CanvasLayer
{
    [Export] public float ScrollSpeedX { get; set; } = 20f;
    [Export] public float ScrollSpeedY { get; set; } = 5f;
    [Export] public float Density { get; set; } = 0.3f;

    private ColorRect? _fogRect1;
    private ColorRect? _fogRect2;

    public override void _Ready()
    {
        Layer = 5;

        _fogRect1 = GetNodeOrNull<ColorRect>("FogRect1");
        _fogRect2 = GetNodeOrNull<ColorRect>("FogRect2");

        ApplyShaderParams(_fogRect1, 1.0f);
        ApplyShaderParams(_fogRect2, 0.6f);
    }

    public override void _Process(double delta)
    {
        ApplyShaderParams(_fogRect1, 1.0f);
        ApplyShaderParams(_fogRect2, 0.6f);
    }

    private void ApplyShaderParams(ColorRect? rect, float speedMultiplier)
    {
        if (rect == null)
            return;

        var material = rect.Material as ShaderMaterial;
        if (material == null)
            return;

        material.SetShaderParameter("scroll_speed",
            new Vector2(ScrollSpeedX * speedMultiplier, ScrollSpeedY * speedMultiplier));
        material.SetShaderParameter("density", Density);
    }

}
