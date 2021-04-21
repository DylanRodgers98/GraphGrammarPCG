using UnityEngine;

public abstract class Actionable : MonoBehaviour
{
    protected PlayerController PlayerController;
    [SerializeField] protected KeyCode actionKey;
    protected bool IsPlayerInRange;

    protected abstract void DoAction();

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
}