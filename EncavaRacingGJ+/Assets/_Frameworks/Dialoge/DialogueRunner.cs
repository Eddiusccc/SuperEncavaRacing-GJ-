using System.Linq;
using Framework.Core;
using Framework.Input;
using UnityEngine;

namespace Framework.Dialogue
{
    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] InputReader input;

        DialogueData data;
        int index;

        public bool IsRunning { get; private set; }

        void OnEnable()
        {
            EventBus.Subscribe<DialogueRequested>(OnRequested);
            EventBus.Subscribe<DialogueAdvanceRequested>(OnAdvance);
            EventBus.Subscribe<DialogueChoiceSelected>(OnChoice);
        }
        void OnDisable()
        {
            EventBus.Unsubscribe<DialogueRequested>(OnRequested);
            EventBus.Unsubscribe<DialogueAdvanceRequested>(OnAdvance);
            EventBus.Unsubscribe<DialogueChoiceSelected>(OnChoice);
        }

        void OnRequested(DialogueRequested e) => StartDialogue(e.Data);

        void OnAdvance(DialogueAdvanceRequested _)
        {
            if (!IsRunning) return;
            var node = data.nodes[index];
            if (node.choices.Count > 0) return; // con opciones, hay que elegir
            Go(node.next);
        }

        void OnChoice(DialogueChoiceSelected e)
        {
            if (!IsRunning) return;
            var node = data.nodes[index];
            if (e.Index < 0 || e.Index >= node.choices.Count) return;
            Go(node.choices[e.Index].next);
        }

        void StartDialogue(DialogueData d)
        {
            if (IsRunning || d == null || d.nodes.Count == 0) return;

            data = d;
            IsRunning = true;
            input.DisableGameplay(); // congela movimiento y libera el cursor
            EventBus.Publish(new DialogueStarted());
            Show(0);
        }

        void Go(int next)
        {
            if (next < 0 || next >= data.nodes.Count) End();
            else Show(next);
        }

        void Show(int i)
        {
            index = i;
            var node = data.nodes[i];
            string[] choices = node.choices.Select(c => c.text).ToArray();
            EventBus.Publish(new DialogueLineShown(node.speaker, node.text, choices));
        }

        void End()
        {
            IsRunning = false;
            input.EnableGameplay();
            EventBus.Publish(new DialogueEnded());
        }
    }
}