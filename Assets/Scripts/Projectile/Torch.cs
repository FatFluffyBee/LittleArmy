using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Torch : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float gravityModif;
    [SerializeField] private float timeToLive;
    private Castle castle;

    public void Initialize(Castle castle) {
        this.castle = castle;
        Vector3 initVelocity = TrajMaths.InitialVelocityForBellTrajectory(transform.position, castle.transform.position, timeToLive, 0, gravityModif);
        GetComponent<Rigidbody>().velocity = initVelocity;;
        GetComponent<GravityAmplifier>().Initialize(gravityModif);
    }

    void Start() {
        Destroy(this.gameObject, timeToLive);
    }

    void Update(){
        transform.RotateAround(transform.position, Vector3.right, Time.deltaTime * rotationSpeed);
    }

    private void OnDestroy() {
        castle?.GetComponent<HealthSystem>().TakeDamage(damage, Vector3.zero);
    }
    
}
