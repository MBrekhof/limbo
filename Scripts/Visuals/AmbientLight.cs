using Godot;

namespace Limbo;

public partial class AmbientLight : CanvasModulate
{
    [Export] public Color AmbientColor { get; set; } = new Color(0.6f, 0.65f, 0.7f, 1.0f);

    public override void _Ready()
    {
        Color = AmbientColor;
    }
}
