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

    public void AddRequiredKeys(params Item[] requiredKeys)
    {
        foreach (Item requiredKey in requiredKeys)
        {
            this.requiredKeys.Add(requiredKey);
        }
    }

    private void Awake()
    {
        requiredKeys = new List<Item>();
    }
}