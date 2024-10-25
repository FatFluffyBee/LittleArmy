using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castle : MonoBehaviour
{
    public int maxHealth;

    private void Start(){
        GetComponent<HealthSystem>().Initialize(maxHealth);
        GetComponent<HealthSystem>().OnDeath += OnDeath;
    }

    protected virtual void OnDeath(){
        EnnemiObjective.instance.Remove(this);
    }
}
