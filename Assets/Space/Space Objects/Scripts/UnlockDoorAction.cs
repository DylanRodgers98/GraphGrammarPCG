using System.Collections.Generic;
using System.Linq;

public class UnlockDoorAction : Actionable
{
    private bool isDoorLocked = true;
    private IList<Item> requiredKeys;

    protected override void DoAction()
    {
        if (isDoorLocked && requiredKeys.All(PlayerController.DoesInventoryContainItem))
        {
            isDoorLocked = false;
        }
    }

    public void AddRequiredKey(Item requiredKey)
    {
        if (requiredKeys == null)
        {
            requiredKeys = new List<Item>();
        }

        requiredKeys.Add(requiredKey);
    }

    public void SetRequiredKeys(IList<Item> requiredKeys)
    {
        if (this.requiredKeys == null)
        {
            this.requiredKeys = requiredKeys;
        }
        else
        {
            foreach (Item requiredKey in requiredKeys)
            {
                this.requiredKeys.Add(requiredKey);
            }
        }
    }
}