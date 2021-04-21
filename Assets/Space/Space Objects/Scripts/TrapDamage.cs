using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [SerializeField] private float damagePerSecond;
    private PlayerController playerController;
    private bool isPlayerInRange;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerController = other.gameObject.GetComponent<PlayerController>();
            isPlayerInRange = true;
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (isPlayerInRange)
        {
            playerController.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerController = null;
            isPlayerInRange = false;
        }
    }
}
