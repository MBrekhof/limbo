using Godot;

namespace Limbo;

public interface IInteractable
{
    void Interact(Node2D interactor);
}
