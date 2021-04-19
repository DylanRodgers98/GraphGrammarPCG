using System;
using UnityEngine;

public abstract class Actionable : MonoBehaviour
{
    protected PlayerController PlayerController;
    [SerializeField] private KeyCode actionKey;
    private bool isPlayerInRange;

    protected abstract void DoAction();

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
}
