using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAmplifier : MonoBehaviour
{
    Rigidbody rb;
    private float gravityModif;
    public void Initialize(float gravityModif)
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        this.gravityModif = gravityModif;
    }

    void FixedUpdate()
    {
        if(rb != null)
            //rb.AddForce(Physics.gravity * gravityModif, ForceMode.Acceleration);
            rb.velocity += Physics.gravity * gravityModif * Time.deltaTime;
    }
}
