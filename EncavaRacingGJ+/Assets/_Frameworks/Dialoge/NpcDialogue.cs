using Framework.Core;
using UnityEngine;

namespace Framework.Dialogue
{
    public class NpcDialogue : MonoBehaviour, IInteractable
    {
        [SerializeField] DialogueData dialogue;
        [SerializeField] string prompt = "Hablar";

        public string Prompt => prompt;

        public void Interact(GameObject who) => EventBus.Publish(new DialogueRequested(dialogue));
    }
}