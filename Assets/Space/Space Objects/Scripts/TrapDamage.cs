using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [SerializeField] private float damagePerSecond;
    private PlayerController playerController;
    private bool isPlayerInRange;

    private void OnCollisionEnter(Collision other) => OnTrapEnter(other.gameObject);

    private void OnTriggerEnter(Collider other) => OnTrapEnter(other.gameObject);

    private void OnTrapEnter(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            playerController = obj.GetComponent<PlayerController>();
            isPlayerInRange = true;
        }
    }

    private void OnCollisionStay(Collision other) => OnTrapStay();

    private void OnTriggerStay(Collider other) => OnTrapStay();

    private void OnTrapStay()
    {
        if (isPlayerInRange)
        {
            playerController.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }

    private void OnCollisionExit(Collision other) => OnTrapExit(other.gameObject);

    private void OnTriggerExit(Collider other) => OnTrapExit(other.gameObject);

    private void OnTrapExit(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            playerController = null;
            isPlayerInRange = false;
        }
    }
}
