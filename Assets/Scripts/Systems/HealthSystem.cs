using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;

    public delegate void HealthAction();
    public event HealthAction OnDeath;
    public delegate void HandleHit(Vector3 knockback);
    public event HandleHit OnTakingDamage;

    public void Initialize(float maxHealth)
    {
        OnDeath += OnDeath;
        this.maxHealth = maxHealth;
        health = maxHealth;
    }

    public void TakeDamage(float healthLost, Vector3 knockbackVector)
    {
        health -= healthLost;
        if (health <= 0)
        {
            if(OnDeath != null)
                OnDeath();
        }
        else
        {
            if(OnTakingDamage != null)
                OnTakingDamage(knockbackVector);
        }
    }
    
}
