using TMPro;
using UnityEngine;
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

    public void SetContainer(UpgradeData data, int baseLevel)
    {
        title.text = data.displayName;
        description.text = data.description;
        cost.text = data.CostFor(baseLevel + 1).ToString() + " $";
        this.baseLevel.text = baseLevel.ToString();
        this.newLevel.text = (baseLevel + 1).ToString();
        baseBonus.text = data.MultiplierFor(baseLevel).ToString();
        newBonus.text = data.MultiplierFor(baseLevel + 1).ToString();

        int rng = Random.Range(0, colors.Length);
        bg.color = colors[rng];
    }
}
