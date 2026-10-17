using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Dialogue
{
    /// <summary>
    /// Nodo 0: "Guardia": "¡Alto! ¿Qué buscas?"
    ///   Opciones:
    ///     "Busco la salida"   → nodo 1
    ///     "Nada, ya me voy"   → nodo 2
    /// Nodo 1: "Guardia": "Sigue el pasillo."          next = -1  (fin)
    /// Nodo 2: "Guardia": "Más te vale."               next = -1  (fin)
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialog", menuName = "Framework/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        public List<DialogueNode> nodes = new(); // el nodo 0 es el inicio
    }
    
    [Serializable] // Hace que Unity la muestre y guarde en el inspector dentro del SO
    public class DialogueNode
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
        public int next = -1; // índice del siguiente nodo. -1 = fin. Se ignora si hay opciones
        public List<DialogueChoice> choices = new();
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public int next = -1; // nodo al que lleva esta opción. -1 = fin
    }
}