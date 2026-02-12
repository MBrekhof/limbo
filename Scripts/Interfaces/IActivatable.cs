using Godot;

namespace Limbo;

public interface IActivatable
{
    bool IsActive { get; }
    void Activate();
    void Deactivate();
}
