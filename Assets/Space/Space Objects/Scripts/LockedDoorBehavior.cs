using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LockedDoorBehavior : MonoBehaviour
{
    private IList<GameObject> requiredKeys;
    private bool isDoorLocked = true;

    public void AddRequiredKey(GameObject requiredKey)
    {
        if (requiredKeys == null)
        {
            requiredKeys = new List<GameObject>();
        }

        requiredKeys.Add(requiredKey);
    }

    public void SetRequiredKeys(IList<GameObject> requiredKeys)
    {
        if (requiredKeys == null)
        {
            this.requiredKeys = requiredKeys;
        }
        else
        {
            foreach (GameObject requiredKey in requiredKeys)
            {
                this.requiredKeys.Add(requiredKey);
            }
        }
    }

    public bool UnlockDoor(ICollection<GameObject> playerInventory)
    {
        if (!isDoorLocked) return true;
        if (!HasAllRequiredKeys(playerInventory)) return false;
        isDoorLocked = false;
        return true;
    }

    private bool HasAllRequiredKeys(ICollection<GameObject> playerInventory)
    {
        return requiredKeys.All(playerInventory.Contains);
    }
}