using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Projectile
{
    public void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.angularVelocity = new Vector3(Random.Range(-180, 180f), Random.Range(-180,180f), Random.Range(-180,180)); //do nothing cause disabled gravity
    }
}
