using Framework.UI;
using Framework.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PerkSeleccionScreen : UIScreen
{
    [Header("Referencias")]
    [SerializeField] PerkService perkService;
    [SerializeField] GameFlow flow;

    [Header("UI")]
    [SerializeField] PerkButton[] perkButtons = new PerkButton[3];
    [SerializeField] Button confirmButton;
    [SerializeField] Button skipButton;
    [SerializeField] Color selectedColor = Color.yellow;
    [SerializeField] Color normalColor = Color.white;

    PerkData[] currentChoices;
    int selectedIndex = -1;
    
    protected override void Awake()
    {
        base.Awake();
        if (perkService == null) perkService = FindFirstObjectByType<PerkService>();
        if (flow == null) flow = FindFirstObjectByType<GameFlow>();

        for (int i = 0; i < perkButtons.Length; i++)
        {
            int index = i;
            perkButtons[i].Button.onClick.AddListener(() => OnPerkSelected(index));
        }
        confirmButton.onClick.AddListener(OnConfirm);
        skipButton.onClick.AddListener(OnSkip);

        confirmButton.interactable = false;
    }
    void OnEnable() => EventBus.Subscribe<PerkOffered>(OnPerkOffered);
    void OnDisable() => EventBus.Unsubscribe<PerkOffered>(OnPerkOffered);

    void OnPerkOffered(PerkOffered e)
    {
        currentChoices = e.Choices;
        selectedIndex = -1;
        flow.RequestPerkSelection(); // Cambia a GameState.PerkSelection

        for(int i = 0; i < perkButtons.Length; i++)
        {
            perkButtons[i].Setup(i < currentChoices.Length ? currentChoices[i] : null);
        }

        UpdateConfirmButton();
    }

    void OnPerkSelected(int index)
    {
        if (index >= currentChoices.Length) return;

        selectedIndex = index;
        UpdateButtonHighlights();
        UpdateConfirmButton();
    }

    void OnConfirm()
    {
        if (selectedIndex >= 0 && selectedIndex < currentChoices.Length)
            perkService.SelectPerk(currentChoices[selectedIndex]);

        flow.RequestResume(); // Vuelve a Playing
    }

    void OnSkip()
    {
        flow.RequestResume(); // Regresa sin perks
    }

    void UpdateButtonHighlights()
    {
        for (int i = 0; i < perkButtons.Length; i++)
        {
            var colors = perkButtons[i].Button.colors;
            colors.normalColor = (i == selectedIndex) ? selectedColor : normalColor;
            perkButtons[i].Button.colors = colors;
        }
    }

    void UpdateConfirmButton() => confirmButton.interactable = selectedIndex >= 0;
}
[Serializable]
public class PerkButton
{
    public Button Button;
    public TMP_Text NameText;
    public TMP_Text DescriptionText;
    public TMP_Text StatText;

    public void Setup(PerkData perk)
    {
        if (perk == null) return;
        Button.gameObject.SetActive(true);

        NameText.text = perk.displayName;
        DescriptionText.text = perk.description;
        StatText.text = $"{perk.stat}: \nx{perk.multiplier}";
    }
}