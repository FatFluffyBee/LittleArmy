using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private float health;
    [SerializeField] private Transform lowerPos;
    [SerializeField] private Transform raisePos;
    private bool isRaised;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<HealthSystem>().Initialize(health);
        GetComponent<HealthSystem>().OnDeath += OnDeath;
    }

    void OnDeath() {
        Destroy(gameObject);
    }

    public void RaiseShield() {
        if(!isRaised) {
            transform.SetPositionAndRotation(raisePos.position, raisePos.rotation);
            isRaised = true;
        }
    }

    public void LowerShield() {
        if(isRaised) {
            transform.SetPositionAndRotation(lowerPos.position, lowerPos.rotation);
            isRaised = false;
        }
    }
    
}
