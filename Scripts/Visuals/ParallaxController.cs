using Godot;

namespace Limbo;

/// <summary>
/// Container for Parallax2D layers. Applies auto-scroll to all child
/// Parallax2D nodes, scaled by each layer's scroll_scale.
/// </summary>
public partial class ParallaxController : Node2D
{
    [Export] public float AutoScrollSpeed { get; set; } = 5f;

    public override void _Process(double delta)
    {
        float scroll = AutoScrollSpeed * (float)delta;
        foreach (Node child in GetChildren())
        {
            if (child is Parallax2D layer)
            {
                layer.ScreenOffset += new Vector2(scroll * layer.ScrollScale.X, 0f);
            }
        }
    }
}
