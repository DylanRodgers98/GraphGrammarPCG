public class PickUpItemAction : Actionable
{
    private Item item;

    private void Start()
    {
        item = GetComponent<Item>();
    }

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
}
