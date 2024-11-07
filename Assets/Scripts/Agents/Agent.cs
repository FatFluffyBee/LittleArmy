using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Agent : MonoBehaviour, ISelectable
{
    protected AgentState currentState { get; set; }
    protected AgentState previousState;

    public SuperAgent SuperAgent {get; set;}
    protected NavMeshAgent navMeshAgent;
    public Renderer rd;
    public GameObject fbSelectionObject;
    public float maxHealth = 10;
    [Range (0.001f, 0.1f)][SerializeField] private float stillThreshold = 0.05f;
    protected Rigidbody rb;
    private PathBallDisplay pathBallDisplay;
    protected bool isCenterAgent = false;
    public bool AsBeenMoveOrdered {get; set;}
    protected Color unitColor;
    public Vector3 HomePoint {get; set;}

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

    public Transform Target {get; set;}
    public bool InCombat {get; set;}
 
    public void Initialize(){
        rb = GetComponent<Rigidbody>();
        baseColor = rd.material.color;
        GetComponent<HealthSystem>().Initialize(maxHealth);
        GetComponent<HealthSystem>().OnDeath += OnDeath;
        GetComponent<HealthSystem>().OnTakingDamage += OnTakingDamage;
        navMeshAgent = GetComponent<NavMeshAgent>();

        pathBallDisplay = GetComponent<PathBallDisplay>();
        pathBallDisplay?.Initialize(unitColor);

        NavMesh.SamplePosition(transform.position, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
        HomePoint = navPos.position;

        navMeshAgent.updateRotation = false;
    }

    public virtual void BaseUpdate()
    {
        FeedbackMovement();
        VisualFeedbackHit();
        if(pathBallDisplay != null && AsBeenMoveOrdered) 
            if(isCenterAgent && currentState == AgentState.Travelling)
                pathBallDisplay.DisplayUnitPathWithBalls(navMeshAgent.path.corners.ToList());

        if(!rb.isKinematic) {
            navMeshAgent.nextPosition = transform.position;
             if(rb.velocity.magnitude < stillThreshold)
                DisableRigidBodyPhysics();
        }
        if(debug)
            DebugPath();
    }

    //--------------------------------------------------------
    //MOVEMENT RELATED

    public void SetDestination(Vector3 destination) {
        navMeshAgent.SetDestination(destination);
    }

    public bool CheckAndSetDestination (Vector3 destination) {
    if(NavMesh.SamplePosition(destination, out NavMeshHit navPos, 5, NavMesh.AllAreas)) {
        navMeshAgent.SetDestination(navPos.position);
        return true;
    }
        return false;
    }

    public bool CheckAndSetDestination (Vector3 destination, out Vector3 navMeshPoint){
        if(NavMesh.SamplePosition(destination, out NavMeshHit navPos, 5, NavMesh.AllAreas)) {
            navMeshPoint = navPos.position;
            navMeshAgent.SetDestination(navMeshPoint);
            return true;
        }

        navMeshPoint = Vector3.zero;
        return false;
    }

    public void GiveMoveOrder(Vector3 destination) { 
        if(CheckAndSetDestination(destination, out Vector3 navPos));
        currentState = AgentState.Travelling;
        HomePoint = navPos; //Todo
        AsBeenMoveOrdered = true;
    }

    void FeedbackMovement(){
        if((feedbackMovement && rb.isKinematic) || cycleTimer != 0){
            cycleTimer += Time.deltaTime; 
            rd.transform.position = transform.position + Vector3.up * (Mathf.Abs(Mathf.Cos((cycleTimer / cycleDuration - 0.25f) * 6.28f)) * heightMax);
            if(cycleTimer > cycleDuration) cycleTimer = 0;
        }
    }

    public bool IsAgentAtDestination(){
        Vector3 start = GenMaths.WithoutY(transform.position);
        Vector3 end = GenMaths.WithoutY(navMeshAgent.path.corners[^1]);

        if(Vector3.Distance(start, end) < 0.1f)
            return true;
        else 
            return false;
    }

    public bool IsAgentAtHomePoint(){
        Vector3 start = GenMaths.WithoutY(transform.position);
        Vector3 end = GenMaths.WithoutY(HomePoint);

        if(Vector3.Distance(start, end) < 0.1f)
            return true;
        else {
            return false;
        }
            
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
        SetSelectionFeedbackVisibility(false);
        rd.material.color = Color.grey;
        transform.parent = null;

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
        navMeshAgent.speed = 10;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
        //NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 1000, NavMesh.AllAreas);
        //navMeshAgent.Warp(navHit.position);
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
        if(fbSelectionObject != null)
            fbSelectionObject.SetActive(status);
    }

    protected void DebugPath() {
        if(navMeshAgent.path.corners.Length > 0){
            for(int i = 1; i < navMeshAgent.path.corners.Length; i++) {
                Debug.DrawLine(navMeshAgent.path.corners[i - 1], navMeshAgent.path.corners[i], Color.red);
            }
        }
    }

    public virtual void LookAtDirection(Vector3 dir) {
        dir.y = transform.position.y;
        transform.LookAt(dir);
    }

    //----------------------------------------------------------------
    //TARGETING RELATED FUNCTIONS
    //----------------------------------------------------------------

    //Target seeking
    protected List<DataTarget> GetDataTargetsInRange(float range, AgentType agentType, TargetType targetType, DistMode distMode) {
        List<DataTarget> targets = new List<DataTarget>();

        LayerMask mask = LayerMask.GetMask("Agents");
        Collider [] hits = Physics.OverlapSphere(transform.position, range, mask);

        if(hits.Length > 0){
            foreach(Collider c in hits){
                if(c.transform.GetComponent<IsTargeteable>()) {
                    if(c.transform.GetComponent<IsTargeteable>().agentType == agentType){ 
                        if(targetType == TargetType.Melee && c.transform.GetComponent<IsTargeteable>().isRange) {continue;}
                        if(targetType == TargetType.Range && !c.transform.GetComponent<IsTargeteable>().isRange) {continue;}

                        float distance;
                        if(distMode == DistMode.View)
                            distance = Vector3.Distance(transform.position, c.transform.position);
                        else 
                            distance = NavMaths.DistBtwPoints(transform.position, c.transform.position);
                        targets.Add(new DataTarget(c, distance));
                    }
                }
            }
        }
        return targets;       
    }

    protected List<DataTarget> OrderDataTargetsByDist(List<DataTarget> dataTargets) {
        for(int i = 0; i < dataTargets.Count - 1; i++) {
            for(int j = 0; j < dataTargets.Count - 1 ; j++) {
                float present = dataTargets[j].dist;
                float adjacent = dataTargets[j+1].dist;

                if(present > adjacent){
                    DataTarget temp = dataTargets[j];
                    dataTargets[j] = dataTargets[j+1];
                    dataTargets[j+1] = temp;
                }
            }
        }        
        return dataTargets;
    }

    protected Transform GetClosestTargetInRange(float range, AgentType agentType, TargetType targetType, DistMode distMode, out float dist) {
        List<DataTarget> dataTargets = GetDataTargetsInRange(range, agentType, targetType, distMode);
        dataTargets = OrderDataTargetsByDist(dataTargets);

        if(dataTargets.Count > 0) {
            dist = dataTargets[0].dist;
            return dataTargets[0].col.transform;
        }

        dist = Mathf.Infinity;
        return null;
    }

    protected Transform GetClosestTargetInRange(float range, AgentType agentType, TargetType targetType, DistMode distMode) {
        return GetClosestTargetInRange(range, agentType, targetType, distMode, out float a);
    }
    
    protected Transform GetRandomTargetInRange(float range, AgentType agentType, TargetType targetType, DistMode distMode, out float dist, int maxTarget) {
        List<DataTarget> dataTargets = GetDataTargetsInRange(range, agentType, targetType, distMode);
        dataTargets = OrderDataTargetsByDist(dataTargets);

        if(dataTargets.Count > 0) {
            int randomIndex = Random.Range(0, Mathf.Min(maxTarget, dataTargets.Count));
            dist = dataTargets[randomIndex].dist;
            return dataTargets[randomIndex].col.transform;
        }

        dist = Mathf.Infinity;
        return null;
    }

    protected Transform GetRandomTargetInRange(float range, AgentType agentType, TargetType targetType, DistMode distMode, int maxTarget) {
        return GetRandomTargetInRange(range, agentType, targetType, distMode, out float a, maxTarget);
    }
    //Target verifying

    protected bool IsTargetValid(Transform target, float range) {
        if(target == null) return false;
        return IsTargetInRange(target, range) && IsTargetTargetable(target);
    }

    protected bool IsTargetInRange(Transform target, float range) {
        return Vector3.Distance(target.position, transform.position) < range;
    }

    protected bool IsTargetTargetable(Transform target) {
        return target.GetComponent<IsTargeteable>() != null;
    }

    protected bool IsTargetHittable(Vector3 startPos, Transform target, float maxRange, Vector2 airTimeRange, float gravityMul, int nSteps) {
        Vector3 initialVelocity = TrajMaths.GetInitVelocityForBellCurveFromRangeValue(startPos, target, maxRange, airTimeRange, gravityMul, Vector2.zero, 
            TrajMode.ActualPos, out float airTime);
        float stepDuration = airTime / nSteps;
        Vector3 pos = startPos;
        LayerMask mask = LayerMask.GetMask("Terrain");
        for(int i = 0; i < nSteps/2; i++) {
                Debug.DrawLine(pos, pos + stepDuration * initialVelocity, Color.blue, 0.01f);
                pos += initialVelocity * stepDuration;
                initialVelocity += Physics.gravity * stepDuration * gravityMul;
                
                if(Physics.Raycast(pos, initialVelocity, out RaycastHit hit, Vector3.Distance(pos, pos + initialVelocity * stepDuration), mask)) 
                {
                    Debug.DrawLine(hit.point, hit.point + hit.normal * 10, Color.red, 0.5f);
                    Debug.Log("Hit wall"); 
                    return false;
                }
            }
        
        Debug.Log("No wall hit");
        return true;
    }   

    public void EnableAgentMovement(bool choice) {
        navMeshAgent.isStopped = !choice;
        feedbackMovement = choice;
    }

    protected void SwitchAgentState(AgentState state) {
        currentState = state;
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
    public enum AgentState {Idle,  Attacking, Travelling, AttackBuilding, SeekAgent, CircleAgent, AttackAgent, SeekBuilding, Following, Circling, Charging, Fleeing}

    public struct DataTarget
    {
        public Collider col;
        public float dist;

        public DataTarget(Collider collider, float distance){
            this.col = collider;
            this.dist = distance;
        }
    }

    public enum DistMode {View, Nav}
    public enum TargetType {Melee, Range, All}
