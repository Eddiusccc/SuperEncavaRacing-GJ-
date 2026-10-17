using UnityEngine;

namespace Framework.Core
{
    public interface IInteractable
    {
    string Prompt { get; } // Texto: "Abrir puerta"
    void Interact(GameObject who); // quien interactua
    }

}

