using UnityEngine;

public class Enemy : Damageable
{
    [SerializeField] private string enemyName;
    [SerializeField] private float damagePerAttack;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;
    private Quest quest;
    private bool isDeactivated;

    public string EnemyName => enemyName;

    public Quest Quest
    {
        set => quest = value;
    }

    private void Update()
    {
        if (IsDead && !isDeactivated)
        {
            gameObject.SetActive(false);
            quest?.MarkAsCompleted();
            isDeactivated = true;
        }
    }
}