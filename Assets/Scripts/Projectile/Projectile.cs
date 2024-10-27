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
        rb = GetComponent<Rigidbody>();
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.origin = origin;
        this.agentType = agentType;
    }
}
