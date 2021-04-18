using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IList<Item> inventory;

    public IList<Quest> Quests { get; private set; }

    private void Start()
    {
        inventory = new List<Item>();
        Quests = new List<Quest>();
    }

    public void AddItemToInventory(Item item) => inventory.Add(item);

    public bool DoesInventoryContainItem(Item item) => inventory.Contains(item);

    public void AddQuest(Quest quest) => Quests.Add(quest);
}
