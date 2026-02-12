namespace Limbo;

public static class Constants
{
    // Input actions (defined in project.godot)
    public const string InputMoveLeft = "move_left";
    public const string InputMoveRight = "move_right";
    public const string InputJump = "jump";
    public const string InputInteract = "interact";

    // Physics layers (1-indexed, matching project.godot)
    public const int LayerEnvironment = 1;
    public const int LayerPlayer = 2;
    public const int LayerPuzzles = 3;
    public const int LayerHazards = 4;
    public const int LayerTriggers = 5;

    // Physics layer bit masks (0-indexed for Godot API)
    public const uint MaskEnvironment = 1 << 0;
    public const uint MaskPlayer = 1 << 1;
    public const uint MaskPuzzles = 1 << 2;
    public const uint MaskHazards = 1 << 3;
    public const uint MaskTriggers = 1 << 4;

    // Node groups
    public const string GroupPlayer = "player";
    public const string GroupHazard = "hazard";
    public const string GroupPushable = "pushable";
    public const string GroupInteractable = "interactable";
    public const string GroupCheckpoint = "checkpoint";

    // Scene paths
    public const string ScenePlayer = "res://Scenes/Player/Player.tscn";
    public const string SceneLevel01 = "res://Scenes/Levels/Level01_Forest.tscn";
    public const string SceneLevel02 = "res://Scenes/Levels/Level02_Industrial.tscn";
    public const string SceneLevel03 = "res://Scenes/Levels/Level03_Depths.tscn";
    public const string SceneEnvironmentTemplate = "res://Scenes/Environment/EnvironmentTemplate.tscn";
}
