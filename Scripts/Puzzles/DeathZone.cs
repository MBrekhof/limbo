using Godot;

namespace Limbo;

public partial class DeathZone : Area2D
{
    public enum DeathZoneType
    {
        Spikes,
        SawBlade,
        Crusher
    }

    [Export] public DeathZoneType Type { get; set; } = DeathZoneType.Spikes;

    public override void _Ready()
    {
        AddToGroup(Constants.GroupHazard);
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupPlayer))
        {
            GameManager.Instance.OnPlayerDied();
        }
    }
}
