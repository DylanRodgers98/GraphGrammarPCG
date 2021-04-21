using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private const float HealthGUIWidth = 100;
    private const float HealthGUIHeight = 25;
    private const float HealthGUIOffset = 10;
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float maxHealth;
    private float currentHealth;
    private IList<Item> inventory;
    private bool isDead;

    public IList<Quest> Quests { get; private set; }

    public void AddItemToInventory(Item item) => inventory.Add(item);

    public bool DoesInventoryContainItem(Item item) => inventory.Contains(item);

    public void AddQuest(Quest quest) => Quests.Add(quest);

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Kill();
    }

    private void Kill()
    {
        if (!isDead)
        {
            currentHealth = 0;
            navMeshAgent.gameObject.SetActive(false);
            isDead = true;
        }
    }

    private void Awake()
    {
        inventory = new List<Item>();
        Quests = new List<Quest>();
        currentHealth = maxHealth;
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

    private void OnGUI()
    {
        float x = Screen.width - HealthGUIWidth - HealthGUIOffset;
        float y = HealthGUIOffset;
        double roundedHealth = Math.Round(currentHealth, MidpointRounding.AwayFromZero);
        string text = $"Health: {roundedHealth}";
        GUI.Box(new Rect(x, y, HealthGUIWidth, HealthGUIHeight), text);
    }
}