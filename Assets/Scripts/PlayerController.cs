using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private const float HealthGUIWidth = 100;
    private const float HealthGUIHeight = 25;
    private const float HealthGUIOffset = 10;
    private const float DeathGUIWidth = 100;
    private const float DeathGUIHeight = 25;
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float maxHealth;
    private float currentHealth;
    private bool isDead;
    private IList<Item> inventory;
    private IList<Quest> quests;

    public void AddItemToInventory(Item item) => inventory.Add(item);

    public bool DoesInventoryContainItem(Item item) => inventory.Contains(item);

    public void AddQuest(Quest quest) => quests.Add(quest);

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
            isDead = true;
        }
    }

    private void Awake()
    {
        inventory = new List<Item>();
        quests = new List<Quest>();
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
        if (!isDead && Input.GetMouseButtonDown(0) &&
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))
        {
            navMeshAgent.destination = hit.point;
        }
    }

    private void OnGUI()
    {
        if (isDead)
        {
            float deathX = Screen.width / 2f - DeathGUIWidth / 2;
            float deathY = Screen.height / 2f - DeathGUIHeight / 2;
            string deathText = "YOU DIED";
            GUI.Box(new Rect(deathX, deathY, DeathGUIWidth, DeathGUIHeight), deathText);
        }
        else
        {
            float healthX = Screen.width - HealthGUIWidth - HealthGUIOffset;
            float healthY = HealthGUIOffset;
            double roundedHealth = currentHealth == 0 ? 0 : Math.Round(currentHealth, MidpointRounding.AwayFromZero);
            string healthText = $"Health: {roundedHealth}";
            GUI.Box(new Rect(healthX, healthY, HealthGUIWidth, HealthGUIHeight), healthText);
        }
    }
}