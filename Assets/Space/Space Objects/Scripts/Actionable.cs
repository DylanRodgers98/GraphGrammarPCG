using UnityEngine;

public abstract class Actionable : MonoBehaviour
{
    protected PlayerController PlayerController;
    [SerializeField] protected KeyCode actionKey;
    protected bool IsPlayerInRange;

    protected abstract void DoAction();

    protected abstract string GetGUIText();

    private void Update()
    {
        if (IsPlayerInRange && Input.GetKeyDown(actionKey))
        {
            DoAction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController = other.gameObject.GetComponent<PlayerController>();
            IsPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController = null;
        IsPlayerInRange = false;
    }

    private void OnGUI()
    {
        string text = GetGUIText();
        if (text != null)
        {
            GUI.Box(new Rect(10, 10, 200, 30), text);
        }
    }
}