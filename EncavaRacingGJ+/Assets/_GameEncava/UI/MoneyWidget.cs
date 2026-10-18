using Framework.Core;
using Framework.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Widget de interfaz de usuario que muestra la cantidad actual de dinero del jugador.
/// Se actualiza automáticamente cuando se publica un evento <see cref="MoneyChanged"/>.
/// </summary>
public class MoneyWidget : EventWidget<MoneyChanged>
{
    [Tooltip("Etiqueta de texto donde se mostrará el dinero.")]
    [SerializeField] TMP_Text label;

    /// <summary>
    /// Se llama cuando se recibe un evento <see cref="MoneyChanged"/>.
    /// Actualiza el texto de la etiqueta con el nuevo valor de dinero formateado.
    /// </summary>
    /// <param name="e">Datos del evento que contienen la cantidad de dinero.</param>
    protected override void OnEvent(MoneyChanged e)
    {
        if (label != null) label.text = $"${e.Money:N0}";
    }
}
