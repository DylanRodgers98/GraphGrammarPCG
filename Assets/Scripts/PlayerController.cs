using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private const float GUIOffset = 10;
    private const float HealthGUIWidth = 100;
    private const float HealthGUIHeight = 25;
    private const float DeathGUIWidth = 100;
    private const float DeathGUIHeight = 25;
    private const float QuestsGUIWidth = 120;
    private const float QuestsGUIInitialHeight = 20;
    private const float QuestsGUIHeightPerQuest = 16;
    
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
            GUI.Box(new Rect(deathX, deathY, DeathGUIWidth, DeathGUIHeight), "YOU DIED");
        }
        else
        {
            float healthX = Screen.width - HealthGUIWidth - GUIOffset;
            double roundedHealth = currentHealth == 0 ? 0 : Math.Round(currentHealth, MidpointRounding.AwayFromZero);
            string healthText = $"HEALTH: {roundedHealth}";
            
            GUI.Box(new Rect(healthX, GUIOffset, HealthGUIWidth, HealthGUIHeight), healthText);
            
            int numIncompleteQuests = quests.Count(quest => !quest.IsCompleted());
            if (numIncompleteQuests == 0) return;
            float questsHeight = QuestsGUIInitialHeight + QuestsGUIHeightPerQuest * numIncompleteQuests;
            string questsText = quests
                .Where(quest => !quest.IsCompleted())
                .Select(quest => quest.QuestName())
                .Aggregate("QUESTS:", (str, questName) => $"{str}\n{questName}");
            
            GUI.Box(new Rect(GUIOffset, GUIOffset, QuestsGUIWidth, questsHeight), questsText);
        }
    }
}