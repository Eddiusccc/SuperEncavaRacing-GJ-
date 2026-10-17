using Framework.Core;
using UnityEngine;

namespace Framework.UI
{
    /// <summary>
    /// Como todos los widgets hacen lo mismo (suscribirse al activarse y 
    /// desuscribirse al desactivarse), lo escribimos una sola vez.
    /// 
    /// Los eventos son structs.
    /// 
    /// Trampa: el widget nunca debe desactivar el GameObject donde vive su propio script.
    /// al hacerlo se desuscribe (OnDisable).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EventWidget<T> : MonoBehaviour where T : struct
    {
        protected virtual void OnEnable() => EventBus.Subscribe<T>(OnEvent);
        protected virtual void OnDisable() => EventBus.Unsubscribe<T>(OnEvent);

        protected abstract void OnEvent(T evt);
    }
}