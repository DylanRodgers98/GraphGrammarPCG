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
            doorGameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
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