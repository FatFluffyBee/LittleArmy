using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected Rigidbody rb;
    protected float damage;
    protected float knockbackForce;
    protected Vector3 origin;
    protected AgentType agentType;

    public virtual void Initialize(float damage, float knockbackForce, Vector3 origin, AgentType agentType)
    {
        StartCoroutine("ActivateCollider");
        rb = GetComponent<Rigidbody>();
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.origin = origin;
        this.agentType = agentType;
    }

    IEnumerator ActivateCollider() {
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        GetComponent<Collider>().enabled = true;
    }
}
