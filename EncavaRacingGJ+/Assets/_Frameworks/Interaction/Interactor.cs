using Framework.Core;
using Framework.Input;
using UnityEngine;

namespace Framework.Interaction
{
    /// <summary>
    /// Montaje: añade Interactor al Player, arrastra el InputReader. 
    /// Los objetos interactuables (pickups, NPCs, puertas) necesitan un Collider. 
    /// Y el InteractPromptWidget del módulo 5 mostrará el texto.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] float radius = 2f;
        [SerializeField] LayerMask mask = ~0;

        [Tooltip("si es null usa este mismo transform")]
        [SerializeField] Transform origin;

        readonly Collider[] hits = new Collider[8]; // maximo almacenado
        IInteractable current;
        void OnEnable() => input.InteractPressed += OnInteract;
        void OnDisable() => input.InteractPressed -= OnInteract;

        string currentPrompt;

        void Update()
        {
            // Sin gameplay (diálogo, pausa, menú) no hay prompt
            if (!input.GameplayEnable)
            {
                if (current != null)
                {
                    current = null;
                    currentPrompt = null;
                    EventBus.Publish(new InteractPromptChanged(null));
                }
                return;
            }

            IInteractable best = FindBest();
            string prompt = best != null ? best.Prompt : null;

            // Republica si cambió el objeto O su texto
            if (best == current && prompt == currentPrompt) return;

            current = best;
            currentPrompt = prompt;
            EventBus.Publish(new InteractPromptChanged(prompt));
        }
        void OnInteract()
        {
            // Volvemos a buscar: el objeto en caché pudo destruirse
            IInteractable target = FindBest();

            target?.Interact(gameObject);
        }

        IInteractable FindBest()
        {
            Vector3 center = (origin != null ? origin : transform).position;
            
            // Busca colliders dentro de una esfera. "NonAlloc" rellena un array que ya tienes
            // y devuelve cuantos encotró sin generar basura de memoria.
            int count = Physics.OverlapSphereNonAlloc(center, radius, hits, mask, QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                // out IInteractable pide explicitamente que el componente sea la interfaz
                if (!hits[i].TryGetComponent(out IInteractable candidate)) continue;

                float d = (hits[i].transform.position - center).sqrMagnitude;

                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = candidate;
                }
            }
            Debug.Log($"Interactuar con: {best}");
            return best;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere( (origin != null ? origin : transform).position, radius);
        }
    }
}