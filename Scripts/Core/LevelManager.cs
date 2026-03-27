using Godot;

namespace Limbo;

public partial class LevelManager : Node
{
    public static LevelManager? Instance { get; private set; }

    private CanvasLayer _fadeLayer = null!;
    private ColorRect _fadeRect = null!;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            GD.PushWarning("Duplicate LevelManager detected. Freeing this instance.");
            QueueFree();
            return;
        }
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        // Create the fade overlay.
        _fadeLayer = new CanvasLayer();
        _fadeLayer.Layer = 10;
        AddChild(_fadeLayer);

        _fadeRect = new ColorRect();
        _fadeRect.Color = new Color(0, 0, 0, 0);
        _fadeRect.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        _fadeLayer.AddChild(_fadeRect);

        // Connect to GameManager's level transition signal.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelTransitionRequested += OnLevelTransitionRequested;
        }
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelTransitionRequested -= OnLevelTransitionRequested;
        }
    }

    private async void OnLevelTransitionRequested(string levelPath)
    {
        try
        {
            // Fade to black.
            Tween fadeOut = CreateTween();
            fadeOut.TweenProperty(_fadeRect, "color:a", 1.0f, 0.5f);
            await ToSignal(fadeOut, Tween.SignalName.Finished);

            if (!IsInsideTree()) return;

            // Reset checkpoint for the new level.
            GameManager.Instance?.ResetCheckpoint();

            // Change scene.
            GetTree().ChangeSceneToFile(levelPath);

            // Wait one frame for the new scene to initialize.
            await ToSignal(GetTree(), SceneTree.SignalName.TreeChanged);
            if (!IsInsideTree()) return;

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            if (!IsInsideTree()) return;

            // Find PlayerSpawn marker in the new level and set start position.
            Node root = GetTree().CurrentScene;
            if (root != null)
            {
                Marker2D? spawn = root.GetNodeOrNull<Marker2D>("PlayerSpawn");
                if (spawn != null && GameManager.Instance != null)
                {
                    GameManager.Instance.LevelStartPosition = spawn.GlobalPosition;
                }
            }

            // Fade from black.
            Tween fadeIn = CreateTween();
            fadeIn.TweenProperty(_fadeRect, "color:a", 0.0f, 0.5f);
            await ToSignal(fadeIn, Tween.SignalName.Finished);
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Level transition failed: {ex.Message}");
        }
    }
}
