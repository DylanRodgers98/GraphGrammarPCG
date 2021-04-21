using UnityEngine;

public abstract class Actionable : MonoBehaviour
{
    private const float GUIWidth = 200;
    private const float GUIHeight = 25;
    private const float GUIOffset = 10;
    
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
            float x = Screen.width / 2f - GUIWidth / 2;
            float y = Screen.height - GUIHeight - GUIOffset;
            GUI.Box(new Rect(x, y, GUIWidth, GUIHeight), text);
        }
    }
}