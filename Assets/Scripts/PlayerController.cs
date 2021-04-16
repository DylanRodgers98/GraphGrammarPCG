using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IList<GameObject> inventory;

    public void AddItemToInventory(GameObject item)
    {
        inventory.Add(item);
    }

    public bool DoesInventoryContainItem(GameObject item)
    {
        return inventory.Contains(item);
    }
}
