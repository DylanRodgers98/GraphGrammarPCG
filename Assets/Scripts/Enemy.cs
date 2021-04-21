using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private string enemyName;

    public string EnemyName => enemyName;
    public Quest Quest { get; set; }
}
