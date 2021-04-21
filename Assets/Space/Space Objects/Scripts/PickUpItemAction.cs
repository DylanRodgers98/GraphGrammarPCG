using UnityEngine;

public class PickUpItemAction : Actionable
{
    private Item item;

    protected override void DoAction()
    {
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            PlayerController.AddItemToInventory(item);
            if (item.IsQuestItem())
            {
                item.Quest.MarkAsCompleted();
            }
        }
    }

    protected void OnGUI()
    {
        if (!IsPlayerInRange || !gameObject.activeInHierarchy) return;
        string text = $"Press {actionKey} to pick up {item.ItemName}";
        GUI.Box(new Rect(10, 10, 200, 30), text);
    }

    private void Start()
    {
        item = GetComponent<Item>();
    }
}