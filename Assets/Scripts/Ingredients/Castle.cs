using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Castle : MonoBehaviour
{
    public int maxHealth;
    private bool feedbackHit = false;
    [SerializeField] private float feedbackDuration;
    private float timerFeedback = 0;
    [SerializeField] private Color feedbackColor;
    private Color baseColor;
    private Renderer rd;


    private void Start(){
        rd = GetComponent<Renderer>();
        baseColor = rd.material.color;
        GetComponent<HealthSystem>().Initialize(maxHealth);
        GetComponent<HealthSystem>().OnDeath += OnDeath;
        GetComponent<HealthSystem>().OnTakingDamage += OnHit;
    }

    void Update() {
        VisualFeedbackHit();
    }

    private void OnDeath(){
        rd.material.color = Color.black;
        EnnemiObjective.instance.Remove(this);
        Destroy(this);
    }

     private void OnHit(Vector3 knockback) {
        feedbackHit = true;
        timerFeedback = 0;
    }

    public Vector3 GetClosestPosition() {
        NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas);
        return hit.position;
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
}
