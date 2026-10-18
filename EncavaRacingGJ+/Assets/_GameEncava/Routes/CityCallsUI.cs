using Framework.Core;
using TMPro;
using UnityEngine;

public class CityCallsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI callsText;

    void OnEnable()
    {
        EventBus.Subscribe<CityCallsUpdated>(OnCallsUpdated);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<CityCallsUpdated>(OnCallsUpdated);
    }

    void OnCallsUpdated(CityCallsUpdated e)
    {
        if (callsText != null)
        {
            callsText.text = $"Paradas con gente: {e.ActiveCalls}";
        }
    }
}