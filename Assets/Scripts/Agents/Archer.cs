using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Archer : Agent
{
    [Header("Projectile Launch")]
    public GameObject arrowPrefab;
    public Transform launchPoint;
    public float reloadTime;
    public Vector2 timeToChargeRange;
    public float range;
    public float deviation;
    public Vector2 airTimeRange;
    public float gravityMul;
    public float damage;
    public float knockbackForce;
    private float reloadTimeTimer;
    private float timeToCharge;
    private float timeToChargeCount;
    private bool isCharging;
    private bool readyToFire = false;

    [Header("Targetting")]
    [SerializeField] private IsTargeteable target;
    public TargettingType targettingType = TargettingType.First;
    public float timeBtwTargetCheck = 0.2f;

    [Header("Debug Trajectory")]
    private LineRenderer lineRd;
    public float stepDuration;
    public int stepNumbers;

    float timerCheckNewTarget = 0;
    
    void Start(){
        lineRd = GetComponent<LineRenderer>();
    }
    void Update(){ //todo recompile
        BaseUpdate();

        if(AgStatus == AgentStatus.Idle){
            if(timerCheckNewTarget < Time.time) {
                target = FindClosestTargetInViewRange();
                timerCheckNewTarget = Time.time + 0.2f;
            }
        } else if (AgStatus == AgentStatus.Attacking) {
            if(target == null){
                feedbackMovement = false;
                AgStatus = AgentStatus.Idle;
            }
        } else if (AgStatus == AgentStatus.Travelling) {
            timeToChargeCount = 0;
            feedbackMovement = true;
            if(IsAgentAtDestination()) {
                AgStatus = AgentStatus.Idle;
                feedbackMovement = false;
            }
        }

        if(AgStatus != AgentStatus.Travelling && target != null) {
            AgStatus = AgentStatus.Attacking;
            feedbackMovement = false;
        }

        if(AgStatus == AgentStatus.Attacking){
            if(reloadTimeTimer < Time.time) {
                if(!isCharging){
                    isCharging = true;
                    timeToCharge = Random.Range(timeToChargeRange.x, timeToChargeRange.y);
                    timeToChargeCount = timeToCharge + Time.time;
                }
                else {
                    if(timeToChargeCount < Time.time)
                        readyToFire = true;
                }
            }
        }
        else {
            isCharging = false;
        }
    }
    void FixedUpdate(){ 
        if(readyToFire){
            FireProjectile();
            reloadTimeTimer = Time.time + reloadTime - timeToCharge;
            isCharging = false;
            readyToFire = false;
        }
    }

    void FireProjectile(){
        float range01 = Vector3.Distance(target.transform.position, launchPoint.position) / range;
        float airTimeFromDist = airTimeRange.x + (airTimeRange.y - airTimeRange.x) * range01; //retourne le temps que met la flèche pour attendre sa cible en fonction de la distance à celle-ci

        Vector3 ennemiFuturePos = target.GetComponent<Agent>().GetPredictedPos(airTimeFromDist);

        Vector3 initialVelocity = TrajMaths.InitialVelocityForBellTrajectory(launchPoint.position, ennemiFuturePos, airTimeFromDist, deviation, gravityMul);

        if(debug){
            Vector3 pos = launchPoint.position;
            Vector3 velocity = initialVelocity;
            lineRd.positionCount = stepNumbers+1;
            lineRd.SetPosition(0, pos);
            for(int i = 0; i < stepNumbers; i++)        {
                pos += velocity * stepDuration;
                velocity += Physics.gravity * stepDuration * gravityMul;
                
                lineRd.SetPosition(i+1, pos);
            }
        }

        GameObject instance = Instantiate(arrowPrefab, launchPoint.transform.position, Quaternion.identity);
        instance.GetComponent<Rigidbody>().velocity = initialVelocity;
        instance.GetComponent<GravityAmplifier>().Initialize(gravityMul);
        instance.GetComponent<Projectile>().Initialize(damage, knockbackForce, transform.position, AgentType.Ennemi);
    }

    private IsTargeteable FindClosestTargetInViewRange(){ //vérifie si la target actuelle n'est plus bonne and if not find a new one
        List<DataTarget> targets = new List<DataTarget>();
        Collider [] hits = Physics.OverlapSphere(transform.position, range);

        if(hits.Length > 0){
            if(targettingType == TargettingType.First || targettingType == TargettingType.Last){
                foreach(Collider c in hits){
                    if(c.transform.GetComponent<IsTargeteable>())
                        if(c.transform.GetComponent<IsTargeteable>().agentType == AgentType.Ennemi){
                            DataTarget target = new DataTarget(c, Vector3.Distance(c.transform.position, launchPoint.position));

                            if(targets.Count == 0){
                                targets.Add(target);
                                continue;
                            }

                            int count = targets.Count;
                            for(int i = 0; i < count; i++){
                                if(targets[i].dist > target.dist){
                                    targets.Insert(i, target);
                                    break;
                                }
                                else if(i == targets.Count - 1){

                                    targets.Add(target);
                                }
                            }
                        }
                }

                if(targets.Count > 0){
                    if(targettingType == TargettingType.First) 
                        target =  targets[0].col.transform.GetComponent<IsTargeteable>();
                    return target;
                } 
            }
        } 
        return null;
    }

    private void OnDrawGizmos() {
        if(debug) 
            Gizmos.DrawWireSphere(transform.position, range); //Draw range
    }
}

