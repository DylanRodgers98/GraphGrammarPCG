using UnityEngine;

public abstract class Damageable : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    protected float CurrentHealth;
    protected bool IsDead;

    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            IsDead = true;
        }
    }

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }
}