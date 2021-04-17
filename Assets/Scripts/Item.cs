using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private int goldValue;
    [SerializeField] private bool isQuestItem;
    [SerializeField] private ItemRarity rarity;

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

    public bool IsQuestItem
    {
        get => isQuestItem;
        set => isQuestItem = value;
    }

    public ItemRarity Rarity
    {
        get => rarity;
        set => rarity = value;
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }
}
