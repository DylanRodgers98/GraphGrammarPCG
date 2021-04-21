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

    protected override string GetGUIText()
    {
        return IsPlayerInRange && !gameObject.activeInHierarchy ?
            $"Press {actionKey} to pick up {item.ItemName}"
            : null;
    }

    private void Start()
    {
        item = GetComponent<Item>();
    }
}