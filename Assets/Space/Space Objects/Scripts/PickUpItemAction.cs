public class PickUpItemAction : Actionable
{
    protected override void DoAction()
    {
        if (gameObject.activeInHierarchy)
        {
            PlayerController.AddItemToInventory(gameObject);
            gameObject.SetActive(false);
        }
    }
}
