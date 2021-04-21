using UnityEngine;

public class Enemy : Damageable
{
    [SerializeField] private string enemyName;
    [SerializeField] private float damagePerAttack;
    [SerializeField] private float attackCooldown;

    public string EnemyName => enemyName;
    public Quest Quest { get; set; }
}