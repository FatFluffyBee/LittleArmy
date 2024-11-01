using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{    
    public AudioSource sound;

    public override void Initialize(float damage, float knockbackForce, Vector3 origin, AgentType agentType)
    {
        base.Initialize(damage, knockbackForce, origin, agentType);
    }
    void Update()
    {
        transform.LookAt(transform.position + rb.velocity, Vector3.forward);
    }

    void OnCollisionEnter(Collision other) 
    {
        sound.Play();
        /*if(other.transform.GetComponent<IsTargeteable>()?.agentType == agentType) {
            Vector3 knockBarDir = other.transform.position - origin; //
            knockBarDir.y = 0;
            Vector3 knockbackVector = knockBarDir.normalized * knockbackForce;
            other.transform.GetComponent<HealthSystem>()?.TakeDamage(damage, knockbackVector);
        }*/
    
        Vector3 knockBarDir = other.transform.position - origin; 
        knockBarDir.y = 0;
        Vector3 knockbackVector = knockBarDir.normalized * knockbackForce;
        other.transform.GetComponent<HealthSystem>()?.TakeDamage(damage, knockbackVector);

        if(other.transform.GetComponent<Agent>()) //cause of feedbacks pb for agents
            transform.SetParent(other.transform.GetComponent<Agent>().rd.transform);
        else
            transform.SetParent(other.transform);
            
        Destroy(rb);
        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<GravityAmplifier>());
        Destroy(this);
    }
}
