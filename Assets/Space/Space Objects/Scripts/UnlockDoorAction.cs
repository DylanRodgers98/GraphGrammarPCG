using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnlockDoorAction : Actionable
{
    private bool isDoorLocked = true;
    private IList<GameObject> requiredKeys;

    protected override void DoAction()
    {
        if (isDoorLocked && requiredKeys.All(PlayerController.DoesInventoryContainItem))
        {
            isDoorLocked = false;
        }
    }

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
        if (this.requiredKeys == null)
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
}