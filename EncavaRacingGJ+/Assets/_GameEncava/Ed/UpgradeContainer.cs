using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeContainer : MonoBehaviour
{
    public Image bg;
    public Color[] colors;
    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text cost;
    public TMP_Text baseLevel;
    public TMP_Text newLevel;
    public TMP_Text baseBonus;
    public TMP_Text newBonus;
    public Button selec;
    UpgradeData data;
    ShopScreen shopScreen => FindAnyObjectByType<ShopScreen>();

    private void Awake()
    {
        selec.onClick.AddListener(OnButton);
    }
    public void OnButton()
    {
        TryBuy();
    }

    void TryBuy()
    {
        bool b = shopScreen.BuyUpgrade(data);
        if(b == true)
        {
            selec.interactable = false;
        }
    }

    public void SetContainer(UpgradeData data, int baseLevel)
    {
        title.text = data.displayName;
        description.text = data.description;
        cost.text = data.CostFor(baseLevel).ToString() + " $";
        this.baseLevel.text = baseLevel.ToString();
        this.newLevel.text = (baseLevel + 1).ToString();
        baseBonus.text = data.MultiplierFor(baseLevel).ToString();
        newBonus.text = data.MultiplierFor(baseLevel + 1).ToString();
        this.data = data;
        int rng = Random.Range(0, colors.Length);
        bg.color = colors[rng];
        selec.interactable = true;

    }
}
