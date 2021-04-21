using UnityEngine;

public abstract class Actionable : MonoBehaviour
{
    protected PlayerController PlayerController;
    [SerializeField] protected KeyCode actionKey;
    private bool isPlayerInRange;

    protected abstract void DoAction();

    protected abstract string GetGUIText();

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(actionKey))
        {
            DoAction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController = other.gameObject.GetComponent<PlayerController>();
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController = null;
        isPlayerInRange = false;
    }

    private void OnGUI()
    {
        if (!isPlayerInRange) return;
        string text = GetGUIText();
        if (text != null)
        {
            GUI.Box(new Rect(10, 10, 200, 30), text);
        }
    }
}