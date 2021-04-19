using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnlockDoorAction : Actionable
{
    [SerializeField] private GameObject doorGameObject;
    private bool isDoorLocked = true;
    private IList<Item> requiredKeys;

    protected override void DoAction()
    {
        if (isDoorLocked && requiredKeys.All(PlayerController.DoesInventoryContainItem))
        {
            isDoorLocked = false;
            float y = doorGameObject.transform.eulerAngles.y + 90;
            doorGameObject.transform.rotation = Quaternion.Euler(0, y, 0);
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