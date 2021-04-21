using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private NavMeshAgent navMeshAgent;
    private IList<Item> inventory;

    public IList<Quest> Quests { get; private set; }

    private void Awake()
    {
        inventory = new List<Item>();
        Quests = new List<Quest>();
    }

    private void Start()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) &&
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))
        {
            navMeshAgent.destination = hit.point;
        }
    }

    public void AddItemToInventory(Item item) => inventory.Add(item);

    public bool DoesInventoryContainItem(Item item) => inventory.Contains(item);

    public void AddQuest(Quest quest) => Quests.Add(quest);
}