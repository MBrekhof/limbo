using Godot;

namespace Limbo;

public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }

    [Signal]
    public delegate void PlayerDiedEventHandler();

    [Signal]
    public delegate void PlayerRespawnedEventHandler();

    [Signal]
    public delegate void LevelTransitionRequestedEventHandler(string levelPath);

    public Vector2 LastCheckpointPosition { get; set; } = Vector2.Zero;
    public Vector2 LevelStartPosition { get; set; } = Vector2.Zero;

    public Vector2 RespawnPosition =>
        LastCheckpointPosition != Vector2.Zero ? LastCheckpointPosition : LevelStartPosition;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            GD.PushWarning("Duplicate GameManager detected. Freeing this instance.");
            QueueFree();
            return;
        }
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public void ResetCheckpoint()
    {
        LastCheckpointPosition = Vector2.Zero;
    }

    public void OnPlayerDied()
    {
        EmitSignal(SignalName.PlayerDied);
    }

    public void OnPlayerRespawned()
    {
        EmitSignal(SignalName.PlayerRespawned);
    }

    public void RequestLevelTransition(string levelPath)
    {
        EmitSignal(SignalName.LevelTransitionRequested, levelPath);
    }
}
