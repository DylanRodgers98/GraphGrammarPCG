using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private int goldValue;
    [SerializeField] private ItemRarity rarity;

    public Quest Quest { get; set; }
    
    public string ItemName
    {
        get => itemName;
        set => itemName = value;
    }

    public int GoldValue
    {
        get => goldValue;
        set => goldValue = value;
    }

    public ItemRarity Rarity
    {
        get => rarity;
        set => rarity = value;
    }

    public bool IsQuestItem() => Quest != null;

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }
}
