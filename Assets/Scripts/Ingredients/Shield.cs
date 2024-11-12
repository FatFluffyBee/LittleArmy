using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private float health;
    [SerializeField] private Transform lowerPos;
    [SerializeField] private Transform raisePos;
    private bool isRaised;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<HealthSystem>().Initialize(health);
        GetComponent<HealthSystem>().OnDeath += OnDeath;
    }

    void OnDeath() {
        Destroy(this);
        transform.parent = null;

        rb.freezeRotation = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        gameObject.layer = LayerMask.NameToLayer("ColOnlyTerrain");
        transform.parent = null;

        Destroy(this);
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
