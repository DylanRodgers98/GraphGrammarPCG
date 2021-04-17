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
            PlayerController.AddItemToInventory(item);
            gameObject.SetActive(false);
        }
    }
}
