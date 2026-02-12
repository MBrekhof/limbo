using Godot;

namespace Limbo;

public partial class PushableBox : RigidBody2D
{
    public override void _Ready()
    {
        AddToGroup(Constants.GroupPushable);
    }
}
