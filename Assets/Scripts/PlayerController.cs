using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private readonly IList<GameObject> inventory = new List<GameObject>();

    public void AddItemToInventory(GameObject item)
    {
        inventory.Add(item);
    }

    public bool DoesInventoryContainItem(GameObject item)
    {
        return inventory.Contains(item);
    }
}
