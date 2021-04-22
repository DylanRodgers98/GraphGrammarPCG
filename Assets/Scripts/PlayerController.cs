using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : Damageable
{
    private const float GUIOffset = 10;
    private const float HealthGUIWidth = 100;
    private const float HealthGUIHeight = 25;
    private const float DeathGUIWidth = 100;
    private const float DeathGUIHeight = 25;
    private const float QuestsGUIWidth = 120;
    private const float QuestsGUIInitialHeight = 20;
    private const float QuestsGUIHeightPerQuest = 16;

    private readonly Collider[] enemiesHit = new Collider[5];

    [SerializeField] private Camera mainCamera;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float damagePerAttack;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private int enemiesLayerIndex;
    
    private IList<Item> inventory;
    private IList<Quest> quests;
    private float currentAttackCooldown;
    private int enemiesLayerMask;

    public void AddItemToInventory(Item item) => inventory.Add(item);

    public bool DoesInventoryContainItem(Item item) => inventory.Contains(item);

    public void AddQuest(Quest quest) => quests.Add(quest);

    private new void Awake()
    {
        base.Awake();
        inventory = new List<Item>();
        quests = new List<Quest>();
        currentAttackCooldown = 0;
        enemiesLayerMask = 1 << enemiesLayerIndex;
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
        if (IsDead) return;
        if (Input.GetMouseButtonDown(0)) MoveToPoint();
        if (Input.GetMouseButtonDown(1)) Attack();
        CoolDownAttackCooldown();
    }

    private void MoveToPoint()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))
        {
            navMeshAgent.destination = hit.point;
        }
    }

    private void Attack()
    {
        if (currentAttackCooldown > 0) return;
        currentAttackCooldown = attackCooldown;

        Physics.OverlapSphereNonAlloc(transform.position, attackRange, enemiesHit, enemiesLayerMask);
        foreach (Collider enemyHit in enemiesHit)
        {
            enemyHit.transform.GetComponent<Enemy>().TakeDamage(damagePerAttack);
        }
    }

    private void CoolDownAttackCooldown()
    {
        if (currentAttackCooldown == 0) return;
        currentAttackCooldown -= Time.deltaTime;
        if (currentAttackCooldown <= 0) currentAttackCooldown = 0;
    }

    private void OnGUI()
    {
        if (IsDead)
        {
            DisplayDeathGUI();
        }
        else
        {
            DisplayHealthGUI();
            DisplayQuestsGUI();
        }
    }

    private static void DisplayDeathGUI()
    {
        float deathX = Screen.width / 2f - DeathGUIWidth / 2;
        float deathY = Screen.height / 2f - DeathGUIHeight / 2;
        GUI.Box(new Rect(deathX, deathY, DeathGUIWidth, DeathGUIHeight), "YOU DIED");
    }

    private void DisplayHealthGUI()
    {
        float healthX = Screen.width - HealthGUIWidth - GUIOffset;
        double roundedHealth = CurrentHealth == 0 ? 0 : Math.Round(CurrentHealth, MidpointRounding.AwayFromZero);
        string healthText = $"HEALTH: {roundedHealth}";

        GUI.Box(new Rect(healthX, GUIOffset, HealthGUIWidth, HealthGUIHeight), healthText);
    }

    private void DisplayQuestsGUI()
    {
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