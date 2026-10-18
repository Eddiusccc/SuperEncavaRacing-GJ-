using Framework.Core;
using TMPro;
using UnityEngine;

/// <summary>
/// Widget del HUD que muestra en pantalla la cantidad de paradas esperando recogida
/// y la cantidad de paradas de entrega pendientes.
/// </summary>
public class CityStopsWidget : MonoBehaviour
{
    [Header("Etiquetas TMP")]
    [Tooltip("Texto para mostrar las paradas que tienen pasajeros esperando subir.")]
    [SerializeField] TMP_Text pickupStopsLabel;

    [Tooltip("Texto para mostrar las paradas de destino donde deben bajarse pasajeros.")]
    [SerializeField] TMP_Text dropoffStopsLabel;

    [Header("Formato de Texto")]
    [SerializeField] string pickupPrefix = "Paradas por recoger: ";
    [SerializeField] string dropoffPrefix = "Paradas por entregar: ";

    void OnEnable()
    {
        EventBus.Subscribe<PickupStopsCountChanged>(OnPickupStopsChanged);
        EventBus.Subscribe<DropoffStopsCountChanged>(OnDropoffStopsChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<PickupStopsCountChanged>(OnPickupStopsChanged);
        EventBus.Unsubscribe<DropoffStopsCountChanged>(OnDropoffStopsChanged);
    }

    void OnPickupStopsChanged(PickupStopsCountChanged e)
    {
        if (pickupStopsLabel != null)
        {
            pickupStopsLabel.text = $"{pickupPrefix}{e.WaitingStopsCount}";
        }
    }

    void OnDropoffStopsChanged(DropoffStopsCountChanged e)
    {
        if (dropoffStopsLabel != null)
        {
            dropoffStopsLabel.text = $"{dropoffPrefix}{e.PendingDropoffStopsCount}";
        }
    }
}
