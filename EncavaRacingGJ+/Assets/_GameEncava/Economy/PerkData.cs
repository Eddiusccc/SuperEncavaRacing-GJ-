using UnityEngine;

[CreateAssetMenu(fileName = "NewPerk", menuName = "GameEncava/Perk")]
public class PerkData : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public StatType stat;
    public float multiplier = 1.2f; // ej: +20%
}
