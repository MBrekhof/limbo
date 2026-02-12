using Godot;

namespace Limbo;

public partial class ParallaxController : ParallaxBackground
{
    [Export] public float AutoScrollSpeed { get; set; } = 5f;

    public override void _Process(double delta)
    {
        // Subtle horizontal auto-scroll for distant layers (layers 1 and 2).
        // ParallaxBackground applies scroll_offset to all layers, weighted by motion_scale.
        // Since distant layers have small motion_scale, the auto-scroll effect is subtle.
        ScrollOffset += new Vector2(AutoScrollSpeed * (float)delta, 0f);
    }
}
