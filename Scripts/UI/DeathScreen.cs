using Godot;

namespace Limbo;

public partial class DeathScreen : CanvasLayer
{
    private ColorRect _overlay;

    public override void _Ready()
    {
        Layer = 8;

        _overlay = GetNode<ColorRect>("Overlay");

        GameManager.Instance.PlayerDied += OnPlayerDied;
        GameManager.Instance.PlayerRespawned += OnPlayerRespawned;
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied -= OnPlayerDied;
            GameManager.Instance.PlayerRespawned -= OnPlayerRespawned;
        }
    }

    private void OnPlayerDied()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(_overlay, "color:a", 0.8f, 0.3f);
    }

    private void OnPlayerRespawned()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(_overlay, "color:a", 0.0f, 0.5f);
    }
}
