using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Agent : MonoBehaviour, ISelectable
{
    public AgentStatus AgStatus { get; set; }
    public SuperAgent SuperAgent {get; set;}
    protected NavMeshAgent navMeshAgent;
    public Renderer rd;
    public GameObject fbSelectionObject;
    public float maxHealth = 10;
    [Range (0.001f, 0.1f)][SerializeField] private float stillThreshold = 0.05f;
    protected Rigidbody rb;
    private PathBallDisplay pathBallDisplay;
    protected bool isCenterAgent = false;
    protected bool asBeenMoveOrdered = false;
    protected Color unitColor;
    protected Vector3 homePoint ;

    [Header("Damage Feedback")]
    public float feedbackDuration = 0.4f;
    public Color feedbackColor= Color.red;
    private bool feedbackHit;
    private float timerFeedback;
    private Color baseColor;

    [Header("Movement Feedback")]
    public float cycleDuration = 0.4f;
    public float heightMax = 1f;
    private float cycleTimer;
    protected bool feedbackMovement = false;
    public bool debug;
 
    public void Initialize(SuperAgent superAgent, Color unitColor){
        SuperAgent = superAgent;
        rb = GetComponent<Rigidbody>();
        baseColor = rd.material.color;
        GetComponent<HealthSystem>().Initialize(maxHealth);
        GetComponent<HealthSystem>().OnDeath += OnDeath;
        GetComponent<HealthSystem>().OnTakingDamage += OnTakingDamage;
        this.unitColor = unitColor;
        navMeshAgent = GetComponent<NavMeshAgent>();

        pathBallDisplay = GetComponent<PathBallDisplay>();
        pathBallDisplay?.Initialize(unitColor);

        NavMesh.SamplePosition(transform.position, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
        homePoint = navPos.position;

        navMeshAgent.updateRotation = false;
    }

    public virtual void BaseUpdate()
    {
        FeedbackMovement();
        VisualFeedbackHit();
        if(pathBallDisplay != null && asBeenMoveOrdered) 
            if(isCenterAgent && AgStatus == AgentStatus.Travelling)
                pathBallDisplay.DisplayUnitPathWithBalls(navMeshAgent.path.corners.ToList());

        if(!rb.isKinematic) {
             if(rb.velocity.magnitude < stillThreshold)
                DisableRigidBodyPhysics();
        }
        if(debug)
            DebugPath();
    }

    //--------------------------------------------------------
    //MOVEMENT RELATED
    public virtual Vector3 GetPredictedPos(float timeToMove) { //return the predicted position of the object in x seconds

        if(navMeshAgent.path.corners.Length == 0) return transform.position; 
        
        Vector3[] path = navMeshAgent.path.corners;
        float agentSpeed = navMeshAgent.speed;

        for(int i = 1; i < path.Length; i++) {
            float timeTillNextNode = Vector3.Distance(path[i-1], path[i]) / agentSpeed;
            if(timeTillNextNode > timeToMove) { //no new path cornet in the time remaining
                return path[i-1] + timeToMove * agentSpeed * (path[i] - path[i-1]).normalized;
            }
            else { // new path cornet in the time remaining
                timeToMove -= timeTillNextNode;
            }
        }   

        return path[path.Length-1]; //if no path cornet in the time remaining
    }

    protected void SetDestination (Vector3 destination){
        navMeshAgent.SetDestination(destination);
    }

    public void GiveMoveOrder(Vector3 destination) {
        SetDestination(destination);
        AgStatus = AgentStatus.Travelling;
        homePoint = destination;
        asBeenMoveOrdered = true;
    }

    void FeedbackMovement(){
        if((feedbackMovement && rb.isKinematic) || cycleTimer != 0){
            cycleTimer += Time.deltaTime; 
            rd.transform.position = transform.position + Vector3.up * (Mathf.Abs(Mathf.Cos(cycleTimer / cycleDuration * 6.28f)) * heightMax);
            if(cycleTimer > cycleDuration) cycleTimer = 0;
        }
    }

    protected bool IsAgentAtDestination(){
        Vector3 start = transform.position;
        Vector3 end = navMeshAgent.destination;
        start.y = 0;
        end.y = 0;

        if(Vector3.Distance(start, end) < 0.1f)
            return true;
        else 
            return false;
    }

    protected bool IsAgentAtHomePoint(){
        Vector3 start = transform.position;
        Vector3 end = homePoint;
        start.y = 0;
        end.y = 0;

        if(Vector3.Distance(start, end) < 0.1f)
            return true;
        else 
            return false;
    }

    //--------------------------------------------------------
    //Health And Damage related
    protected virtual void OnDeath(){
        if(SuperAgent != null)
            SuperAgent.RemoveUnit(this);
        
        Destroy(GetComponent<IsTargeteable>());
        EnableRigidBodyPhysics();
        rb.freezeRotation = false;
        gameObject.layer = LayerMask.NameToLayer("ColOnlyTerrain");
        transform.Rotate(Vector3.forward * Random.Range(10, -10) + Vector3.forward * Random.Range(10, -10), 1);
        rd.material.color = Color.grey;

        Destroy(navMeshAgent);
        Destroy(this);
    }

    void OnTakingDamage(Vector3 knockbackVector){
        EnableRigidBodyPhysics();
        rb.velocity += knockbackVector / rb.mass;
        feedbackHit = true;
    }

    void EnableRigidBodyPhysics() {
        navMeshAgent.updatePosition = false;
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    void DisableRigidBodyPhysics() {
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
        NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 1000, NavMesh.AllAreas);
        navMeshAgent.Warp(navHit.position);
        navMeshAgent.updatePosition = true;
    }

    void VisualFeedbackHit(){
        if(feedbackHit) {
            if(timerFeedback >= feedbackDuration){
                timerFeedback = 0;
                feedbackHit = false;
            }
            else {
                float ratio = 1 - timerFeedback / feedbackDuration;
                rd.material.color = Color.Lerp(baseColor, feedbackColor, ratio * ratio * ratio);
            }
            timerFeedback += Time.deltaTime;
        }
    }
    public void SetBaseColor(Color color) {
        baseColor = color;
        rd.material.color = baseColor;
    }
    public void SetSelectionFeedbackVisibility(bool status) {
        fbSelectionObject.SetActive(status);
    }

    protected void DebugPath() {
        if(navMeshAgent.path.corners.Length > 0){
            for(int i = 1; i < navMeshAgent.path.corners.Length; i++) {
                Debug.DrawLine(navMeshAgent.path.corners[i - 1], navMeshAgent.path.corners[i], Color.red);
            }
        }
    }

    protected virtual void LookAtDirection(Vector3 dir) {
        dir.y = transform.position.y;
        transform.LookAt(dir);
    }

    protected List<Transform> GetTargetsInViewRange(float range, AgentType agentType) 
    {
        List<Transform> targets = new List<Transform>();

        LayerMask mask = LayerMask.GetMask("Agents");
        Collider [] hits = Physics.OverlapSphere(transform.position, range, mask);

        if(hits.Length > 0){
            foreach(Collider c in hits){
                if(c.transform.GetComponent<IsTargeteable>()) {
                    if(c.transform.GetComponent<IsTargeteable>().agentType == agentType){ 
                        targets.Add(c.transform);
                    }
                }
            }
        }
        return targets;       
    }

    protected Transform FindClosestTargetInNavRange(List<Transform> targets) //check if target still valid and then find target if not (do not change until previous target is wrong)   
    {  
        float minDistance = float.MaxValue;
        Transform target = null;

        for(int i = 0; i < targets.Count; i++) {
            float curentDistance = NavMaths.DistBtwPoints(transform.position, targets[i].transform.position);

            if(minDistance > curentDistance) {
                minDistance = curentDistance;
                target = targets[i];
            }
        }              
               
        return target;
    }

    protected void SwitchAgentState(AgentStatus status) {
        AgStatus = status;
        //Debug.Log("Switch Agent State to " + status.ToString());
    }

    //--------------------------------------------------------
    // Interface or selection
    public void IsSelected(){}
    public void IsDeselected(){}

    public AgentType GetAgentType(){
       return AgentType.Ennemi;
    }

    public ISelectable ReturnSelectedObject(){
        if(SuperAgent != null) //!modify when ennemies dont use this
            return SuperAgent.GetComponent<ISelectable>();
        else 
            return GetComponent<ISelectable>();
    }
    public void SetCenterAgent(){
        isCenterAgent = true;
    }
    public void ClearCenterAgent(){
        isCenterAgent = false;
        pathBallDisplay.EraseAllPathBalls();
    }
    void OnMouseEnter(){
        if(SuperAgent!= null) //!modify when ennemies dont use this
            SuperAgent.MouseEnterFeedback();
    }
    void OnMouseExit(){
        if(SuperAgent!= null) //!modify when ennemies dont use this
            SuperAgent.MouseExitFeedback();
    }
}
    public enum AgentStatus {Idle,  Attacking, Travelling, AttackBuilding, SeekAgent, CircleAgent, AttackAgent, SeekBuilding, Following, Circling}

    public struct DataTarget
    {
        public Collider col;
        public float dist;

        public DataTarget(Collider collider, float distance){
            this.col = collider;
            this.dist = distance;
        }
    }
