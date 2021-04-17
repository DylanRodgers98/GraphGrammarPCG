using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IList<Item> inventory;

    private void Start()
    {
        inventory = new List<Item>();
    }

    public void AddItemToInventory(Item item)
    {
        inventory.Add(item);
    }

    public bool DoesInventoryContainItem(Item item)
    {
        return inventory.Contains(item);
    }
}
