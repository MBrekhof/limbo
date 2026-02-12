using Godot;

namespace Limbo;

public partial class RespawnManager : Node
{
    [Export] public float RespawnDelay { get; set; } = 1.0f;

    private PlayerController _player;

    public override void _Ready()
    {
        _player = GetParent<PlayerController>();
        GameManager.Instance.PlayerDied += OnPlayerDied;
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied -= OnPlayerDied;
        }
    }

    private async void OnPlayerDied()
    {
        await ToSignal(GetTree().CreateTimer(RespawnDelay), SceneTreeTimer.SignalName.Timeout);

        Vector2 respawnPosition = GameManager.Instance.RespawnPosition;
        _player.Respawn(respawnPosition);
        GameManager.Instance.OnPlayerRespawned();
    }
}
