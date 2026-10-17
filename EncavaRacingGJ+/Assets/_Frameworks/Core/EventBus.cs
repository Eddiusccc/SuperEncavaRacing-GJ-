using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    /// <summary>
    /// Centraliza la comunicación entre sistemas mediante publicación y suscripción de eventos.
    /// </summary>
    public static class EventBus
    {
        // Diccionario que almacena callbacks por tipo de evento.
        static readonly Dictionary<Type, Delegate> handlers = new();

        /// <summary>
        /// Suscribe un listener para un tipo de evento estructurado.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">Action o funcion que se suscribe</param>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            handlers.TryGetValue(type, out var existing);
            handlers[type] = Delegate.Combine(existing, handler);
        }

        /// <summary>
        /// Desuscribe un listener.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">Action o función que se desuscribe</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!handlers.TryGetValue(type, out var existing)) return;

            var result = Delegate.Remove(existing, handler);
            if (result == null) handlers.Remove(type);
            else handlers[type] = result;
        }

        /// <summary>
        /// Publica un evento estructurado.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt">Evento a publicar</param>
        public static void Publish<T>(T evt) where T : struct
        {
            if (handlers.TryGetValue(typeof(T), out var d)) (d as Action<T>)?.Invoke(evt);
        }

        /* 
        indica a Unity que debe ejecutar el método Reset() durante la 
        inicialización de los subsistemas, antes de que comience la ejecución normal del juego.
        */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => handlers.Clear();
    }
}